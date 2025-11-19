using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public interface IErpAccountPublicModelFactory
{
    Task<ErpAccountPublicSearchModel> PrepareErpAccountSearchModelAsync(
        ErpAccountPublicSearchModel searchModel
    );

    Task<ErpAccountPublicListModel> PrepareErpAccountListModelAsync(
        ErpAccountPublicSearchModel searchModel
    );

    Task<RecentTransactionListModel> PrepareRecentTransactionListAsync(
        RecentTransactionSearchModel transactionSearchModel
    );
    Task<RecentTransactionListModel> PrepareRecentTransactionListAsync(
        ErpAccountInfoModel erpAccountInfoModel
    );

    Task<ErpAccountInfoModel> PrepareErpAccountInfoModelAsync(
        ErpAccount b2BAccount,
        ErpAccountInfoModel model,
        bool enableErpAccountUpdate = false
    );

    Task<ErpAccountOrderSearchModel> PrepareErpAccountOrderSearchModelAsync(
        ErpAccount erpAccount,
        ErpNopUser erpNopUser,
        ErpAccountOrderSearchModel model
    );

    Task<ErpAccountOrderListModel> PrepareErpOrderListModelAsync(
        ErpAccountOrderSearchModel searchModel
    );

    Task<ErpAccountQuoteOrderSearchModel> PrepareErpAccountQuoteOrderSearchModelAsync(
        ErpAccount erpAccount,
        ErpNopUser erpNopUser,
        ErpAccountQuoteOrderSearchModel searchModel
    );

    Task<ErpQuoteOrderListModel> PrepareErpQuoteOrderListModelAsync(
        ErpAccountQuoteOrderSearchModel searchModel
    );
    Task PrepareTransactionSortingOptionsAsync(IList<SelectListItem> items);

    Task<byte[]> ExportB2BAccountProductsToXlsxAsync(int categoryId = 0);

    Task<byte[]> ExportB2BOrderPerAccountProductsToXlsxAsync();

    Task<byte[]> PrintB2BOrderPerAccountProductsToPdfAsync(ErpAccount erpAccount);
    Task<byte[]> PrintB2BAccountProductsToPdfAsync(ErpAccount erpAccount, int categoryId = 0);

    Task<ErpCustomerConfigurationModel> PrepareB2BCustomerConfigurationModelAsync(ErpAccount erpAccount, ErpCustomerConfigurationModel model);
    Task SetB2BCustomerConfigurationModelAsync(ErpAccount erpAccount, ErpCustomerConfigurationModel model);
    Task<ErpShipToAddressListModel> PrepareB2BShippingAddressListModel(ErpAccount erpAccount, ErpNopUser erpNopUser, ErpShipToAddressListModel model);
    Task<decimal?> GetB2BCurrentCustomerAccountSavingsForthisYearAsync(ErpAccount erpAccount);
    Task<decimal?> GetB2BCurrentCustomerAccountSavingsForAllTimeAsync(ErpAccount erpAccount);
    Task<decimal?> GetB2CCurrentCustomerAccountSavingsForthisYearAsync(ErpNopUser erpNopUser);
    Task<decimal?> GetB2CCurrentCustomerAccountSavingsForAllTimeAsync(ErpNopUser erpNopUser);
    Task<ErpBillingAddressModel> PrepareB2BBillingAddressModel(ErpAccount erpAccount, ErpNopUser erpNopUser, ErpBillingAddressModel erpBillingAddressModel);

    Task<ErpAccountInfoAjaxLoadModel> PrepareB2BAccountInfoAjaxLoadModelAsync(ErpAccount b2BAccount, Customer customer, bool enableErpAccountUpdate = false);
}
