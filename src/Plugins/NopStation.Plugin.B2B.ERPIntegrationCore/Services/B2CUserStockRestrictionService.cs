using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class B2CUserStockRestrictionService : IB2CUserStockRestrictionService
{
    #region fields

    private readonly IRepository<B2CUserStockRestriction> _b2CUserStockRestrictionRepository;

    #endregion

    #region Ctor

    public B2CUserStockRestrictionService(
        IRepository<B2CUserStockRestriction> b2CUserStockRestrictionRepository
    )
    {
        _b2CUserStockRestrictionRepository = b2CUserStockRestrictionRepository;
    }

    #endregion

    #region Methods

    public async Task<B2CUserStockRestriction> GetB2CUserStockRestrictionByUserIdProductIdAsync(
        int b2cUserId,
        int productId
    )
    {
        if (b2cUserId == 0)
            throw new ArgumentException(nameof(b2cUserId));

        if (productId == 0)
            throw new ArgumentException(nameof(productId));

        return await _b2CUserStockRestrictionRepository
            .Table.WhereAwait(async x => x.B2CUserId == b2cUserId && x.ProductId == productId)
            .FirstOrDefaultAsync();
    }

    public async Task InsertB2CUserStockRestrictionAsync(
        B2CUserStockRestriction b2CUserStockRestriction
    )
    {
        ArgumentNullException.ThrowIfNull(b2CUserStockRestriction);

        await _b2CUserStockRestrictionRepository.InsertAsync(b2CUserStockRestriction);
    }

    public async Task UpdateB2CUserStockRestrictionAsync(
        B2CUserStockRestriction b2CUserStockRestriction
    )
    {
        ArgumentNullException.ThrowIfNull(b2CUserStockRestriction);

        await _b2CUserStockRestrictionRepository.UpdateAsync(b2CUserStockRestriction);
    }

    #endregion
}
