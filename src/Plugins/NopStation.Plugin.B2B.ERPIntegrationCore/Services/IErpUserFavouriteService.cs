using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public interface IErpUserFavouriteService
{
    Task<ErpUserFavourite> GetErpUserFavouriteByErpNopUserIdAsync(int erpNopUserId);

    Task<IList<int>> GetErpUserFavouriteIdsByErpSalesRepCustomerIdAsync(int customerId);

    Task<ErpUserFavourite> GetErpUserFavouriteByCustomerIdAndErpNopUserIdAsync(int customerId, int erpNopUserId);

    Task InsertErpUserFavouriteAsync(ErpUserFavourite erpUserFavourite);

    Task UpdateErpUserFavouriteAsync(ErpUserFavourite erpUserFavourite);

    Task DeleteErpUserFavouriteAsync(ErpUserFavourite erpUserFavourite);
}
