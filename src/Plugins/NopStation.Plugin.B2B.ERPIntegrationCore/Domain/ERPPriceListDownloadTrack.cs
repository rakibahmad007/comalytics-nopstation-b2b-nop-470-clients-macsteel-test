using System;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public class ERPPriceListDownloadTrack : BaseEntity
{
    public int NopCustomerId { get; set; }
    public int B2BAccountId { get; set; }
    public int B2BSalesOrganisationId { get; set; }
    public DateTime DownloadedOnUtc { get; set; }
    public int PriceListDownloadTypeId { get; set; }
    public PriceListDownloadType PriceListDownloadType
    {
        get => (PriceListDownloadType)PriceListDownloadTypeId;
        set => PriceListDownloadTypeId = (int)value;
    }
}
