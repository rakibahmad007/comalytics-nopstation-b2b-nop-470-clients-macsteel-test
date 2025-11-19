using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/10/07 12:00:00", ".Misc.NopStation.ERPIntegrationCore base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    #region Methods

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        Create.TableFor<ErpSalesOrg>();
        Create.TableFor<ErpGroupPriceCode>();
        Create.TableFor<ErpAccount>();
        Create.TableFor<ErpLogs>();
        Create.TableFor<ErpGroupPrice>();
        Create.TableFor<ErpInvoice>();
        Create.TableFor<ErpShipToAddress>();
        Create.TableFor<ErpShiptoAddressErpAccountMap>();
        Create.TableFor<ErpNopUser>();
        Create.TableFor<ErpNopUserAccountMap>();
        Create.TableFor<ErpOrderAdditionalData>();
        Create.TableFor<ErpOrderItemAdditionalData>();
        Create.TableFor<ErpSalesRep>();
        Create.TableFor<ErpSalesRepSalesOrgMap>();
        Create.TableFor<ErpSalesRepErpAccountMap>();
        Create.TableFor<ErpSpecialPrice>();
        Create.TableFor<ErpWarehouseSalesOrgMap>();
        Create.TableFor<QuickOrderTemplate>();
        Create.TableFor<QuickOrderItem>();
        Create.TableFor<ErpActivityLogs>();
        Create.TableFor<ErpAccountCustomerRegistrationForm>();
        Create.TableFor<ErpAccountCustomerRegistrationBankingDetails>();
        Create.TableFor<ErpAccountCustomerRegistrationPhysicalTradingAddress>();
        Create.TableFor<ErpAccountCustomerRegistrationTradeReferences>();
        Create.TableFor<ErpAccountCustomerRegistrationPremises>(); 
        Create.TableFor<ErpProductNotePerSalesOrg>();
        Create.TableFor<ERPSAPErrorMsgTranslation>();
        Create.TableFor<ErpUserRegistrationInfo>();
        Create.TableFor<ErpCustomerConfiguration>();
        Create.TableFor<ERPPriceListDownloadTrack>();
        Create.TableFor<ErpShipToCodeChange>();
        Create.TableFor<SpecialIncludesAndExcludes>();
        Create.TableFor<ErpUserFavourite>();
        Create.TableFor<ERPProductCategoryMap>();
        Create.TableFor<B2CShoppingCartItem>();
        Create.TableFor<ErpCategoryImageShow>();
        Create.TableFor<B2CUserStockRestriction>();
        Create.TableFor<B2CMacsteelExpressShop>();
    }

    #endregion
}