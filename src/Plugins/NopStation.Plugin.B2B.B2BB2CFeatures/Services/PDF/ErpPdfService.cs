using System;
using System.IO;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Services.Localization;
using Nop.Services.Media;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories.PDF;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.PDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using static System.Net.Mime.MediaTypeNames;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.PDF;

public sealed class ErpPdfService : IErpPdfService
{
    private const string DATE_FORMAT = "dd/MM/yyyy";
    private readonly IErpPdfModelFactory _erpPdfModelFactory;
    private readonly ILocalizationService _localizationService;
    private readonly PdfSettings _pdfSettings;
    private readonly AddressSettings _addressSettings;
    private readonly IPictureService _pictureService;

    public ErpPdfService(
        IErpPdfModelFactory erpPdfModelFactory,
        ILocalizationService localizationService,
        PdfSettings pdfSettings,
        AddressSettings addressSettings,
        IPictureService pictureService
    )
    {
        _erpPdfModelFactory = erpPdfModelFactory;
        _localizationService = localizationService;
        _pdfSettings = pdfSettings;
        _addressSettings = addressSettings;
        _pictureService = pictureService;
    }

    public async Task GenerateOrderPdfAsync(Stream stream, Order order, Language language = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(order);

        var pageSize = PageSizes.A4;
        if (_pdfSettings.LetterPageSizeEnabled)
            pageSize = PageSizes.Letter;

        byte[] logoData = null;
        var logoPicture = await _pictureService.GetPictureByIdAsync(_pdfSettings.LogoPictureId);
        if (logoPicture != null)
        {
            var logoFilePath = await _pictureService.GetThumbLocalPathAsync(logoPicture, 0, false);
            if (!string.IsNullOrEmpty(logoFilePath) && File.Exists(logoFilePath))
            {
                logoData = await File.ReadAllBytesAsync(logoFilePath);
            }
        }


        var model = await _erpPdfModelFactory.PrepareErpOrderPdfModelAsync(order);
        var source = new ReceiptSource
        {
            Language = language,
            PageSize = pageSize,
            FontFamily = _pdfSettings.FontFamily,
            LogoData = logoData,
            OrderType = model.OrderSummaryPdfModel.OrderType,
            OrderNumber = model.OrderSummaryPdfModel.OrderNumber,
            CustomOrderNumber = model.OrderSummaryPdfModel.CustomOrderNumber,
            SalesOrgCode = model.SalesOrgPdfModel.SalesOrgCode,
            SalesOrgName = model.SalesOrgPdfModel.SalesOrgName,
            SalesOrgAddress = model.SalesOrgPdfModel.Address?.Address1,
            SalesOrgSuburb = model.SalesOrgPdfModel.Suburb,
            SalesOrgPostalCode = model.SalesOrgPdfModel.Address?.ZipPostalCode,
            SalesOrgCity = model.SalesOrgPdfModel.Address?.City,
            SalesOrgCountry = model.SalesOrgPdfModel.Address?.Country,
            PickupInStore = model.OrderSummaryPdfModel.PickupInStore,
            BillingAddress = new AddressModel
            {
                AccountNumber = model.ErpAccountPdfModel.AccountNumber,
                Name = model.BillingAddressPdfModel?.Name,
                PhoneNumber = model.BillingAddressPdfModel.PhoneNumber,
                Suburb = model.ErpAccountPdfModel.BillingSuburb,
                Address1 = model.BillingAddressPdfModel?.Address1,
                Address2 = model.BillingAddressPdfModel?.Address2,
                City = model.BillingAddressPdfModel.City,
                ZipPostalCode = model.BillingAddressPdfModel?.ZipPostalCode,
                StateProvince = model.BillingAddressPdfModel?.StateProvince,
                Country = model.BillingAddressPdfModel?.Country,
            },
            ShippingAddress = new AddressModel
            {
                ShipToCode = model.ShippingAddressPdfModel?.ShipToCode,
                Name = model.ShippingAddressPdfModel?.ShipToName,
                PhoneNumber = model.ShippingAddressPdfModel?.Address?.PhoneNumber,
                Suburb = model.ShippingAddressPdfModel.Suburb,
                Address1 = model.ShippingAddressPdfModel?.Address?.Address1,
                Address2 = model.ShippingAddressPdfModel?.Address?.Address2,
                City = model.ShippingAddressPdfModel?.Address?.City,
                ZipPostalCode = model.ShippingAddressPdfModel?.Address?.ZipPostalCode,
                StateProvince = model.ShippingAddressPdfModel?.Address?.StateProvince,
                Country = model.ShippingAddressPdfModel?.Address?.Country,
            },
            PickupAddress = new AddressModel
            {
                Name = model.PickupAddressPdfModel?.Name,
                PhoneNumber = model.PickupAddressPdfModel?.PhoneNumber,
                Address1 = model.PickupAddressPdfModel?.Address1,
                Address2 = model.PickupAddressPdfModel?.Address2,
                City = model.PickupAddressPdfModel?.City,
                ZipPostalCode = model.PickupAddressPdfModel?.ZipPostalCode,
                StateProvince = model.PickupAddressPdfModel?.StateProvince,
                Country = model.PickupAddressPdfModel?.Country,
            },
            CustomerName = model.ErpAccountPdfModel.AccountName,
            OrderDate = model.OrderSummaryPdfModel.OrderDate.ToString(DATE_FORMAT),
            DeliveryDate = model.OrderSummaryPdfModel.DeliveryDate.Value.ToString(DATE_FORMAT),
            SalesRepName = model.OrderSummaryPdfModel.SalesRepName,
            DeliverOrCollect = model.OrderSummaryPdfModel.PickupInStore ? "Collect" : "Deliver",
            CustomerReference = model.OrderSummaryPdfModel.CustomerReference,
            OrderItems = model.OrderItemPdfModelList,
            SpecialInstruction = model.OrderSummaryPdfModel.SpecialInstructions,
            OrderSubtotal = FormatPrice(model.OrderSummaryPdfModel.OrderSubtotal),
            ShippingCost = FormatPrice(model.OrderSummaryPdfModel.ShippingCost),
            TaxAmount = FormatPrice(model.OrderSummaryPdfModel.TaxAmount),
            CashRounding = model.OrderSummaryPdfModel.CashRounding,
            OrderTotal = FormatPrice(model.OrderSummaryPdfModel.OrderTotal),
            IsB2b = model.IsB2b
        };

        await using var pdfStream = new MemoryStream();
        new ReceiptDocument(source, _localizationService, _addressSettings).GeneratePdf(pdfStream);

        pdfStream.Position = 0;
        await pdfStream.CopyToAsync(stream);
    }

    private static string FormatPrice(decimal price) => $"R  {price:N2}";
}
