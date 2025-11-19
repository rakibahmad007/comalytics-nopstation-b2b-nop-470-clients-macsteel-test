using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.ErpWebhook;

public class ErpWebhookSettings : ISettings
{
    public string WebhookSecretKey { get; set; }
    
    public string DefaultCountryThreeLetterIsoCode { get; set; }

    #region Process already running

    public bool AccounthookAlreadyRunning { get; set; }
    public bool ShiptoAddresshookAlreadyRunning { get; set; }
    public bool ProducthookAlreadyRunning { get; set; }
    public bool StockhookAlreadyRunning { get; set; }
    public bool AccountPricinghookAlreadyRunning { get; set; }
    public bool OrderhookAlreadyRunning { get; set; }
    public bool CredithookAlreadyRunning { get; set; }
    public bool DeliveryDateshookAlreadyRunning { get; set; }
    public bool ProductsImagehookAlreadyRunning { get; set; }

    #endregion
}