using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.ErpWebhook.Domain;

namespace Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;

public interface IAllowedWebhookManagerIpAddressesService
{
    Task<List<AllowedWebhookManagerIpAddresses>> GetAllowedWebhookManagerIpAddressesAsync();
    Task<IPagedList<AllowedWebhookManagerIpAddresses>> GetAllIpAddressesAsync(int pageIndex = 0, int pageSize = int.MaxValue);
    Task DeleteIpAddressAsync(int id);
    Task AddIpAddressAsync(AllowedWebhookManagerIpAddresses ipAddress);
    Task<AllowedWebhookManagerIpAddresses> GetIpAddressByIpAdressAsync(string ipAddress);
    Task<AllowedWebhookManagerIpAddresses> GetIpAddressByIdAsync(int id);
    Task UpdateIpAddressAsync(AllowedWebhookManagerIpAddresses resource);
}
