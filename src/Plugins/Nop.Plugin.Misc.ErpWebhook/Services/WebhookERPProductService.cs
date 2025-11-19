using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpProduct;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Seo;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.ErpWebhook.Services
{
    public class WebhookERPProductService : IWebhookERPProductService
    {
        #region fields

        private Dictionary<string, int> _vendorDictionary;
        private ErpWebhookConfig _erpWebhookConfig = null;
        static string _mappingsFilePath = "~/Plugins/Misc.ErpWebhook/mappings.json";
        static string _connectionString = DataSettingsManager.LoadSettings().ConnectionString;

        System.Text.RegularExpressions.Regex _lineBreakReplacer = new System.Text.RegularExpressions.Regex(@"\r?\n");
        private readonly ILogger _logger;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly ErpWebhookSettings _erpWebhookSettings;
        private readonly IVendorService _vendorService;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IRepository<SpecificationAttribute> _specificationAttrRepository;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IRepository<SpecificationAttributeOption> _spAttrOptionRepository;
        private readonly IRepository<ProductSpecificationAttribute> _psaRepository;
        private readonly ICategoryService _categoryService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IRepository<ProductCategory> _pcRepository;
        private readonly IErpWebhookService _erpWebhookService;
        private readonly IWorkContext _workContext;
        private readonly IRepository<Parallel_ErpProduct> _erpProductRepo;

        #endregion

        #region ctor

        public WebhookERPProductService(ILogger logger,
            IRepository<Product> productRepository,
            IRepository<UrlRecord> urlRecordRepository,
            ErpWebhookSettings erpWebhookSettings,
            IVendorService vendorService,
            IRepository<Vendor> vendorRepository,
            IProductService productService,
            IUrlRecordService urlRecordService,
            IRepository<SpecificationAttribute> specificationAttrRepository,
            ISpecificationAttributeService specificationAttributeService,
            IRepository<SpecificationAttributeOption> spAttrOptionRepository,
            IRepository<ProductSpecificationAttribute> psaRepository,
            ICategoryService categoryService,
            CatalogSettings catalogSettings,
            IRepository<ProductCategory> pcRepository,
            IErpWebhookService erpWebhookService,
            IWorkContext workContext,
            IRepository<Parallel_ErpProduct> erpProductRepo)
        {
            _logger = logger;
            _productRepository = productRepository;
            _urlRecordRepository = urlRecordRepository;
            _erpWebhookSettings = erpWebhookSettings;
            _vendorService = vendorService;
            _vendorRepository = vendorRepository;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _specificationAttrRepository = specificationAttrRepository;
            _specificationAttributeService = specificationAttributeService;
            _spAttrOptionRepository = spAttrOptionRepository;
            _psaRepository = psaRepository;
            _categoryService = categoryService;
            _catalogSettings = catalogSettings;
            _pcRepository = pcRepository;
            _erpWebhookService = erpWebhookService;
            _workContext = workContext;
            _erpProductRepo = erpProductRepo;
        }

        #endregion

        #region utils

        public async Task<int> GetVendorIdAsync(string vendorName)
        {
            int vendorId = _erpWebhookConfig.DefaultProductsVendorId ?? 1;

            // Lookup vendor ID from the preloaded dictionary
            if (!string.IsNullOrEmpty(vendorName))
            {
                if (_vendorDictionary.ContainsKey(vendorName))
                {
                    vendorId = _vendorDictionary[vendorName];
                }
                else
                {
                    _logger.Information($"Vendor '{vendorName}' not found. Default id '{_erpWebhookConfig.DefaultProductsVendorId}' returned instead");
                }
            }

            // Simulate async behavior if no I/O operations exist in this method
            await Task.CompletedTask;

            return vendorId;
        }


        public string CleanStringForSlug(string istr)
        {
            if (istr == null)
                return "";
            if (istr.Length > 390)
            { istr = istr.Substring(0, 390); } // NVarchar(400)
            string rstr = "";
            foreach (char c in istr)
            {
                if (c >= '0' && c <= '9' ||
                    c >= 'a' && c <= 'z' ||
                    c >= 'A' && c <= 'Z' ||
                    c == ' ')
                    rstr += c.ToString();
            }

            rstr = rstr.Replace(" ", "-");
            rstr = rstr.Replace(".", "_");
            rstr = rstr.Replace(",", "_");
            rstr = rstr.Replace(";", "_");
            rstr = rstr.Replace(":", "_");
            rstr = rstr.Replace("'", "_");
            rstr = rstr.Replace("\" ", "_");
            rstr = rstr.Replace("\\", "_");
            rstr = rstr.Replace("/", "_");
            rstr = rstr.Replace("__", "_");
            rstr = rstr.Replace("___", "_");
            rstr = rstr.Replace("____", "_");

            if (rstr.EndsWith("_"))
                rstr = rstr.Substring(0, rstr.Length - 1);

            return rstr;
        }

        static Lazy<Dictionary<string, SpecificationAttributeMapping>> _specificationAttributeMappings
            = new Lazy<Dictionary<string, SpecificationAttributeMapping>>(() =>
            {
                var result = new Dictionary<string, SpecificationAttributeMapping>(StringComparer.InvariantCultureIgnoreCase);
                if (File.Exists(_mappingsFilePath))
                {
                    string json = File.ReadAllText(_mappingsFilePath);
                    var token = JObject.Parse(json);
                    var mappings = token["specificationAttributes"]?.ToObject<List<SpecificationAttributeMapping>>();
                    if (mappings != null)
                    {
                        result = mappings.ToDictionary(m => m.ErpAttribute);
                    }
                }
                return result;
            });

        public static Dictionary<string, SpecificationAttributeMapping> GetSpecificationAttributeMappings()
        {
            return _specificationAttributeMappings.Value;
        }

        /// <summary>
        /// Ensures each category in a category hierarchy branch exists in NOP,
        /// and returns their ids
        /// </summary>
        /// <param name="catnames">A list of category names, from parent to child</param>
        /// <returns>A list of NOP category ids, from parent to child</returns>
        private async Task<List<int>> CheckCategoryTreeERPAsync(List<string> catnames)
        {
            List<int> cats = new List<int>(0);
            int parentId = 0;

            foreach (string catName in catnames)
            {
                // Assume GetCategoryByParentIdAndNameAsync is an asynchronous method
                DataTable dtcat = await GetCategoryByParentIdAndNameAsync(parentId, catName);

                if (dtcat.Rows.Count > 0)
                {
                    DataRow row = dtcat.Rows[0];
                    if (!row.IsNull("ID"))
                        parentId = Convert.ToInt32(row["ID"]);
                    cats.Add(parentId);
                }
                else
                {
                    // Assume InsertCategoryAsync is an asynchronous method
                    parentId = await InsertCategoryAsync(catName, parentId);
                    cats.Add(parentId);
                }
            }

            _logger.Information($"Category NOP ids are {string.Join(", ", cats)}");

            return cats;
        }

        private void RemoveProductCategoriesExceptGivenAndSpecials(int productId, List<int> categoryIds)
        {
            var productCategoriesToRemove = _pcRepository.Table.Where(x => x.ProductId == productId && !categoryIds.Contains(x.CategoryId)).ToList();
            foreach (var item in productCategoriesToRemove)
            {
                _categoryService.DeleteProductCategoryAsync(item);
            }
        }

        private async Task<DataTable> GetCategoryByParentIdAndNameAsync(int ParentID, string Name)
        {
            try
            {
                string sql = "Select * from Category where ParentCategoryID = @ParentID and Name = @Name order by id";

                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    await conn.OpenAsync();
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@ParentID", ParentID);
                    cmd.Parameters.AddWithValue("@Name", Name);

                    using (SqlDataAdapter da = new SqlDataAdapter())
                    {
                        da.SelectCommand = cmd;

                        DataSet ds = new DataSet();
                        await Task.Run(() => da.Fill(ds)); // DataAdapter.Fill does not have an async version

                        DataTable dt = null;

                        if (ds.Tables.Count > 0)
                        {
                            dt = ds.Tables[0];

                            if (dt.Rows.Count > 0)
                            {
                                var row = dt.Rows[0];
                                if ((!row.IsNull("Deleted") && Convert.ToInt32(row["Deleted"]) != 0)
                                    || (!row.IsNull("Published") && Convert.ToInt32(row["Published"]) == 0))
                                {
                                    _logger.Information($"Undeleting category {Name} under parent {ParentID}");
                                    using (var cmd2 = conn.CreateCommand())
                                    {
                                        cmd2.CommandText = "update Category set Deleted = 0, Published = 1, CategoryTemplateId = @CategoryTemplateId where ParentCategoryID = @ParentID and Name = @Name";
                                        cmd2.Parameters.AddWithValue("@ParentID", ParentID);
                                        cmd2.Parameters.AddWithValue("@Name", Name);
                                        cmd2.Parameters.AddWithValue("@CategoryTemplateId", _erpWebhookConfig.CategoryTemplateId);
                                        await cmd2.ExecuteNonQueryAsync();
                                    }
                                }
                            }
                        }

                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("GetCategoryByParentIdAndNameAsync - " + ex.TargetSite + "  -  " + ex.Message);
                return null;
            }
        }

        private async Task<int> InsertCategoryAsync(string name, int parentID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand command = conn.CreateCommand())
                {
                    await conn.OpenAsync();

                    command.CommandText =
                            "INSERT INTO Category (" +
                            "[Name], " +
                            "[Description], " +
                            "CategoryTemplateId, " +
                            "MetaKeywords, " +
                            "MetaDescription, " +
                            "MetaTitle, " +
                            "ParentCategoryId, " +
                            "PictureId, PageSize, " +
                            "AllowCustomersToSelectPageSize, " +
                            "PageSizeOptions, " +
                            "ShowOnHomePage, " +
                            "IncludeInTopMenu, " +
                            "SubjectToAcl, " +
                            "LimitedToStores, " +
                            "Published, " +
                            "Deleted, " +
                            "DisplayOrder, " +
                            "CreatedOnUtc, " +
                            "UpdatedOnUtc, " +
                            "PriceRangeFiltering, " +
                            "PriceFrom, " +
                            "PriceTo, " +
                            "ManuallyPriceRange) " +
                        "VALUES (" +
                            "@Name, " +
                            "@Name, " +
                            "@CategoryTemplateId, " +
                            "@Name, " +
                            "@Name, " +
                            "@Name, " +
                            "@ParentID, " +
                            "0, " +
                            "@pageSize, " +
                            "1, " +
                            "@pageSizeOptions, " +
                            "0, " +
                            "1, " +
                            "0, " +
                            "0, " +
                            "1, " +
                            "0, " +
                            "0, " +
                            "GETDATE(), " +
                            "GETDATE(), " +
                            "0, " +
                            "0, " +
                            "0, " +
                            "0); " +
                        "SELECT SCOPE_IDENTITY();";

                    command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 400) { Value = name });
                    command.Parameters.Add(new SqlParameter("@ParentID", SqlDbType.Int) { Value = parentID });
                    command.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = _catalogSettings.DefaultCategoryPageSize });
                    command.Parameters.Add(new SqlParameter("@pageSizeOptions", SqlDbType.NVarChar, 200) { Value = _catalogSettings.DefaultCategoryPageSizeOptions });
                    command.Parameters.Add(new SqlParameter("@CategoryTemplateId", SqlDbType.Int) { Value = _erpWebhookConfig.CategoryTemplateId });

                    object result = await command.ExecuteScalarAsync();
                    int insertedCategoryId = Convert.ToInt32(result);

                    var urlRecord = await _urlRecordRepository.Table.FirstOrDefaultAsync(x => x.EntityId == insertedCategoryId && x.EntityName == "Category");
                    if (urlRecord == null)
                    {
                        var categoryCopy = await _categoryService.GetCategoryByIdAsync(insertedCategoryId);
                        if (categoryCopy != null)
                        {
                            string validatedSeName = await _urlRecordService.ValidateSeNameAsync(categoryCopy, string.Empty, name, false);
                            await _urlRecordService.InsertUrlRecordAsync(new UrlRecord { EntityId = insertedCategoryId, EntityName = "Category", Slug = validatedSeName });
                        }
                    }

                    return insertedCategoryId;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"InsertCategory - {ex.TargetSite} - {ex.Message}");
                return -1;
            }
        }
        private async Task<int> InsertProductCategoryMapAsync(int productID, int categoryID)
        {
            _logger.Information($"InsertProductCategoryMap(product={productID}, cat={categoryID})");

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    string sql = @"
                IF NOT EXISTS (SELECT * FROM Product_Category_Mapping WHERE ProductID = @ProductID AND CategoryID = @CategoryID)
                BEGIN
                    INSERT INTO Product_Category_Mapping (ProductId, CategoryId, IsFeaturedProduct, DisplayOrder)
                    VALUES (@ProductId, @CategoryId, 0, 0)
                END";

                    using (SqlCommand command = new SqlCommand(sql, conn))
                    {
                        command.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = productID });
                        command.Parameters.Add(new SqlParameter("@CategoryID", SqlDbType.Int) { Value = categoryID });

                        int rc = await command.ExecuteNonQueryAsync();
                        return rc;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"InsertProductCategoryMapAsync - {ex.TargetSite} - {ex.Message}");
                return -1;
            }
        }

        private async Task MapErpProductToNopProductAsync(Parallel_ErpProduct erpProduct, Product existingProduct)
        {
            bool isNewProduct = (existingProduct.Id <= 0);
            existingProduct.Sku = erpProduct.Sku;
            existingProduct.ManufacturerPartNumber = erpProduct.ManufacturerPartNumber;

            if (!string.IsNullOrWhiteSpace(erpProduct.ShortDescription))
            {
                if (erpProduct.ShortDescription.Length > 400)
                    erpProduct.ShortDescription = erpProduct.ShortDescription.Substring(0, 400);

                existingProduct.Name = erpProduct.ShortDescription;
            }

            if (!string.IsNullOrWhiteSpace(erpProduct.FullDescription))
            {
                string fullDescription = (erpProduct.FullDescription.Length > 400) ?
                    erpProduct.FullDescription.Substring(0, 400) : erpProduct.FullDescription;
                fullDescription = _lineBreakReplacer.Replace(fullDescription, "<br/>");
                existingProduct.FullDescription = fullDescription;
            }

            existingProduct.MetaTitle = erpProduct.ShortDescription.Replace("*", "");
            if (erpProduct.Height.HasValue)
                existingProduct.Height = erpProduct.Height.Value;
            if (erpProduct.Width.HasValue)
                existingProduct.Width = erpProduct.Width.Value;
            if (erpProduct.Length.HasValue)
                existingProduct.Length = erpProduct.Length.Value;
            if (erpProduct.Weight.HasValue)
                existingProduct.Weight = erpProduct.Weight.Value;
            if (erpProduct.SellingPriceA.HasValue)
                existingProduct.Price = erpProduct.SellingPriceA.Value;
            if (erpProduct.InStockforLocNo.HasValue)
                existingProduct.StockQuantity = (int)erpProduct.InStockforLocNo.Value;

            existingProduct.OrderMaximumQuantity = int.MaxValue;

            // Assume GetVendorIdAsync is the async version of GetVendorId
            existingProduct.VendorId = await GetVendorIdAsync(erpProduct.VendorName);

            existingProduct.UseMultipleWarehouses = true;
            existingProduct.VisibleIndividually = true;

            if (isNewProduct || (_erpWebhookConfig.Override_LowStockActivityId ?? false))
            {
                existingProduct.LowStockActivityId = _erpWebhookConfig.LowStockActivityId_DefaultValue ?? 0;
            }

            if (isNewProduct || (_erpWebhookConfig.Override_BackorderModeId ?? false))
            {
                existingProduct.BackorderModeId = _erpWebhookConfig.BackorderModeId_DefaultValue ?? 1;
            }

            existingProduct.IsShipEnabled = true;
            existingProduct.AdminComment = $"Updated by Product webhook (B2B) on {DateTime.UtcNow.ToString("u")}";
            existingProduct.UpdatedOnUtc = DateTime.UtcNow;
            existingProduct.Published = true;
            existingProduct.Deleted = false;
        }

        private void MapErpPorduct(Parallel_ErpProduct dbErpProduct, ErpProductModel updatedErpProduct)
        {
            dbErpProduct.Sku = updatedErpProduct.Sku;
            dbErpProduct.IsActive = updatedErpProduct.IsActive ?? true;
            dbErpProduct.IsDeleted = updatedErpProduct.IsDeleted ?? false;
            dbErpProduct.ManufacturerPartNumber = updatedErpProduct.ManufacturerPartNumber;
            dbErpProduct.ShortDescription = updatedErpProduct.ShortDescription;
            dbErpProduct.IsSpecial = updatedErpProduct.IsSpecial;
            dbErpProduct.FullDescription = updatedErpProduct.FullDescription;
            dbErpProduct.Height = updatedErpProduct.Height;
            dbErpProduct.Width = updatedErpProduct.Width;
            dbErpProduct.Length = updatedErpProduct.Length;
            dbErpProduct.Weight = updatedErpProduct.Weight;
            dbErpProduct.ManufacturerName = updatedErpProduct.ManufacturerName;
            dbErpProduct.ManufacturerDescription = updatedErpProduct.ManufacturerDescription;
            dbErpProduct.SellingPriceA = updatedErpProduct.SellingPriceA;
            dbErpProduct.InStockforLocNo = updatedErpProduct.InStockforLocNo;
            dbErpProduct.VendorName = updatedErpProduct.VendorName;
            dbErpProduct.CategoriesJson = JsonConvert.SerializeObject(updatedErpProduct.Categories);
            dbErpProduct.SpecificationAttributesJson = JsonConvert.SerializeObject(updatedErpProduct.SpecificastionAttributes);
            dbErpProduct.UpdatedOnUtc = DateTime.UtcNow;
        }
        private async Task UpdateProductSlugsAsync(IEnumerable<Product> products)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            List<int> productIds = products.Select(p => p.Id).ToList();

            try
            {
                const string entityName = "Product";

                // Loading existing records asynchronously
                var slugLookup = (await _urlRecordRepository.Table
                    .Where(s => s.EntityName == entityName && productIds.Contains(s.EntityId))
                    .ToListAsync())
                    .ToLookup(s => s.EntityId);

                _logger.Information($"{slugLookup.Count} existing url records loaded");

                // And make changes if needed
                foreach (Product product in products)
                {
                    string wantedSlug = CleanStringForSlug(product.Name);
                    _logger.Information($"Url record for product {product.Id} should be '{wantedSlug}'");

                    IEnumerable<UrlRecord> existingProductSlugs = slugLookup.Contains(product.Id) ?
                        slugLookup[product.Id] :
                        Enumerable.Empty<UrlRecord>();

                    var matchingExistingSlug = existingProductSlugs.FirstOrDefault(s => s.Slug.StartsWith(wantedSlug));
                    if (matchingExistingSlug != null)
                    {
                        // A matching slug exists...
                        if (matchingExistingSlug.IsActive)
                        {
                            // ...and is active: nothing to do
                            _logger.Information($"Url record for product {product.Id} is already '{wantedSlug}'");
                            continue;
                        }
                        else
                        {
                            // ...but it's not active. Make it active
                            _logger.Information("Marking existing url record as active");
                            matchingExistingSlug.IsActive = true;

                            // Deactivate previous active slug
                            var activeSlug = existingProductSlugs.FirstOrDefault(s => s.IsActive);
                            if (activeSlug != null)
                                activeSlug.IsActive = false;

                            await _urlRecordService.UpdateUrlRecordAsync(matchingExistingSlug);
                            if (activeSlug != null)
                                await _urlRecordService.UpdateUrlRecordAsync(activeSlug);
                        }
                    }
                    else
                    {
                        // No matching slug exists. Need to create a new one
                        string freeSlug = wantedSlug;
                        int i = 0;

                        while (true)
                        {
                            var test = await _urlRecordRepository.Table.FirstOrDefaultAsync(s => s.Slug == freeSlug);
                            if (test == null)
                            {
                                break;
                            }
                            _logger.Information($"Url record '{test.Slug}' already in use");
                            freeSlug = wantedSlug + "-" + i;
                            i++;
                        }

                        _logger.Information($"Adding url record for product {product.Id}, slug = {freeSlug}");
                        var newSlug = new UrlRecord
                        {
                            EntityId = product.Id,
                            EntityName = entityName,
                            Slug = freeSlug,
                            LanguageId = 0,
                            IsActive = true,
                        };

                        await _urlRecordService.InsertUrlRecordAsync(newSlug);

                        // Deactivate previous active slug
                        var activeSlug = existingProductSlugs.FirstOrDefault(s => s.IsActive);
                        if (activeSlug != null)
                        {
                            activeSlug.IsActive = false;
                            await _urlRecordService.UpdateUrlRecordAsync(activeSlug);
                        }
                    }
                }

                _logger.Information("Submitted url record updates");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to update slugs of products {string.Join(",", productIds)}: {ex.Message}", ex);
            }
        }

        private async Task UpdateProductSpecificationAttributesAsync(IEnumerable<Models.ErpProduct.ProductAttributeMapping> attributeMappings)
        {
            var config = GetSpecificationAttributeMappings();

            foreach (var mapping in attributeMappings)
            {
                int productId = mapping.ProductId;
                string attributeName = mapping.AttributeName;
                string attributeValue = mapping.AttributeValue;

                if (string.IsNullOrEmpty(attributeName) || string.IsNullOrEmpty(attributeValue))
                {
                    _logger.Information($"Skipping attribute '{attributeName}' of product {productId}, value '{attributeValue}'");
                    continue;
                }

                _logger.Information($"Processing attribute '{attributeName}' of product {productId}, value '{attributeValue}'");

                #region Load specification attribute metadata (from file)

                if (!config.TryGetValue(attributeName, out SpecificationAttributeMapping metadata))
                {
                    // If no mapping exists, we create a new one which maps from erp name to same nop name
                    metadata = new SpecificationAttributeMapping
                    {
                        ErpAttribute = attributeName,
                        NopAttribute = attributeName,
                        AllowFiltering = false,
                        ShowOnProductPage = false,
                        AttributeType = SpecificationAttributeType.CustomText,
                    };
                    config.Add(attributeName, metadata);
                    _logger.Information($"Will map ERP attribute '{attributeName}' to Nop attribute of same name");
                }
                System.Diagnostics.Debug.Assert(metadata != null);
                _logger.Information($"Mapping '{metadata.ErpAttribute}' to '{metadata.NopAttribute}', attr.type = {metadata.AttributeType}, allow filtering = {metadata.AllowFiltering}, show on homepage = {metadata.ShowOnProductPage}");

                #endregion

                #region Determine id of SpecificationAttribute row

                if (metadata.NopId == null)
                {
                    // If mapping exists but id hasn't been established yet, we first try to load from the database
                    var specAttr = await _specificationAttrRepository.Table
                        .FirstOrDefaultAsync(sa => sa.Name == attributeName);
                    if (specAttr == null)
                    {
                        // If it doesn't exist yet, we create it
                        specAttr = new SpecificationAttribute
                        {
                            DisplayOrder = 0,
                            Name = attributeName,
                        };

                        await _specificationAttributeService.InsertSpecificationAttributeAsync(specAttr);
                        _logger.Information($"Created new specification attribute '{attributeName}' (Id = {specAttr.Id})");
                    }
                    metadata.NopId = specAttr.Id;
                }
                System.Diagnostics.Debug.Assert(metadata.NopId != null);

                #endregion

                #region Determine id of SpecificationAttributeOption

                int? specAttrOptionId = null;
                if (metadata.AttributeType == SpecificationAttributeType.Option)
                {
                    #region If spec. attr. is of type Option, we need the id of the option with the given name
                    specAttrOptionId = await _spAttrOptionRepository.Table
                        .Where(a => a.SpecificationAttributeId == metadata.NopId && a.Name == attributeValue)
                        .Select(a => a.Id)
                        .FirstOrDefaultAsync();

                    if (specAttrOptionId == null || specAttrOptionId <= 0)
                    {
                        // if no such option exists we create it
                        var specAttrOption = new SpecificationAttributeOption
                        {
                            Name = attributeValue,
                            SpecificationAttributeId = metadata.NopId.Value
                        };
                        await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(specAttrOption);
                        specAttrOptionId = specAttrOption.Id;
                        _logger.Information($"Created option {specAttrOptionId} for value '{attributeValue}' of attribute '{attributeName}' (id={metadata.NopId})");
                    }
                    #endregion
                }
                else
                {
                    #region If spec. attr is NOT of type Option, we need the id of the "default" option

                    // (attribute value is stored in the mapping itself, updated below)
                    specAttrOptionId = await _spAttrOptionRepository.Table
                        .Where(o => o.SpecificationAttributeId == metadata.NopId)
                        .Select(o => o.Id)
                        .FirstOrDefaultAsync();

                    if (specAttrOptionId == null || specAttrOptionId <= 0)
                    {
                        // if no such option exists we create it
                        var specAttrOption = new SpecificationAttributeOption
                        {
                            Name = "default",
                            SpecificationAttributeId = metadata.NopId.Value
                        };
                        await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(specAttrOption);
                        specAttrOptionId = specAttrOption.Id;
                        _logger.Information($"Created new default option {specAttrOptionId} for value '{attributeValue}' of attribute '{attributeName}' (id={metadata.NopId})");
                    }

                    #endregion
                }

                System.Diagnostics.Debug.Assert(specAttrOptionId != null);
                _logger.Information($"Option id is {specAttrOptionId}");

                #endregion

                #region Finally, update product<->attribute mapping

                string customValue = (metadata.AttributeType == SpecificationAttributeType.Option) ? null : attributeValue;
                _logger.Information($"Checking mapping of product {productId} to attribute option '{specAttrOptionId}', custom value  '{customValue}'");

                ProductSpecificationAttribute psaMapping =
                    await (from m in _psaRepository.Table
                           join sao in _spAttrOptionRepository.Table on m.SpecificationAttributeOptionId equals sao.Id
                           where m.ProductId == productId && sao.SpecificationAttributeId == metadata.NopId
                           select m)
                    .FirstOrDefaultAsync();

                if (psaMapping != null)
                {
                    // update existing mapping...
                    psaMapping.SpecificationAttributeOptionId = specAttrOptionId.Value;
                    psaMapping.AttributeTypeId = metadata.AttributeTypeId.Value;
                    psaMapping.CustomValue = customValue;
                    psaMapping.AllowFiltering = metadata.AllowFiltering;
                    psaMapping.ShowOnProductPage = metadata.ShowOnProductPage;

                    await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(psaMapping);
                    _logger.Information($"Will update mapping {psaMapping.Id}");
                }
                else
                {
                    // ...or create new one
                    psaMapping = new ProductSpecificationAttribute()
                    {
                        ProductId = productId,
                        SpecificationAttributeOptionId = specAttrOptionId.Value,
                        AttributeTypeId = metadata.AttributeTypeId.Value,
                        AllowFiltering = metadata.AllowFiltering,
                        CustomValue = customValue,
                        ShowOnProductPage = metadata.ShowOnProductPage,
                    };
                    await _specificationAttributeService.InsertProductSpecificationAttributeAsync(psaMapping);
                    _logger.Information($"Created mapping {psaMapping.Id}");
                }

                #endregion Update mapping
            }
        }

        private async Task UpdateProductManufacturersAsync(IList<ProductManufacturerTuple> productManufacturers)
        {
            try
            {
                if ((productManufacturers?.Count ?? 0) < 1)
                {
                    return;
                }

                var manufCodesAndDescriptions = productManufacturers
                    .ToLookup(x => x.ManufacturerCode)
                    .ToDictionary(grp => grp.Key, grp => grp.First().ManufacturerDescription);

                var manufacturerIds = new Dictionary<string, int>(productManufacturers.Count());

                #region Get id of manufacturer (creating if necessary)
                foreach (KeyValuePair<string, string> pair in manufCodesAndDescriptions)
                {
                    _logger.Information($"Going to ensure manufacturer ('{pair.Key}', '{pair.Value}') exists");

                    using (var conn = new SqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();

                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                    UPDATE Manufacturer SET Deleted = 0 WHERE [Name] = @Name;
                    INSERT INTO Manufacturer
                    SELECT TOP 1 
                        @Name as [Name], 
                        @Description as [Description],
                        1 as ManufacturerTemplateId,
                        @MetaKeywords as MetaKeywords, 
                        @MetaDescription as MetaDescription, 
                        @MetaTitle as MetaTitle,
                        0 as PictureId, 
                        6 as PageSize,
                        1 as AllowCustomersToSelectPageSize,
                        '6, 3, 9' as PageSizeOptions,
                        0 as SubjectToAcl, 
                        0 as LimitedToStores,
                        1 as Published, 
                        0 as Deleted, 
                        0 as DisplayOrder,
                        GetDate() as CreatedOnUtc, 
                        GetDate() as UpdatedOnUtc,
                        0 as PriceRangeFiltering,
                        0 as PriceFrom,
                        0 as PriceTo,
                        0 as ManuallyPriceRange
                    FROM Manufacturer
                    WHERE NOT EXISTS (
                        SELECT 1 FROM Manufacturer WITH (UPDLOCK, SERIALIZABLE) WHERE [Name] = @Name
                    );
                    SELECT TOP 1 Id FROM Manufacturer WHERE [Name] = @Name ORDER BY Id;";

                            cmd.Parameters.AddWithValue("@Name", pair.Key);
                            cmd.Parameters.AddWithValue("@Description", pair.Value);
                            cmd.Parameters.AddWithValue("@MetaKeywords", pair.Key);
                            cmd.Parameters.AddWithValue("@MetaDescription", pair.Key);
                            cmd.Parameters.AddWithValue("@MetaTitle", pair.Key);

                            var result = new List<int>();
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    int mid = reader.GetInt32(0);
                                    result.Add(mid);
                                }
                            }

                            int id = result.FirstOrDefault();
                            manufacturerIds[pair.Key] = id;
                        }
                    }

                    _logger.Information($"Manufacturer ('{pair.Key}', '{pair.Value}') exists");
                }
                #endregion

                #region Ensure manufacturers' URLRecords
                foreach (KeyValuePair<string, string> pair in manufCodesAndDescriptions)
                {
                    int manufacturerId = manufacturerIds[pair.Key];
                    string slug = CleanStringForSlug(pair.Key);
                    _logger.Information($"Going to ensure slug '{slug}' exists for manufacturer ('{pair.Key}', '{pair.Value}') exists");

                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();

                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                    INSERT INTO URLRecord (EntityId, EntityName, Slug, IsActive, LanguageId)
                    SELECT TOP 1 @ManufacturerId, 'Manufacturer', @Slug, 1, 0
                    FROM URLRecord
                    WHERE NOT EXISTS (
                        SELECT 1 FROM URLRecord WITH (UPDLOCK, SERIALIZABLE)
                        WHERE EntityName = 'Manufacturer' AND EntityId = @ManufacturerId
                    );";

                            cmd.Parameters.AddWithValue("@ManufacturerId", manufacturerId);
                            cmd.Parameters.AddWithValue("@Slug", slug);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                #endregion

                #region Update product-manufacturer mapping
                foreach (ProductManufacturerTuple productManufacturer in productManufacturers)
                {
                    int manufacturerId = manufacturerIds[productManufacturer.ManufacturerCode];
                    _logger.Information($"Going to ensure mapping product with id {productManufacturer.ProductId} to manufacturer id {manufacturerId} ({productManufacturer.ManufacturerCode})");

                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();

                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                    INSERT INTO [Product_Manufacturer_Mapping]
                    (ProductId, ManufacturerId, IsFeaturedProduct, DisplayOrder)
                    SELECT TOP 1 @ProductId, @ManufacturerId, 0, 0
                    FROM [Product_Manufacturer_Mapping]
                    WHERE NOT EXISTS (
                        SELECT 1 FROM Product_Manufacturer_Mapping WITH (UPDLOCK, SERIALIZABLE)
                        WHERE ProductID = @ProductId AND ManufacturerID = @ManufacturerId
                    );";

                            cmd.Parameters.AddWithValue("@ProductId", productManufacturer.ProductId);
                            cmd.Parameters.AddWithValue("@ManufacturerId", manufacturerId);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in {nameof(UpdateProductManufacturersAsync)}: {ex.Message}");
            }
        }


        #endregion

        #region methods

        public async Task ProcessErpProductsAsync()
        {
            #region Insert/Update Products
            List<Parallel_ErpProduct> batch = _erpProductRepo.Table.Where(x => !x.IsUpdated).ToList();
            _vendorDictionary = _vendorRepository.Table.ToDictionary(v => v.Name, v => v.Id);
            _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();

            Dictionary<string, Parallel_ErpProduct> erpProducts = batch
                .GroupBy(p => p.Sku, StringComparer.InvariantCultureIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.InvariantCultureIgnoreCase);
            List<string> skus = erpProducts.Keys.ToList();
            _logger.Information($"Product webhook: processing new batch: {string.Join(",", skus)}");

            Dictionary<string, Product> existing = _productRepository.Table.Where(x => skus.Contains(x.Sku))
                .OrderBy(x => x.Deleted).ThenByDescending(x => x.Published).ThenBy(x => x.Id)
                .AsEnumerable().GroupBy(x => x.Sku.Trim(), StringComparer.InvariantCultureIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.InvariantCultureIgnoreCase);

            List<string> missingSkus = skus.Except(existing.Keys).ToList();
            foreach (var existingProduct in existing.Values)
            {
                if (!erpProducts.TryGetValue(existingProduct.Sku, out var erpProduct))
                {
                    _logger.Error($"Product Webhook: SKU '{existingProduct.Sku}' not found in products loaded from database.");
                    continue;
                }

                await MapErpProductToNopProductAsync(erpProduct, existingProduct);
            }

            foreach (var product in existing.Values)
            {
                await _productService.UpdateProductAsync(product);
            }

            _logger.Information($"Updated {string.Join(",", existing.Keys)}");
            if (missingSkus.Count > 0)
            {
                List<Product> fresh = erpProducts.Values
                    .Where(p => missingSkus.Contains(p.Sku))
                    .Select(erpProd =>
                    {
                        Product nopProd = new Product();
                        MapErpProductToNopProductAsync(erpProd, nopProd);
                        return nopProd;
                    })
                    .ToList();

                await _productRepository.InsertAsync(fresh);
                _logger.Information($"Inserting missing products {string.Join(",", missingSkus)}");

                var newlyInserted = _productRepository.Table.Where(x => missingSkus.Contains(x.Sku))
                    .AsEnumerable().GroupBy(p => p.Sku, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
                existing = existing.Concat(newlyInserted)
                    .GroupBy(kvp => kvp.Key, kvp => kvp.Value)
                    .ToDictionary(grp => grp.Key, grp => grp.Last());
            }

            #endregion

            #region InsertOrUpdateProductsSlug

            _logger.Information($"Updating url records for products {string.Join(",", existing.Values.Select(p => p.Sku))}");

            await UpdateProductSlugsAsync(existing.Values);

            #endregion

            #region Specification Attributes

            // Specification Attributes
            _logger.Information("Updating specification attributes");

            var attributeMappings = erpProducts.Values
                .Where(prd => prd.SpecificationAttributesJson != null)
                .SelectMany(prd => JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(prd.SpecificationAttributesJson)
                .Select(attr => new Models.ErpProduct.ProductAttributeMapping
                {
                    ProductId = existing[prd.Sku].Id,
                    AttributeName = attr.Key,
                    AttributeValue = attr.Value
                }));

            await UpdateProductSpecificationAttributesAsync(attributeMappings);

            #endregion

            #region Insert/Update Categories

            _logger.Information("Categories");
            foreach (var product in erpProducts.Values)
            {
                if (!existing.TryGetValue(product.Sku, out var erpProduct))
                {
                    _logger.Error($"SKU '{product.Sku}' not found in products loaded from database (categories step). WTF?");
                    continue;
                }

                List<string> categoryDescs = JsonConvert.DeserializeObject<List<ErpProductCategoryModel>>(product.CategoriesJson).Select(c => c.CategoryName).ToList();
                _logger.Information($"Categories of {product.Sku} are: {string.Join(",", categoryDescs)}");

                int anonymousCatCount = categoryDescs.RemoveAll(cn => string.IsNullOrWhiteSpace(cn));
                if (anonymousCatCount > 0)
                {
                    _logger.Warning($"Removed {anonymousCatCount} categories with no name from product {product.Sku}. Categories are {string.Join("; ", categoryDescs)}");
                }
                List<int> cids = await CheckCategoryTreeERPAsync(categoryDescs);

                // Delete old Product category links
                RemoveProductCategoriesExceptGivenAndSpecials(erpProduct.Id, cids);

                if (cids.Count > 0)
                {
                    // Create new relationship with the leaf category
                    await InsertProductCategoryMapAsync(erpProduct.Id, cids.Last());

                    foreach (var pair in categoryDescs.Zip(cids, (desc, cid) => new { desc, cid }))
                    {
                        var urlRecord = _urlRecordRepository.Table.Where(x => x.EntityId == pair.cid && x.EntityName == "Category");

                        if (urlRecord != null)
                        {
                            var categoryCopy = await _categoryService.GetCategoryByIdAsync(pair.cid);
                            if (categoryCopy != null)
                                await _urlRecordService.InsertUrlRecordAsync(new UrlRecord { EntityId = pair.cid, EntityName = "Category", Slug = await _urlRecordService.ValidateSeNameAsync(categoryCopy, string.Empty, pair.desc, false) });
                        }
                    }
                }
            }

            #endregion

            #region Brand / manufacturers

            var productBrands = erpProducts.Values
                .Where(x => !string.IsNullOrWhiteSpace(x.ManufacturerName))
                .Select(p =>
                {
                    return new ProductManufacturerTuple()
                    {
                        ProductId = existing[p.Sku].Id,
                        ManufacturerCode = p.ManufacturerName,
                        ManufacturerDescription = p.ManufacturerDescription,
                    };
                })
                .ToList();
            if (productBrands.Any())
            {
                _logger.Information($"Updating brands of {productBrands.Count} products");
                await UpdateProductManufacturersAsync(productBrands);
            }

            #endregion
        }

        public async Task ProcessErpProductsToParallelTableAsync(List<ErpProductModel> batch)
        {
            if (!batch.Any())
                return;

            var currentCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
            var productToAdd = new List<Parallel_ErpProduct>();

            // Check if similar entities exist in the database
            var existingErpProducts = from obj in batch
                                      join dbEntity in _erpProductRepo.Table
                                      on obj.Sku.Trim().ToLower() equals dbEntity.Sku.Trim().ToLower()
                                      select dbEntity;

            var existingSku = new List<string>();

            foreach (var dbErpProduct in existingErpProducts)
            {
                var updatedErpProduct = batch.Find(x => x.Sku.Equals(dbErpProduct.Sku));
                if (updatedErpProduct != null)
                {
                    MapErpPorduct(dbErpProduct, updatedErpProduct);
                    dbErpProduct.UpdatedById = currentCustomerId;
                }
            }

            if (existingErpProducts.Any())
            {
                // Looping through existingErpProducts to update each one
                foreach (var product in existingErpProducts)
                {
                    await _erpProductRepo.UpdateAsync(product);
                }

                existingSku.AddRange(existingErpProducts.Select(x => x.Sku).ToList());
            }

            var newErpProduct = batch.Where(x => !existingSku.Contains(x.Sku));

            foreach (var erpProductModel in newErpProduct)
            {
                var dbProduct = new Parallel_ErpProduct();
                MapErpPorduct(dbProduct, erpProductModel);
                // Common
                dbProduct.CreatedById = currentCustomerId;
                dbProduct.UpdatedById = currentCustomerId;
                dbProduct.CreatedOnUtc = DateTime.UtcNow;

                productToAdd.Add(dbProduct);
            }

            if (productToAdd.Any())
            {
                await _erpProductRepo.InsertAsync(productToAdd);
            }
        }

        public async Task<List<Parallel_ErpProduct>> GetErpProductsAsync(int skipCount, int batchSize)
        {
            return await _erpProductRepo.Table
                .Where(x => !x.IsUpdated)
                .OrderByDescending(x => x.Id)
                .Skip(skipCount)
                .Take(batchSize)
                .ToListAsync();
        }

        public async Task UpdateErpProductsAsync(List<Parallel_ErpProduct> erpProducts)
        {
            if (erpProducts == null)
                return;

            erpProducts.ForEach(x => x.IsUpdated = true);

            await _erpProductRepo.UpdateAsync(erpProducts);
        }

        #endregion
    }
}
