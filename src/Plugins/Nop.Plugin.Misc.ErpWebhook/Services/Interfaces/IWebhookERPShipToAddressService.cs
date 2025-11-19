using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpShipToAddress;

namespace Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;

public interface IWebhookERPShipToAddressService
{
    Task ProcessErpShipToAddressAsync(List<ErpShipToAddressModel> erpShipToAddress);
}
