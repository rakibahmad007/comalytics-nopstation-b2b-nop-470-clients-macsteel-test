using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Data;
using Nop.Plugin.Comalytics.DomainFilter.Domains;
using Nop.Plugin.Comalytics.DomainFilter.Models;
using Nop.Plugin.Comalytics.DomainFilter.Services;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Comalytics.DomainFilter.Factories
{
    public class DomainModelFactory : IDomainModelFactory
    {
        #region fields

        private readonly IDomainFilterExportImportService _domainFilterExportImportService;
        private readonly IDomainFilterService _domainFilterService;
        private readonly ILocalizationService _localizationService;
        private readonly INopDataProvider _nopDataProvider;

        #endregion

        #region Ctor

        public DomainModelFactory(
            IDomainFilterExportImportService domainFilterExportImportService,
            IDomainFilterService domainFilterService,
            ILocalizationService localizationService,
            INopDataProvider nopDataProvider)
        {
            _domainFilterExportImportService = domainFilterExportImportService;
            _domainFilterService = domainFilterService;
            _localizationService = localizationService;
            _nopDataProvider = nopDataProvider;
        }

        #endregion

        #region Methods

        public async Task<DomainListModel> PrepareDomainListModelAsync(DomainSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // Get parameters to filter the list
            var overrideActive = searchModel.SearchActiveId == 0 ? null : (bool?)(searchModel.SearchActiveId == 1);
            var searchTypeId = searchModel.SearchTypeId == 0 ? null : (int?)searchModel.SearchTypeId;

            // Get domains asynchronously
            var domains = await _domainFilterService.GetAllDomainsAsync(
                searchModel.SearchDomainOrEmailName,
                typeId: searchTypeId,
                overrideActive: overrideActive,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize
            );

            // Prepare list model
            var model = new DomainListModel();

            // Convert domains to models asynchronously
            var domainModels = await Task.WhenAll(domains.Select(async domain =>
            {
                var domainModel = domain.ToModel<DomainModel>();
                domainModel.Type = await _localizationService.GetLocalizedEnumAsync((DomainType)domain.TypeId);
                return domainModel;
            }));

            // Prepare the model for the grid
            model.PrepareToGrid(searchModel, domains, () => domainModels);
            return model;
        }

        public async Task<DomainModel> PrepareDomainModelAsync(DomainModel model, Domain domain)
        {
            if (domain != null)
            {
                model = model ?? domain.ToModel<DomainModel>();
                model.Type = await _localizationService.GetLocalizedEnumAsync(domain.Type);
            }
            else
            {
                model.IsActive = false;
            }

            model.AvailableTypeOptions.Add(new SelectListItem
            {
                Value = ((int)DomainType.Email).ToString(),
                Text = await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.DomainModel.Type.Email"),
            });
            model.AvailableTypeOptions.Add(new SelectListItem
            {
                Value = ((int)DomainType.Domain).ToString(),
                Text = await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.DomainModel.Type.Domain"),
            });

            return model;
        }


        public async Task<DomainSearchModel> PrepareDomainSearchModelAsync(DomainSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare "active" filter (0 - all; 1 - active only; 2 - inactive only)
            searchModel.AvailableActiveOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchActive.All"),
            });
            searchModel.AvailableActiveOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchActive.ActiveOnly"),
            });
            searchModel.AvailableActiveOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchActive.InactiveOnly"),
            });

            //prepare email/domain filter (0 - all; 1 - email; 2 - domain)
            searchModel.AvailableTypeOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchType.All"),
            });
            searchModel.AvailableTypeOptions.Add(new SelectListItem
            {
                Value = ((int)DomainType.Email).ToString(),
                Text = await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchType.Email"),
            });
            searchModel.AvailableTypeOptions.Add(new SelectListItem
            {
                Value = ((int)DomainType.Domain).ToString(),
                Text = await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchType.Domain"),
            });

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task ImportDomainListFromXlsx(Stream stream)
        {
            await _nopDataProvider.ExecuteNonQueryAsync("Truncate TABLE [dbo].[CP_DomainImport];");

            var totalRow = await _domainFilterExportImportService.WriteStreamInDatabaseAsync(stream, "CP_DomainImport");
            if (totalRow > 0)
            {
                await _nopDataProvider.ExecuteNonQueryAsync("EXEC CP_DomainImportProcedure;");
            }
        }

        private string GetCommonPartOfQuery()
        {
            var query = @"SELECT domainTable.[Id]
                          ,domainTable.[DomainOrEmailName]
                          ,domainTable.[TypeId]
                          ,domainTable.[IsActive]
                      FROM [dbo]." + DomainFilterDefaults.DomainOrEmailBlacklistTableName + @" domainTable";

            return query;
        }

        public async Task<byte[]> ExportDomainsToXlsx(List<int> ids)
        {
            var query = GetCommonPartOfQuery() + " WHERE domainTable.[Id] IN (" + string.Join(", ", ids) + ")";
            var fileBytesArray = await _domainFilterExportImportService.GetExcelPackageByQueryAsync(query, "DomainList");

            return fileBytesArray;
        }

        public async Task<byte[]> ExportDomainsToXlsx(DomainSearchModel searchModel)
        {
            var query = GetCommonPartOfQuery() + " WHERE domainTable.[Id] > 0";

            if (!string.IsNullOrWhiteSpace(searchModel.SearchDomainOrEmailName))
                query += " AND domainTable.[DomainOrEmailName] LIKE '%" + searchModel.SearchDomainOrEmailName + "%'";

            var searchTypeId = searchModel.SearchTypeId == 0 ? null : (int?)searchModel.SearchTypeId;
            if (searchTypeId != null)
                query += " AND domainTable.[TypeId] = " + searchTypeId;

            var overrideActive = searchModel.SearchActiveId == 0 ? null : (bool?)(searchModel.SearchActiveId == 1);
            if (overrideActive != null)
            {
                int v = (overrideActive ?? false) ? 1 : 0;
                query += " AND domainTable.[IsActive] = " + v;
            }

            // Call the method that now returns a byte[]
            var fileBytesArray = await _domainFilterExportImportService.GetExcelPackageByQueryAsync(query, "DomainList");

            return fileBytesArray;
        }

        #endregion
    }
}
