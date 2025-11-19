using System.IO;
using System.Threading.Tasks;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.PDF;

public interface IErpPdfService
{
    Task GenerateOrderPdfAsync(Stream stream, Order order, Language language = null);
}
