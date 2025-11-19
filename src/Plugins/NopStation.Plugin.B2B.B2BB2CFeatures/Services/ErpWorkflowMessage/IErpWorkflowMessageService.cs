using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;

public interface IErpWorkflowMessageService
{
    Task<int> SendERPOrderPlaceFailedSalesRepNotificationAsync(Order order, int languageId, ErpShipToAddress erpShipToAddress);
    Task<int> SendERPCustomerRegistrationApplicationCreatedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId);
    Task<int> SendERPCustomerRegistrationApplicationApprovedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId);
    Task<IList<int>> SendOrderOrDeliveryDatesOrShippingCostBAPIFailedMessageAsync(Customer customer, int failedType = 0, int nopOrderId = 0);
    Task SendCombinedLineItemWarningMessageAsync(int orderId, int originalQuoteOrderId, int languageId);

    Task<IList<int>> SendB2CCustomerWelcomeMessageAsync(Customer customer, 
        ErpAccount erpAccount, 
        ErpNopUser erpNopUser, 
        ErpSalesOrg erpSalesOrganization, 
        ErpShipToAddress erpShipToAddress, 
        int languageId);

    Task<IList<int>> SendB2CCustomerEmailVerificationMessageAsync(Customer customer, 
        ErpAccount erpAccount, 
        ErpNopUser erpNopUser, 
        ErpSalesOrg erpSalesOrg, 
        ErpShipToAddress erpShipToAddress, 
        int languageId);

    Task<List<int>> SendErpCustomerRegisteredNotificationMessageAsync(Customer customer, 
        ErpAccount erpAccount, 
        ErpNopUser erpNopUser, 
        ErpSalesOrg erpSalesOrg, 
        ErpShipToAddress erpShipToAddress, 
        int languageId, 
        string email, 
        ErpUserRegistrationInfo erpUserRegistrationInfo, 
        bool isB2bUser);
}