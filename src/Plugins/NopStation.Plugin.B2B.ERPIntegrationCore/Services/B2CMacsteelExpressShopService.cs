using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;
public class B2CMacsteelExpressShopService : IB2CMacsteelExpressShopService
{
    private readonly IRepository<B2CMacsteelExpressShop> _b2CMacsteelExpressShopRepository;

    public B2CMacsteelExpressShopService(IRepository<B2CMacsteelExpressShop> b2CMacsteelExpressShopRepositoryRepository)
    {
        _b2CMacsteelExpressShopRepository = b2CMacsteelExpressShopRepositoryRepository;
    }

    public async Task DeleteB2CMacsteelExpressShopAsync(B2CMacsteelExpressShop b2CMacsteelExpressShop)
    {
        ArgumentNullException.ThrowIfNull(b2CMacsteelExpressShop);

        await _b2CMacsteelExpressShopRepository.DeleteAsync(b2CMacsteelExpressShop);
    }

    public async Task<B2CMacsteelExpressShop> GetB2CMacsteelExpressShopByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _b2CMacsteelExpressShopRepository.GetByIdAsync(id);
    }
    public async Task<B2CMacsteelExpressShop> GetB2CMacsteelExpressShopByIdWithoutTrackingAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _b2CMacsteelExpressShopRepository.Table.FirstOrDefaultAsync(es => es.Id == id);
    }

    public async Task<B2CMacsteelExpressShop> GetB2CMacsteelExpressShopByCodeAsync(string expressShopCode)
    {
        if (string.IsNullOrEmpty(expressShopCode))
            return null;

        return await _b2CMacsteelExpressShopRepository.Table.FirstOrDefaultAsync(b => b.MacsteelExpressShopCode == expressShopCode);
    }

    public async Task InsertB2CMacsteelExpressShopAsync(B2CMacsteelExpressShop b2CMacsteelExpressShop)
    {
        ArgumentNullException.ThrowIfNull(b2CMacsteelExpressShop);

        await _b2CMacsteelExpressShopRepository.InsertAsync(b2CMacsteelExpressShop);
    }

    public async Task UpdateB2CMacsteelExpressShopAsync(B2CMacsteelExpressShop b2CMacsteelExpressShop)
    {
        ArgumentNullException.ThrowIfNull(b2CMacsteelExpressShop);

        await _b2CMacsteelExpressShopRepository.UpdateAsync(b2CMacsteelExpressShop);
    }

    public async Task<IPagedList<B2CMacsteelExpressShop>> GetAllB2CMacsteelExpressShopsAsync(string searchMacsteelExpressShopCode, string searchMacsteelExpressShopName, int pageIndex, int pageSize)
    {
        var query = _b2CMacsteelExpressShopRepository.Table;

        if (!string.IsNullOrEmpty(searchMacsteelExpressShopCode))
            query = query.Where(es => es.MacsteelExpressShopCode.Contains(searchMacsteelExpressShopCode));

        if (!string.IsNullOrEmpty(searchMacsteelExpressShopName))
            query = query.Where(es => es.MacsteelExpressShopName.Contains(searchMacsteelExpressShopName));

        var b2CMacsteelExpressShops = new PagedList<B2CMacsteelExpressShop>(await query.ToListAsync(), pageIndex, pageSize);

        return b2CMacsteelExpressShops;
    }

    public async Task<bool> CheckAnyB2CMacsteelExpressShopByCodeAsync(string code)
    {
        if (string.IsNullOrEmpty(code))
            return false;

        var query = from es in _b2CMacsteelExpressShopRepository.Table
                    select es;

        return await query.AnyAsync(es => es.MacsteelExpressShopCode == code);
    }
}
