using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpProductsImage;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Media;
using SkiaSharp;
using ImageHash = System.Numerics.BigInteger;


namespace Nop.Plugin.Misc.ErpWebhook.Services
{
    public class WebhookProductsImageService : IWebhookProductsImageService
    {
        #region fields

        private readonly ILogger _logger;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IRepository<ProductPicture> _productPictureRepo;
        private readonly IRepository<Parallel_CustomPictureBinaryForERP> _customPictureFromErpRepo;

        #endregion

        #region ctor

        public WebhookProductsImageService(ILogger logger,
            IPictureService pictureService,
            IProductService productService,
            IRepository<ProductPicture> productPictureRepo,
            IRepository<Parallel_CustomPictureBinaryForERP> customPictureFromErpRepo)
        {
            _logger = logger;
            _pictureService = pictureService;
            _productService = productService;
            _productPictureRepo = productPictureRepo;
            _customPictureFromErpRepo = customPictureFromErpRepo;
        }

        #endregion

        #region utils

        public static bool IsValidImage(byte[] bytes)
        {
            try
            {
                using (var ms = new MemoryStream(bytes))
                {
                    using var image = SkiaSharp.SKBitmap.Decode(ms);
                    if (image == null)
                        return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static string GetMimeTypeFromImageByteArray(byte[] byteArray)
        {
            try
            {
                using (var stream = new MemoryStream(byteArray))
                {
                    using (var skiaImage = SKBitmap.Decode(stream))
                    {
                        if (skiaImage == null)
                            return null;

                        using (var image = SKImage.FromBitmap(skiaImage))
                        {
                            var format = image.EncodedData;
                            return format.ToString().ToLower();
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public static System.Numerics.BigInteger MD5Hash(byte[] input, int from = 0, int to = int.MaxValue)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            return new System.Numerics.BigInteger(md5.ComputeHash(input, from, Math.Min(to, input.Length)));
        }

        #endregion

        #region methods

        public async Task ProcessProductsImageAsync(List<ErpProductsImageModel> erpProductsImage)
        {
            if (erpProductsImage == null)
            {
                return;
            }
            var productPictureMappings = new List<ProductPicture>();
            var customPictureBinaryForErps = new List<Parallel_CustomPictureBinaryForERP>();

            foreach (var item in erpProductsImage)
            {
                if (string.IsNullOrEmpty(item.Sku) || string.IsNullOrEmpty(item.ImageBase64))
                    continue;

                var product = await _productService.GetProductBySkuAsync(item.Sku);
                if (product == null)
                {
                    await _logger.InformationAsync($"Image webhook: provided sku {item.Sku} does not exist in the system");
                    continue;
                }

                byte[] imgdata = Convert.FromBase64String(item.ImageBase64);
                if (!IsValidImage(imgdata))
                {
                    await _logger.InformationAsync($"Image webhook: provided base64 data is not valid for sku: {item.Sku}");
                    continue;
                }

                var existingProductPictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
                var existingImageHash = new HashSet<ImageHash>();
                if (existingProductPictures != null)
                {
                    foreach (var pic in existingProductPictures)
                    {
                        var tempPicture = await _pictureService.GetPictureByIdAsync(pic.PictureId);
                        var binary = await _pictureService.LoadPictureBinaryAsync(tempPicture);
                        var hash = MD5Hash(binary, 0, 4000);
                        if (existingImageHash.Any(x => x.Equals(hash)))
                        {
                            //removing duplicate picture
                            var pictureId = pic.PictureId;
                            await _productService.DeleteProductPictureAsync(pic);

                            //try to get a picture with the specified id
                            var picture = await _pictureService.GetPictureByIdAsync(pictureId)
                                ?? throw new ArgumentException("No picture found with the specified id");

                            await _pictureService.DeletePictureAsync(picture);
                        }
                        else
                        {
                            existingImageHash.Add(hash);
                        }
                    }
                }

                var currentImageHash = MD5Hash(imgdata, 0, 4000);
                if (existingImageHash.Any(x => x.Equals(currentImageHash)))
                    continue;

                string mimetype = GetMimeTypeFromImageByteArray(imgdata);
                if (!string.IsNullOrEmpty(mimetype))
                {
                    var picture = await _pictureService.InsertPictureAsync(imgdata, mimetype, await _pictureService.GetPictureSeNameAsync(product.Name));
                    if (picture != null)
                    {
                        productPictureMappings.Add(new ProductPicture
                        {
                            PictureId = picture.Id,
                            ProductId = product.Id,
                            DisplayOrder = 0
                        });

                        customPictureBinaryForErps.Add(new Parallel_CustomPictureBinaryForERP
                        {
                            NopPictureId = picture.Id,
                            BinaryData = imgdata,
                            LastUpdatedOn = DateTime.UtcNow
                        });
                    }
                }
                else
                {
                    await _logger.InformationAsync($"Image webhook: Can't insert, mimetype is null (corrupted image) for sku: {item.Sku}");
                }
            }

            if (productPictureMappings.Any())
                await _productPictureRepo.InsertAsync(productPictureMappings);

            if (customPictureBinaryForErps.Any())
                await _customPictureFromErpRepo.InsertAsync(customPictureBinaryForErps);
        }

        public Parallel_CustomPictureBinaryForERP GetCustomPictureBinaryForERPByNopPictureId(int nopPictureId)
        {
            if (nopPictureId <= 0)
                return null;

            return _customPictureFromErpRepo.Table.FirstOrDefault(x => x.NopPictureId == nopPictureId);
        }

        public void DeleteCustomPictureBinaryForERP(Parallel_CustomPictureBinaryForERP customPictureBinary)
        {
            if (customPictureBinary != null)
            {
                _customPictureFromErpRepo.Delete(customPictureBinary);
            }
        }

        #endregion
    }
}
