using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using LinqToDB.DataProvider;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.ErpWebhook.Services
{
    public class OverridenPictureService : PictureService
    {
        #region Fields
        private readonly IDataProvider _dataProvider;
        private readonly IDownloadService _downloadService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INopFileProvider _fileProvider;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<PictureBinary> _pictureBinaryRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;
        private readonly IRepository<Parallel_CustomPictureBinaryForERP> _customProductPictureRepository;
        private readonly ISettingService _settingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly MediaSettings _mediaSettings;


        #endregion
        public OverridenPictureService(IDownloadService downloadService, IHttpContextAccessor httpContextAccessor, ILogger logger, INopFileProvider fileProvider, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, IRepository<Picture> pictureRepository, IRepository<PictureBinary> pictureBinaryRepository, IRepository<ProductPicture> productPictureRepository, ISettingService settingService, IUrlRecordService urlRecordService, IWebHelper webHelper, MediaSettings mediaSettings, IRepository<Parallel_CustomPictureBinaryForERP> customProductPictureRepository) : base(downloadService, httpContextAccessor, logger, fileProvider, productAttributeParser, productAttributeService, pictureRepository, pictureBinaryRepository, productPictureRepository, settingService, urlRecordService, webHelper, mediaSettings)
        {
            _downloadService = downloadService;
            _httpContextAccessor = httpContextAccessor;
            _fileProvider = fileProvider;
            _productAttributeParser = productAttributeParser;
            _pictureRepository = pictureRepository;
            _pictureBinaryRepository = pictureBinaryRepository;
            _productPictureRepository = productPictureRepository;
            _settingService = settingService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _mediaSettings = mediaSettings;
            _customProductPictureRepository = customProductPictureRepository;
        }


        #region Ctor



        #endregion

        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="picture">Picture</param>
        /// <param name="fromDb">Load from database; otherwise, from file system</param>
        /// <returns>Picture binary</returns>

        protected override async Task<byte[]> LoadPictureBinaryAsync(Picture picture, bool fromDb)
        {
            if (picture == null)
                throw new ArgumentNullException(nameof(picture));

            var pictureBinary = await GetPictureBinaryByPictureIdAsync(picture.Id);

            var result = _customProductPictureRepository.Table.FirstOrDefault(x => x.NopPictureId == picture.Id)?.BinaryData;

            if (result != null && result.Length > 0)
            {
                return result;
            }
            //var pictureBinary = await LoadPictureBinaryAsync(picture, fromDb);
            result = fromDb
                ? pictureBinary.BinaryData ?? Array.Empty<byte>() // If fromDb is true, get BinaryData or return empty array
                : await LoadPictureFromFileAsync(picture.Id, picture.MimeType); // If fromDb is false, load from file

            if (!fromDb && (result?.Length ?? 0) == 0 && pictureBinary?.BinaryData != null)
            {
                result = pictureBinary?.BinaryData ?? new byte[0];
            }

            return result;
        }
    }
}
