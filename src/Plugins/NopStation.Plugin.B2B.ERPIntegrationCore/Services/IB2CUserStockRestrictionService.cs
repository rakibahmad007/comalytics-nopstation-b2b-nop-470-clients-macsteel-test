using System.Threading.Tasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public interface IB2CUserStockRestrictionService
{
    Task<B2CUserStockRestriction> GetB2CUserStockRestrictionByUserIdProductIdAsync(
        int b2cUserId,
        int productId
    );
    Task InsertB2CUserStockRestrictionAsync(B2CUserStockRestriction b2CUserStockRestriction);
    Task UpdateB2CUserStockRestrictionAsync(B2CUserStockRestriction b2CUserStockRestriction);
}
