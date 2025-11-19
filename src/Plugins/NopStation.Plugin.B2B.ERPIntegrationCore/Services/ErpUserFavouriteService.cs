using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpUserFavouriteService : IErpUserFavouriteService
{
    private readonly IRepository<ErpUserFavourite> _erpUserFavouriteRepository;

    public ErpUserFavouriteService(IRepository<ErpUserFavourite> erpUserFavouriteRepository)
    {
        _erpUserFavouriteRepository = erpUserFavouriteRepository;
    }
    
    public async Task<ErpUserFavourite> GetErpUserFavouriteByErpNopUserIdAsync(int erpNopUserId)
    {
        if (erpNopUserId == 0)
            return null;

        return await _erpUserFavouriteRepository.GetByIdAsync(erpNopUserId);
    }

    public async Task<IList<int>> GetErpUserFavouriteIdsByErpSalesRepCustomerIdAsync(int customerId)
    {
        if (customerId == 0)
            return null;

        return await _erpUserFavouriteRepository.Table
            .Where(x => x.NopCustomerId == customerId)
            .Select(x => x.ErpNopUserId)
            .ToListAsync();
    }


    public async Task<ErpUserFavourite> GetErpUserFavouriteByCustomerIdAndErpNopUserIdAsync(int customerId, int erpNopUserId)
    {
        if (customerId == 0 || erpNopUserId == 0)
            return null;

        return await _erpUserFavouriteRepository.Table
            .FirstOrDefaultAsync(b => b.NopCustomerId == customerId && b.ErpNopUserId == erpNopUserId);
    }

    public async Task InsertErpUserFavouriteAsync(ErpUserFavourite erpUserFavourite)
    {
        ArgumentNullException.ThrowIfNull(erpUserFavourite);

        await _erpUserFavouriteRepository.InsertAsync(erpUserFavourite);
    }

    public async Task UpdateErpUserFavouriteAsync(ErpUserFavourite erpUserFavourite)
    {
        ArgumentNullException.ThrowIfNull(erpUserFavourite);

        await _erpUserFavouriteRepository.UpdateAsync(erpUserFavourite);
    }

    public async Task DeleteErpUserFavouriteAsync(ErpUserFavourite erpUserFavourite)
    {
        ArgumentNullException.ThrowIfNull(erpUserFavourite);

        await _erpUserFavouriteRepository.DeleteAsync(erpUserFavourite);
    }
}
