using System;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model.Common;
public class ErpProductImageDataModel
{
    public string Sku { get; set; }
    public byte[] ImageData { get; set; }
    public byte[] SpecData { get; set; }

    public ErpProductImageDataModel()
    {
        ImageData = Array.Empty<byte>();
        SpecData = Array.Empty<byte>();
    }
}

