using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
public class ErpNopUserExportImportModel
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string AccountNumber { get; set; }
    public string AccountName { get; set; }
    public string AccountSalesOrganisationCode { get; set; }
    public string ShipToCode { get; set; }
    public string ShipToName { get; set; }
    public string IsActive { get; set; }
    public string ErpUserType { get; set; }
}
