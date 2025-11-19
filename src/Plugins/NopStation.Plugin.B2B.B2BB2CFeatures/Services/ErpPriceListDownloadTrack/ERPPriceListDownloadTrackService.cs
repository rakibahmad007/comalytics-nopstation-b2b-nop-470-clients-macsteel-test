using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceListDownloadTrack
{
    public class ERPPriceListDownloadTrackService : IERPPriceListDownloadTrackService
    {
        private readonly IRepository<ERPPriceListDownloadTrack> _erpPriceListDownloadTrackRepository;

        public ERPPriceListDownloadTrackService(IRepository<ERPPriceListDownloadTrack> erpPriceListDownloadTrackRepository)
        {
            _erpPriceListDownloadTrackRepository = erpPriceListDownloadTrackRepository;
        }

        public async Task InsertB2BPriceListDownloadTrackAsync(ERPPriceListDownloadTrack b2BPriceListDownloadTrack)
        {
            if (b2BPriceListDownloadTrack == null)
                throw new ArgumentNullException(nameof(b2BPriceListDownloadTrack));

            await _erpPriceListDownloadTrackRepository.InsertAsync(b2BPriceListDownloadTrack);
        }

        public async Task<IPagedList<ERPPriceListDownloadTrack>> GetAllERPPriceListDownloadTrackAsync(
        int b2bAccountId = 0,
        int b2bSalesOrganisationId = 0,
        DateTime? downloadedFromUtc = null,
        DateTime? downloadedToUtc = null,
        int priceListDownloadTypeId = 0,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
        {
            var query = _erpPriceListDownloadTrackRepository.Table;

            if (b2bAccountId > 0)
                query = query.Where(b => b.B2BAccountId == b2bAccountId);
            if (b2bSalesOrganisationId > 0)
                query = query.Where(b => b.B2BSalesOrganisationId == b2bSalesOrganisationId);
            if (priceListDownloadTypeId > 0)
                query = query.Where(b => b.PriceListDownloadTypeId == priceListDownloadTypeId);
            if (downloadedFromUtc != null)
                query = query.Where(o => downloadedFromUtc.Value <= o.DownloadedOnUtc);
            if (downloadedToUtc != null)
                query = query.Where(o => downloadedToUtc.Value >= o.DownloadedOnUtc);

            query = query.OrderByDescending(b => b.Id);

            // Use the ToPagedListAsync method
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

    }
}
