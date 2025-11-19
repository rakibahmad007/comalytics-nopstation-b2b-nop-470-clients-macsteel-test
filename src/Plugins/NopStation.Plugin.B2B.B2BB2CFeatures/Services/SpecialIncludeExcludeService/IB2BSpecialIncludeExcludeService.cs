using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SpecialIncludeExcludes;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.SpecialIncludeExcludeService
{
    public interface IB2BSpecialIncludeExcludeService
    {
        Task<IPagedList<SpecialIncludeExcludeModel>> GetAllSpecialIncludeExcludesAsync(
        SpecialType? type = null,
        string accountName = "",
        string accountNumber = "",
        int ErpSalesOrgId = -1,
        bool? isActive = null,
        bool? published = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool showHidden = false);

        Task<SpecialIncludeExcludeModel> GetSpecialIncludeExcludeByIdAsync(int id);
        Task UpdateSpecialIncludeExcludeAsync(int id, bool isActive, byte mode = 0);
        Task DeleteSpecialIncludeExcludeByIdAsync(int id);
        Task DeleteSpecialIncludeExcludeByIdListAsync(ICollection<int> ids);
        Task<ImportResult> ImportSpecialIncludeExcludesFromXlsxAsync(Stream stream, SpecialType type);
        Task<byte[]> ExportSpecialIncludeExcludeToXlsxAsync(int type, int mode);
        Task<byte[]> ExportSpecialIncludeExcludeToXlsxAsync(ICollection<int> ids);
        Task<int> GetB2BSalesOrgIdByB2BAccountIdAsync(int b2BAccountId);
        Task AddSpecialIncludesAndExcludesAsync(SpecialIncludesAndExcludes entity);
        Task<SpecialIncludesAndExcludes> GetUniqueB2BSpecialIncludesAndExcludesAsync(SpecialIncludeExcludeModel model);
    }
}
