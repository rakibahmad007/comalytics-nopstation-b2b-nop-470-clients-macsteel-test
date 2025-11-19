using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic
{
    public record RecentTransactionModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.FinancialTransaction.Fields.PostingDate")]
        public DateTime PostingDate { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.FinancialTransaction.Fields.DocumentType")]
        public string DocumentType { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.FinancialTransaction.Fields.DocumentDisplayName")]
        public string DocumentDisplayName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.FinancialTransaction.Fields.DocumentNo")]
        public string DocumentNo { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.FinancialTransaction.Fields.Status")]
        public string Status { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.FinancialTransaction.Fields.Remaining")]
        public decimal Remaining { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.FinancialTransaction.Fields.AmountExVat")]
        public string AmountExVat { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.FinancialTransaction.Fields.CustomerOrder")]
        public string CustomerOrder { get; set; }

        public int NopOrderId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.FinancialTransaction.Fields.ErpOrderNumber")]
        public string ErpOrderNumber { get; set; }

        public bool IsDocumentTypeInvoice { get; set; }
        public bool IsDocumentTypeDownloadable { get; set; }

        //public static implicit operator RecentTransactionModel(RecentTransactionModel v)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
