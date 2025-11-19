using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpSalesRep;
public record SelectCustomerForErpSalesRepModel : BaseNopEntityModel
{
    public int SelectedCustomerId { get; set; }
}
