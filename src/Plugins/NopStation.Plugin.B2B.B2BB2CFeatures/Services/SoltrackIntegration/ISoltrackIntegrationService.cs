using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Soltrack;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.SoltrackIntegration;

public interface ISoltrackIntegrationService
{
    Task<(bool isCustomerInExpressShopZone, bool isCustomerOnDeliveryRoute, ClosestGeoEntityResult)> GetSoltrackResponseAsync(Customer customer, string latitude, string longitude);
}
