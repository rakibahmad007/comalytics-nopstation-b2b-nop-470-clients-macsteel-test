using System;
using System.Collections.Generic;
using Nop.Core.Domain.Common;
using Nop.Services.Common.Pdf;
using Nop.Services.Localization;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.PDF;

public sealed class ReceiptDocument : PdfDocument<ReceiptSource>
{
    private const float BORDER_THICKNESS = 0.5f;
    private const string PRIMARY_COLOR = "#1a3b83";
    private const int STANDARD_PADDING = 8;
    private const string INDENT = "   ";

    private readonly AddressSettings _addressSettings;

    public ReceiptDocument(
        ReceiptSource source,
        ILocalizationService localizationService,
        AddressSettings addressSettings
    )
        : base(source, localizationService)
    {
        _addressSettings = addressSettings;
    }

    public override void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(24);
            page.DefaultTextStyle(x => x.FontSize(9));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer()
                .AlignCenter()
                .Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            AddTitleBar(column);
            AddSupplierAndCustomerSection(column);
            AddBillingAndShippingOrPickupSection(column);
        });
    }

    private void AddTitleBar(ColumnDescriptor column)
    {
        column
            .Item()
            .Background(PRIMARY_COLOR)
            .Padding(STANDARD_PADDING)
            .Row(row =>
            {
                if (Source.LogoData is { Length: > 0 })
                {
                    row.AutoItem()
                        .AlignLeft()
                        .AlignMiddle()
                        .Height(13)
                        .Image(Source.LogoData, ImageScaling.FitArea);
                }
                else
                {
                    row.AutoItem()
                        .AlignMiddle()
                        .Text("MACSTEEL")
                        .FontSize(13)
                        .ExtraBold()
                        .FontColor(Colors.White);
                }

                var isSalesOrder =
                    Source.OrderType is ErpOrderType.B2BSalesOrder or ErpOrderType.B2CSalesOrder;
                var orderTypeResourceString = isSalesOrder
                    ? "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.SalesOrder"
                    : "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.QuoteOrder";
                var orderNumber = isSalesOrder ? Source.OrderNumber : Source.CustomOrderNumber;

                row.RelativeItem()
                    .AlignRight()
                    .AlignMiddle()
                    .Text(
                        $"{_localizationService.GetResourceAsync(orderTypeResourceString).Result}{INDENT}{orderNumber}"
                    )
                    .FontSize(10)
                    .Bold()
                    .FontColor(Colors.White);
            });
    }

    private void AddSupplierAndCustomerSection(ColumnDescriptor column)
    {
        column
            .Item()
            .Border(BORDER_THICKNESS)
            .BorderColor(Colors.Black)
            .Row(row =>
            {
                row.RelativeItem()
                    .BorderRight(BORDER_THICKNESS)
                    .BorderColor(Colors.Black)
                    .Padding(STANDARD_PADDING)
                    .Column(supplierColumn =>
                    {
                        AddSectionHeader(
                            supplierColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Supplier"
                                )
                                .Result
                        );

                        AddLabeledField(
                            supplierColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.SalesOrgCode"
                                )
                                .Result,
                            Source.SalesOrgCode
                        );
                        AddLabeledField(
                            supplierColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.SalesOrgName"
                                )
                                .Result,
                            Source.SalesOrgName
                        );
                        AddLabeledField(
                            supplierColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.SaledOrgAddress"
                                )
                                .Result,
                            Source.SalesOrgAddress
                        );
                        AddLabeledField(
                            supplierColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.SalesOrgSuburb"
                                )
                                .Result,
                            Source.SalesOrgSuburb
                        );
                        AddLabeledField(
                            supplierColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.SalesOrgCity"
                                )
                                .Result,
                            Source.SalesOrgCity
                        );
                        AddLabeledField(
                            supplierColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.SalesOrgPostalCode"
                                )
                                .Result,
                            Source.SalesOrgPostalCode
                        );
                        AddLabeledField(
                            supplierColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.SalesOrgCountry"
                                )
                                .Result,
                            Source.SalesOrgCountry
                        );
                    });

                row.RelativeItem()
                    .Padding(STANDARD_PADDING)
                    .Column(customerColumn =>
                    {
                        AddSectionHeader(
                            customerColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.AccountInfo"
                                )
                                .Result
                        );

                        AddLabeledField(
                            customerColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.CustomerName"
                                )
                                .Result,
                            Source.CustomerName
                        );
                    });
            });
    }

    private void AddBillingAndShippingOrPickupSection(ColumnDescriptor column)
    {
        column
            .Item()
            .Border(BORDER_THICKNESS)
            .BorderColor(Colors.Black)
            .Row(row =>
            {
                row.RelativeItem()
                    .BorderRight(BORDER_THICKNESS)
                    .BorderColor(Colors.Black)
                    .Padding(STANDARD_PADDING)
                    .Column(billingColumn =>
                    {
                        var address = Source.BillingAddress;

                        AddSectionHeader(
                            billingColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.BillingInformation"
                                )
                                .Result
                        );

                        AddLabeledField(
                            billingColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.InvoiceTo"
                                )
                                .Result,
                            address.AccountNumber
                        );
                        AddLabeledField(
                            billingColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Name"
                                )
                                .Result,
                            address.Name
                        );

                        if (_addressSettings.PhoneEnabled)
                        {
                            AddLabeledField(
                                billingColumn,
                                _localizationService
                                    .GetResourceAsync(
                                        "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Phone"
                                    )
                                    .Result,
                                address.PhoneNumber
                            );
                        }
                        AddLabeledField(
                            billingColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Email"
                                )
                                .Result,
                            address.Email
                        );
                        AddLabeledField(
                            billingColumn,
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Company"
                                )
                                .Result,
                            address.Company
                        );

                        if (_addressSettings.StreetAddressEnabled)
                        {
                            AddLabeledField(
                                billingColumn,
                                _localizationService
                                    .GetResourceAsync(
                                        "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Address"
                                    )
                                    .Result,
                                address.Address1
                            );
                        }

                        if (_addressSettings.StreetAddress2Enabled)
                        {
                            AddLabeledField(
                                billingColumn,
                                _localizationService
                                    .GetResourceAsync(
                                        "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Address2"
                                    )
                                    .Result,
                                address.Address2
                            );
                        }

                        if (!string.IsNullOrEmpty(address.Suburb)){
                            AddLabeledField(
                                billingColumn,
                                _localizationService
                                    .GetResourceAsync(
                                        "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Suburb"
                                    )
                                    .Result,
                                address.Suburb
                            );
                        }

                        if (
                            _addressSettings.CityEnabled
                            || _addressSettings.StateProvinceEnabled
                            || _addressSettings.ZipPostalCodeEnabled
                        )
                        {
                            billingColumn
                                .Item()
                                .PaddingLeft(8)
                                .Text(text =>
                                {
                                    var addressLine =
                                        $"{INDENT}{address.City}, "
                                        + $"{(!string.IsNullOrEmpty(address.StateProvince) ? $"{address.StateProvince}, " : string.Empty)}"
                                        + $"{address.ZipPostalCode}";

                                    text.Span(addressLine);

                                    if (
                                        _addressSettings.CountryEnabled
                                        && !string.IsNullOrEmpty(address.Country)
                                    )
                                    {
                                        text.EmptyLine();
                                        text.Span(INDENT + address.Country);
                                    }
                                });
                        }

                        if (Source.PickupInStore)
                        {
                            AddPickupSection(row);
                        }
                        else
                        {
                            AddShippingSection(row);
                        }
                    });
            });
    }

    private void AddPickupSection(RowDescriptor row)
    {
        row.RelativeItem()
            .Padding(STANDARD_PADDING)
            .Column(pickupColumn =>
            {
                var address = Source.PickupAddress;

                AddSectionHeader(
                    pickupColumn,
                    _localizationService
                        .GetResourceAsync(
                            "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.PickupPoint"
                        )
                        .Result
                );

                if (!string.IsNullOrEmpty(address.Address1))
                    AddLabeledField(
                        pickupColumn,
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Address"
                            )
                            .Result,
                        address.Address1
                    );

                if (!string.IsNullOrEmpty(address.City))
                    AddLabeledField(
                        pickupColumn,
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.City"
                            )
                            .Result,
                        address.City
                    );

                if (!string.IsNullOrEmpty(address.Country))
                    AddLabeledField(
                        pickupColumn,
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Country"
                            )
                            .Result,
                        address.Country
                    );

                if (!string.IsNullOrEmpty(address.ZipPostalCode))
                    AddLabeledField(
                        pickupColumn,
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.ZipPostalCode"
                            )
                            .Result,
                        address.ZipPostalCode
                    );
            });
    }

    private void AddShippingSection(RowDescriptor row)
    {
        row.RelativeItem()
            .Padding(STANDARD_PADDING)
            .Column(shippingColumn =>
            {
                var address = Source.ShippingAddress;

                AddSectionHeader(
                    shippingColumn,
                    _localizationService
                        .GetResourceAsync(
                            "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.ShippingInformatino"
                        )
                        .Result
                );

                AddLabeledField(
                    shippingColumn,
                    _localizationService
                        .GetResourceAsync(
                            "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.ShipToCode"
                        )
                        .Result,
                    address.ShipToCode
                );

                AddLabeledField(
                    shippingColumn,
                    _localizationService
                        .GetResourceAsync(
                            "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Name"
                        )
                        .Result,
                    address.Name
                );

                if (_addressSettings.PhoneEnabled)
                {
                    AddLabeledField(
                        shippingColumn,
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Phone"
                            )
                            .Result,
                        address.PhoneNumber
                    );
                }

                AddLabeledField(
                    shippingColumn,
                    _localizationService
                        .GetResourceAsync(
                            "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Email"
                        )
                        .Result,
                    address.Email
                );

                AddLabeledField(
                    shippingColumn,
                    _localizationService
                        .GetResourceAsync(
                            "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Company"
                        )
                        .Result,
                    address.Company
                );

                if (_addressSettings.StreetAddressEnabled)
                {
                    AddLabeledField(
                        shippingColumn,
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Address" //houseNumber
                            )
                            .Result,
                        address.Address1
                    );
                }

                if (_addressSettings.StreetAddress2Enabled)
                {
                    AddLabeledField(
                        shippingColumn,
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Address2" //street
                            )
                            .Result,
                        address.Address2
                    );
                }

                AddLabeledField(
                    shippingColumn,
                    _localizationService
                        .GetResourceAsync(
                            "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.Address.Suburb"
                        )
                        .Result,
                    address.Suburb
                );

                if (
                    _addressSettings.CityEnabled
                    || _addressSettings.StateProvinceEnabled
                    || _addressSettings.ZipPostalCodeEnabled
                )
                {
                    shippingColumn
                        .Item()
                        .PaddingLeft(8)
                        .Text(text =>
                        {
                            var addressLine =
                                $"{INDENT}{address.City}, "
                                + $"{(!string.IsNullOrEmpty(address.StateProvince) ? $"{address.StateProvince}, " : string.Empty)}"
                                + $"{address.ZipPostalCode}";

                            text.Span(addressLine);

                            if (
                                _addressSettings.CountryEnabled
                                && !string.IsNullOrEmpty(address.Country)
                            )
                            {
                                text.EmptyLine();
                                text.Span(INDENT + address.Country);
                            }
                        });
                }
            });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(10);
            AddOrderDetailsTable(column);

            column.Item().PaddingTop(10);
            AddOrderItemsTable(column);

            column.Item().PaddingTop(10);
            AddFooterSection(column);
        });
    }

    private void AddOrderDetailsTable(ColumnDescriptor column)
    {
        column
            .Item()
            .Border(BORDER_THICKNESS)
            .BorderColor(Colors.Black)
            .Table(table =>
            {
                List<(string, string)> orderDetails =
                [
                    (
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderDetails.OrderDate"
                            )
                            .Result,
                        Source.OrderDate
                    ),
                    (
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderDetails.DeliveryDate"
                            )
                            .Result,
                        Source.DeliveryDate
                    ),
                    (
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderDetails.CustomerReference"
                            )
                            .Result,
                        Source.CustomerReference
                    ),
                    (
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderDetails.SalesRepName"
                            )
                            .Result,
                        Source.SalesRepName
                    ),
                    (
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderDetails.DeliverOrCollect"
                            )
                            .Result,
                        Source.DeliverOrCollect
                    ),
                ];

                table.ColumnsDefinition(columns =>
                {
                    for (var i = 0; i < orderDetails.Count; i++)
                        columns.RelativeColumn();
                });

                foreach (var (header, _) in orderDetails)
                {
                    AddOrderDetailHeaderCell(table, header);
                }

                foreach (var (_, value) in orderDetails)
                {
                    AddOrderDetailValueCell(table, value);
                }
            });
    }

    private void AddOrderItemsTable(ColumnDescriptor column)
    {
        column
            .Item()
            .Border(BORDER_THICKNESS)
            .BorderColor(Colors.Black)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80); // Material
                    columns.RelativeColumn(3); // Name
                    columns.RelativeColumn(1); // Notes
                    columns.ConstantColumn(60); // Price
                    columns.ConstantColumn(40); // Qty
                    columns.ConstantColumn(40); // UOM
                    columns.ConstantColumn(80); // Price (Ex VAT) Net
                });

                table.Header(header =>
                {
                    List<string> headers =
                    [
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderItem.Material"
                            )
                            .Result,
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderItem.ProductName"
                            )
                            .Result,
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderItem.Notes"
                            )
                            .Result,
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderItem.Price"
                            )
                            .Result,
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderItem.Quantity"
                            )
                            .Result,
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderItem.UnitOfMeasure"
                            )
                            .Result,
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderItem.NetTotal"
                            )
                            .Result,
                    ];

                    foreach (var headerText in headers)
                    {
                        AddTableHeaderCell(header, headerText);
                    }
                });

                foreach (var item in Source.OrderItems)
                {
                    AddTableDataCell(table, item.Sku);
                    AddTableDataCell(table, item.ProductName);
                    AddTableDataCell(table, item.ErpOrderLineNotes);
                    AddTableDataCell(table, item.PriceFormatted, HorizontalAlignment.Right);
                    AddTableDataCell(table, item.Quantity.ToString(), HorizontalAlignment.Right);
                    AddTableDataCell(table, item.UnitOfMeasure ?? "EA", HorizontalAlignment.Center);
                    AddTableDataCell(table, item.NetTotalFormatted, HorizontalAlignment.Right);
                }
            });
    }

    private void AddFooterSection(ColumnDescriptor column)
    {
        column
            .Item()
            .Border(BORDER_THICKNESS)
            .BorderColor(Colors.Black)
            .Row(row =>
            {
                AddSpecialRequestsBox(row);
                AddTotalsSection(row);
            });
    }

    private void AddSpecialRequestsBox(RowDescriptor row)
    {
        row.RelativeItem(2)
            .BorderRight(BORDER_THICKNESS)
            .BorderColor(Colors.Black)
            .Padding(STANDARD_PADDING)
            .Column(specialRequestsColumn =>
            {
                specialRequestsColumn
                    .Item()
                    .Text(
                        _localizationService
                            .GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.SpecialInstruction"
                            )
                            .Result
                    )
                    .FontColor(PRIMARY_COLOR)
                    .Bold();

                specialRequestsColumn
                    .Item()
                    .PaddingTop(5)
                    .MinHeight(60)
                    .Text(Source.SpecialInstruction)
                    .FontSize(8);
            });
    }

    private void AddTotalsSection(RowDescriptor row)
    {
        row.RelativeItem()
            .Padding(STANDARD_PADDING)
            .Column(rightCol =>
            {
                rightCol
                    .Item()
                    .AlignRight()
                    .Text(text =>
                    {
                        text.Span(
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderSubtotal"
                                )
                                .Result + INDENT
                        );

                        text.Span(Source.OrderSubtotal);
                    });

                rightCol
                    .Item()
                    .AlignRight()
                    .Text(text =>
                    {
                        text.Span(
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.ShippingCost"
                                )
                                .Result + INDENT
                        );

                        text.Span(Source.ShippingCost);
                    });

                if (!Source.IsB2b)
                { 
                    rightCol
                        .Item()
                        .AlignRight()
                        .Text(text =>
                        {
                            text.Span(
                                _localizationService
                                    .GetResourceAsync(
                                        "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.CashRounding"
                                    )
                                    .Result + INDENT
                            );

                            text.Span(Source.CashRounding);
                        });
                }

                rightCol
                    .Item()
                    .AlignRight()
                    .Text(text =>
                    {
                            text.Span(
                            _localizationService
                                .GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.TaxAmount"
                                )
                                .Result + INDENT
                            );
                        
                        text.Span(Source.TaxAmount);
                    });

                rightCol
                    .Item()
                    .AlignRight()
                    .Text(text =>
                    {
                        text.Span(
                                _localizationService
                                    .GetResourceAsync(
                                        "Plugin.Misc.NopStation.B2BB2CFeatures.ReceiptDocument.OrderTotal"
                                    )
                                    .Result + INDENT
                            )
                            .FontColor(PRIMARY_COLOR);

                        text.Span(Source.OrderTotal).FontColor(PRIMARY_COLOR);
                    });
            });
    }

    private static void AddSectionHeader(ColumnDescriptor column, string headerText)
    {
        column.Item().Text(headerText).FontColor(PRIMARY_COLOR).Bold();
    }

    private static void AddLabeledField(ColumnDescriptor column, string label, string value)
    {
        if (string.IsNullOrWhiteSpace(label))
            return;

        column
            .Item()
            .PaddingLeft(8)
            .Text(text =>
            {
                text.Span($"{label}{INDENT}").Bold();
                text.Span(value ?? string.Empty);
            });
    }

    private static void AddTableHeaderCell(TableCellDescriptor header, string text)
    {
        header
            .Cell()
            .Background(Colors.Grey.Lighten2)
            .Border(BORDER_THICKNESS)
            .BorderColor(Colors.Black)
            .Padding(STANDARD_PADDING)
            .Text(text)
            .Bold();
    }

    private static void AddOrderDetailHeaderCell(TableDescriptor table, string text)
    {
        table
            .Cell()
            .Border(BORDER_THICKNESS)
            .BorderColor(Colors.Black)
            .Padding(8)
            .AlignCenter()
            .Text(text)
            .FontColor(PRIMARY_COLOR)
            .Bold();
    }

    private static void AddOrderDetailValueCell(TableDescriptor table, string text)
    {
        table
            .Cell()
            .BorderRight(BORDER_THICKNESS)
            .BorderBottom(BORDER_THICKNESS)
            .BorderColor(Colors.Black)
            .Padding(STANDARD_PADDING)
            .AlignCenter()
            .Text(text);
    }

    private static void AddTableDataCell(
        TableDescriptor table,
        string text,
        HorizontalAlignment alignment = HorizontalAlignment.Left
    )
    {
        Func<IContainer, IContainer> alignmentOption = alignment switch
        {
            HorizontalAlignment.Left => AlignmentExtensions.AlignLeft,
            HorizontalAlignment.Right => AlignmentExtensions.AlignRight,
            HorizontalAlignment.Center => AlignmentExtensions.AlignCenter,
            _ => AlignmentExtensions.AlignCenter,
        };

        alignmentOption(
                table
                    .Cell()
                    .BorderRight(BORDER_THICKNESS)
                    .BorderBottom(BORDER_THICKNESS)
                    .BorderColor(Colors.Black)
                    .Padding(5)
            )
            .Text(text);
    }
}
