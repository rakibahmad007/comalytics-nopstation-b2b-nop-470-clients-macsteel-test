using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpDeliveryDates;

namespace Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;

public interface IWebhookDeliveryDatesService
{
    Task ProcessDeliveryDatesAsync(List<DeliveryDatesModel> erpDeliveryDates);
}
