using System.Collections.Generic;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpDeliveryDates;

namespace Nop.Plugin.Misc.ErpWebhook.Utils;

public class ErpDeliveryDatesComparer : IEqualityComparer<DeliveryDatesModel>
{
    public bool Equals(DeliveryDatesModel x, DeliveryDatesModel y)
    {
        return x.SalesOrgOrPlant == y.SalesOrgOrPlant
            && x.City == y.City; // Add other properties for comparison
    }

    public int GetHashCode(DeliveryDatesModel obj)
    {
        return obj.SalesOrgOrPlant.GetHashCode()
            ^ obj.City.GetHashCode().GetHashCode(); // Include other properties in the hash code
    }
}
