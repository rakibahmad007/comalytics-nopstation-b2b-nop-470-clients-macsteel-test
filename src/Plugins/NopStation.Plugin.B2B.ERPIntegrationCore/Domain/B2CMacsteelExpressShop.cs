using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public class B2CMacsteelExpressShop : BaseEntity
{
    public string MacsteelExpressShopName { get; set; }
    public string MacsteelExpressShopCode { get; set; }
    public string Message { get; set; }
}