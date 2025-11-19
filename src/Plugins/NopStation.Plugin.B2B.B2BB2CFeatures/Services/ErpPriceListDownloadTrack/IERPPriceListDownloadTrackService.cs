using System;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceListDownloadTrack
{
    public interface IERPPriceListDownloadTrackService
    {
        Task<IPagedList<ERPPriceListDownloadTrack>> GetAllERPPriceListDownloadTrackAsync(
            int b2bAccountId = 0,
            int b2bSalesOrganisationId = 0,
            DateTime? downloadedFromUtc = null,
            DateTime? downloadedToUtc = null,
            int priceListDownloadTypeId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        Task InsertB2BPriceListDownloadTrackAsync(ERPPriceListDownloadTrack b2BPriceListDownloadTrack);
    }
}
