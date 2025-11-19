using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpShipToAddress;
public class ErpShipToAddressExportImportModel
{
    public string Id { get; set; }
    public string ShipToCode { get; set; }
    public string ShipToName { get; set; }
    public string Company { get; set; }
    public string Country { get; set; }
    public string StateProvince { get; set; }
    public string City { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Suburb { get; set; }
    public string ZipPostalCode { get; set; }
    public string PhoneNumber { get; set; }
    public string DeliveryNotes { get; set; }
    public string EmailAddresses { get; set; }
    public string AccountNumber { get; set; }
    public string AccountSalesOrganisationCode { get; set; }
    public string IsActive { get; set; }

}
