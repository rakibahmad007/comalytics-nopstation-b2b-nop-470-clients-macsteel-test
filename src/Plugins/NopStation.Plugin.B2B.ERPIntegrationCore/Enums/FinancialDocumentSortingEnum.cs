using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

public enum FinancialDocumentSortingEnum
{
    Position = 0,
    DocumentNumberAsc = 5,
    DocumentNumberDesc = 6,
    TransactionDateAsc = 10,
    TransactionDateDesc = 11,
    AmountExclVatAsc = 12,
    AmountExclVatDesc = 13
}
