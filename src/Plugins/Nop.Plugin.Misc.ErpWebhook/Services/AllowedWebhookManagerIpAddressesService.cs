using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Domain;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;

namespace Nop.Plugin.Misc.ErpWebhook.Services;

public class AllowedWebhookManagerIpAddressesService : IAllowedWebhookManagerIpAddressesService
{
    private readonly IRepository<AllowedWebhookManagerIpAddresses> _ipAddressRepo;

    public AllowedWebhookManagerIpAddressesService(IRepository<AllowedWebhookManagerIpAddresses> ipAddressRepo)
    {
        _ipAddressRepo = ipAddressRepo;
    }

    public async Task<IPagedList<AllowedWebhookManagerIpAddresses>> GetAllIpAddressesAsync(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        // Asynchronously execute the query and retrieve the list
        var query = _ipAddressRepo.Table;

        query = query.OrderByDescending(b => b.Id);

        // Execute the query asynchronously and convert to list
        IList<AllowedWebhookManagerIpAddresses> list = await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

        // Now create a PagedList from the list
        var ipAddresses = new PagedList<AllowedWebhookManagerIpAddresses>(list, pageIndex, pageSize);

        return ipAddresses;
    }

    public async Task<List<AllowedWebhookManagerIpAddresses>> GetAllowedWebhookManagerIpAddressesAsync()
    {
        return await _ipAddressRepo.Table.ToListAsync();
    }

    public async Task<AllowedWebhookManagerIpAddresses> GetIpAddressByIdAsync(int id)
    {
        return await _ipAddressRepo.GetByIdAsync(id);
    }

    public async Task<AllowedWebhookManagerIpAddresses> GetIpAddressByIpAdressAsync(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return null;

        return await _ipAddressRepo.Table
                                   .Where(x => x.IpAddress.Equals(ipAddress))
                                   .FirstOrDefaultAsync();
    }

    public async Task UpdateIpAddressAsync(AllowedWebhookManagerIpAddresses resource)
    {
        if (resource != null)
            await _ipAddressRepo.UpdateAsync(resource);
    }
    public async Task AddIpAddressAsync(AllowedWebhookManagerIpAddresses ipAddress)
    {
        if (ipAddress != null)
            await _ipAddressRepo.InsertAsync(ipAddress);
    }

    public async Task DeleteIpAddressAsync(int id)
    {
        if (id > 0)
        {
            var ipAddress = await _ipAddressRepo.GetByIdAsync(id);
            if (ipAddress != null)
                await _ipAddressRepo.DeleteAsync(ipAddress);
        }
    }
}
