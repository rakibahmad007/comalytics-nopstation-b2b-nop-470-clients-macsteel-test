using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.PDF;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories.PDF;

public interface IErpPdfModelFactory
{
    Task<OrderPdfModel> PrepareErpOrderPdfModelAsync(Order order);
}