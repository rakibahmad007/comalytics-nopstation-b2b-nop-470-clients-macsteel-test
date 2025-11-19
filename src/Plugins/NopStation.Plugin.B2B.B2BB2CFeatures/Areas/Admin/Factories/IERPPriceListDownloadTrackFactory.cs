using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ERPPriceListDownloadTracks;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public interface IERPPriceListDownloadTrackFactory
    {
        Task<ErpPriceListListModel> PrepareERPPriceListListModelAsync(ErpPriceListSearchModel searchModel);
        Task<ErpPriceListSearchModel> PrepareERPPriceListSearchModelAsync(ErpPriceListSearchModel searchModel);
        Task<byte[]> ExportERPPriceListDownloadToXlsxAsync(ErpPriceListSearchModel searchModel);
        Task<byte[]> ExportERPPriceListDownloadToXlsxAsync(List<int> ids);
    }
}
