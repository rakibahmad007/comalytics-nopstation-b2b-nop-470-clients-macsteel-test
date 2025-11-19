using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ErpWebhook.Models.Credit;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpAccount;

namespace Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;

public interface IWebhookERPAccountService
{
    Task ProcessErpAccountsAsync(IEnumerable<WebhookErpAccountModel> accounts);
    Task ProcessCreditsAsync(IEnumerable<Credit> credits);
}
