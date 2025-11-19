using System;
using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public class B2CShoppingCartItem : BaseEntity
{
    public int ShoppingCartItemId { get; set; }
    public int NopWarehouseId { get; set; }
    public string WarehouseCode { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string SpecialInstructions { get; set; }
}
