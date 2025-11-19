using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;
public interface IB2CMacsteelExpressShopService
{
    Task<bool> CheckAnyB2CMacsteelExpressShopByCodeAsync(string code);
    Task InsertB2CMacsteelExpressShopAsync(B2CMacsteelExpressShop b2CMacsteelExpressShop);
    Task UpdateB2CMacsteelExpressShopAsync(B2CMacsteelExpressShop b2CMacsteelExpressShop);
    Task DeleteB2CMacsteelExpressShopAsync(B2CMacsteelExpressShop b2CMacsteelExpressShop);
    Task<B2CMacsteelExpressShop> GetB2CMacsteelExpressShopByIdAsync(int id);
    Task<B2CMacsteelExpressShop> GetB2CMacsteelExpressShopByIdWithoutTrackingAsync(int id);
    Task<B2CMacsteelExpressShop> GetB2CMacsteelExpressShopByCodeAsync(string expressShopCode);
    Task<IPagedList<B2CMacsteelExpressShop>> GetAllB2CMacsteelExpressShopsAsync(string searchMacsteelExpressShopCode, string searchMacsteelExpressShopName, int pageIndex, int pageSize);
}
