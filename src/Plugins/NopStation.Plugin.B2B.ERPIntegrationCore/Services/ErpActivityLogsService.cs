using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public partial class ErpActivityLogsService : IErpActivityLogsService
{
    #region Fields

    private readonly IRepository<ErpActivityLogs> _erpActivityLogRepository;
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;
    private readonly ICustomerService _customerService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpSalesRepService _erpSalesRepService;
    private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
    private readonly IGenericAttributeService _genericAttributeService;

    #endregion

    #region Ctor

    public ErpActivityLogsService(
        IRepository<ErpActivityLogs> erpActivityLogRepository,
        IWebHelper webHelper,
        IWorkContext workContext,
        ILocalizationService localizationService,
        ICustomerService customerService,
        IErpAccountService erpAccountService,
        IErpShipToAddressService erpShipToAddressService,
        IErpSalesOrgService erpSalesOrgService,
        IErpSalesRepService erpSalesRepService,
        IErpGroupPriceCodeService erpGroupPriceCodeService,
        IGenericAttributeService genericAttributeService
    )
    {
        _erpActivityLogRepository = erpActivityLogRepository;
        _workContext = workContext;
        _localizationService = localizationService;
        _customerService = customerService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpSalesRepService = erpSalesRepService;
        _erpGroupPriceCodeService = erpGroupPriceCodeService;
        _genericAttributeService = genericAttributeService;
        _erpShipToAddressService = erpShipToAddressService;
    }

    #endregion

    #region Methods

    private async Task<string> GetFormattedEntityInfoAsync(BaseEntity entity)
    {
        var entityInfo = string.Empty;

        if (entity is null)
            return entityInfo;

        string replaceNewLine(string text) => text?.Replace(@"\n", Environment.NewLine);

        if (entity is Customer customer)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.NopCustomer"
                    )
                ),
                customer.Id,
                customer.Email
            );

        if (entity is ErpActivityLogs erpActivityLog)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BActivityLog"
                    )
                ),
                erpActivityLog.Id,
                (ErpActivityType)erpActivityLog.ErpActivityLogTypeId
            );

        if (entity is ErpAccount erpAccount)
            return string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BAccount"
                    )
                ),
                erpAccount.Id,
                erpAccount.AccountNumber
            );
        else if (entity is ErpNopUser erpNopUser)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BUserInformation"
                    )
                ),
                erpNopUser.Id,
                await GetCustomerEmailAsync(erpNopUser.NopCustomerId)
            );
        else if (entity is ErpShipToAddress erpShipToAddress)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BShipToAddress"
                    )
                ),
                erpShipToAddress.Id,
                erpShipToAddress.ShipToCode
            );
        else if (entity is ErpSalesOrg erpSalesOrganisation)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BSalesOrganisation"
                    )
                ),
                erpSalesOrganisation.Id,
                erpSalesOrganisation.Code
            );
        else if (entity is ErpOrderAdditionalData erpOrderAdditionalData)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BOrderPerAccount"
                    )
                ),
                erpOrderAdditionalData.Id,
                erpOrderAdditionalData.ErpOrderNumber
            );
        else if (entity is ErpSpecialPrice erpSpecialPrice)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BPerAccountProductPricing"
                    )
                ),
                erpSpecialPrice.Id,
                await GetFormattedPropertyValuesAsync(
                    nameof(erpSpecialPrice.ErpAccountId),
                    erpSpecialPrice.ErpAccountId.ToString()
                ),
                erpSpecialPrice.NopProductId
            );
        else if (entity is ErpGroupPriceCode erpGroupPriceCode)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BPriceGroupCode"
                    )
                ),
                erpGroupPriceCode.Id,
                erpGroupPriceCode.Code
            );
        else if (entity is ErpGroupPrice erpGroupPrice)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BPriceGroupProductPricing"
                    )
                ),
                erpGroupPrice.Id,
                erpGroupPrice.NopProductId,
                await GetErpGroupPriceCodeNameAsync(erpGroupPrice.ErpNopGroupPriceCodeId)
            );
        else if (entity is ErpProductNotePerSalesOrg erpProductNotePerSalesOrg)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BProductNotePerSalesOrg"
                    )
                ),
                erpProductNotePerSalesOrg.Id,
                await GetFormattedPropertyValuesAsync(
                    nameof(erpProductNotePerSalesOrg.SalesOrgId),
                    erpProductNotePerSalesOrg.SalesOrgId.ToString()
                ),
                erpProductNotePerSalesOrg.ProductId
            );
        else if (entity is B2BSalesOrgPickupPoint b2BSalesOrgPickupPoint)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BSalesOrgPickupPoint"
                    )
                ),
                b2BSalesOrgPickupPoint.Id,
                b2BSalesOrgPickupPoint.NopPickupPointId,
                await GetErpSalesOrgCodeAsync(b2BSalesOrgPickupPoint.B2BSalesOrgId)
            );
        else if (entity is ErpWarehouseSalesOrgMap erpWarehouseSalesOrgMap)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BSalesOrgWarehouse"
                    )
                ),
                erpWarehouseSalesOrgMap.Id,
                erpWarehouseSalesOrgMap.WarehouseCode
            );
        else if (entity is ErpSalesRep erpSalesRep)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BSalesRep"
                    )
                ),
                erpSalesRep.Id,
                await GetCustomerEmailAsync(erpSalesRep.NopCustomerId)
            );
        else if (entity is ErpSalesRepSalesOrgMap erpSalesRepSalesOrgMap)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BSalesRepSalesOrg"
                    )
                ),
                erpSalesRepSalesOrgMap.Id,
                await GetFormattedPropertyValuesAsync(
                    nameof(erpSalesRepSalesOrgMap.ErpSalesRepId),
                    erpSalesRepSalesOrgMap.ErpSalesRepId.ToString()
                )
            );
        else if (entity is ErpSalesRepErpAccountMap erpSalesRepErpAccountMap)
            entityInfo = string.Format(
                replaceNewLine(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.B2BSalesRepMultiAccountShipto"
                    )
                ),
                erpSalesRepErpAccountMap.Id,
                await GetFormattedPropertyValuesAsync(
                    nameof(erpSalesRepErpAccountMap.ErpSalesRepId),
                    erpSalesRepErpAccountMap.ErpSalesRepId.ToString()
                )
            );

        return entityInfo;
    }

    private async Task<string> GetCustomerEmailAsync(int customerId)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        return customer?.Email;
    }

    private async Task<string> GetFormattedPropertyValuesAsync(
        string propertyName,
        string propertyValue
    )
    {
        if (propertyValue == null)
            return string.Empty;

        if (propertyName.Contains("CustomerId", StringComparison.OrdinalIgnoreCase))
        {
            int.TryParse(propertyValue, out var customerId);
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            return await GetFormattedEntityInfoAsync(customer);
        }

        if (propertyName.Contains("ErpAccountId", StringComparison.OrdinalIgnoreCase))
        {
            int.TryParse(propertyValue, out var erpAccountId);
            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpAccountId);
            return await GetFormattedEntityInfoAsync(erpAccount);
        }

        if (
            propertyName.Contains("SalesOrganisationId", StringComparison.OrdinalIgnoreCase)
            || propertyName.Contains("SalesOrgId", StringComparison.OrdinalIgnoreCase)
        )
        {
            int.TryParse(propertyValue, out var salesOrgId);
            var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(salesOrgId);
            return await GetFormattedEntityInfoAsync(salesOrg);
        }

        if (propertyName.Contains("ErpShipToAddressId", StringComparison.OrdinalIgnoreCase))
        {
            int.TryParse(propertyValue, out var shipToAddressId);
            var shipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(
                shipToAddressId
            );
            return await GetFormattedEntityInfoAsync(shipToAddress);
        }

        if (propertyName.Contains("ErpSalesRepId", StringComparison.OrdinalIgnoreCase))
        {
            int.TryParse(propertyValue, out var salesRepId);
            var salesRep = await _erpSalesRepService.GetErpSalesRepByIdAsync(salesRepId);
            return await GetCustomerEmailAsync(salesRep?.NopCustomerId ?? 0);
        }

        if (propertyName.Contains("SalesRepTypeId", StringComparison.OrdinalIgnoreCase))
        {
            int.TryParse(propertyValue, out var salesRepTypeId);
            return ((SalesRepType)salesRepTypeId).ToString();
        }

        if (propertyName.Contains("ActivityTypeId", StringComparison.OrdinalIgnoreCase))
        {
            int.TryParse(propertyValue, out var activityTypeId);
            return ((ErpActivityType)activityTypeId).ToString();
        }

        return propertyValue?.ToString();
    }

    private async Task<string> GetErpGroupPriceCodeNameAsync(int erpGroupPriceCodeId)
    {
        var erpGroupPriceCode = await _erpGroupPriceCodeService.GetErpGroupPriceCodeByIdAsync(
            erpGroupPriceCodeId
        );
        return erpGroupPriceCode?.Code;
    }

    private async Task<string> GetErpSalesOrgCodeAsync(int erpSalesOrgId)
    {
        var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpSalesOrgId);
        return erpSalesOrg?.Code;
    }

    private async Task AppendErpAccountInfoAsync(
        ErpActivityLogs erpActivityLog,
        ErpActivityType activityType,
        string oldValue = null,
        string newValue = null
    )
    {
        if (activityType == ErpActivityType.Insert)
        {
            int.TryParse(newValue, out var shipToAddressId);
            var shipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(
                shipToAddressId
            );
            var erpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(
                shipToAddress
            );
            var erpAccountId = erpAccount?.Id ?? 0;

            erpActivityLog.NewValue +=
                "\\n"
                + await GetFormattedPropertyValuesAsync("ErpAccountId", erpAccountId.ToString());
        }
        else if (activityType == ErpActivityType.Delete)
        {
            int.TryParse(newValue, out var shipToAddressId);
            var shipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(
                shipToAddressId
            );
            var erpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(
                shipToAddress
            );
            var erpAccountId = erpAccount?.Id ?? 0;

            erpActivityLog.OldValue +=
                "\\n"
                + await GetFormattedPropertyValuesAsync("ErpAccountId", erpAccountId.ToString());
        }
        else if (activityType == ErpActivityType.Update)
        {
            int.TryParse(newValue, out var oldShipToAddressId);
            var oldShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(
                oldShipToAddressId
            );
            var oldErpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(
                oldShipToAddress
            );
            var oldErpAccountId = oldErpAccount?.Id ?? 0;

            int.TryParse(newValue, out var newShipToAddressId);
            var newShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(
                newShipToAddressId
            );
            var newErpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(
                newShipToAddress
            );
            var newErpAccountId = oldErpAccount?.Id ?? 0;

            erpActivityLog.NewValue +=
                "\\n"
                + await GetFormattedPropertyValuesAsync("B2BAccountId", oldErpAccountId.ToString());
            erpActivityLog.OldValue +=
                "\\n"
                + await GetFormattedPropertyValuesAsync("B2BAccountId", newErpAccountId.ToString());
        }
    }

    /// <summary>
    /// Inserts an erp activity log item
    /// </summary>
    /// <param name="activityLog">Erp Activity log</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    private async Task InsertErpActivityLogAsync(ErpActivityLogs erpActivityLog)
    {
        ArgumentNullException.ThrowIfNull(erpActivityLog);

        await _erpActivityLogRepository.InsertAsync(erpActivityLog);
    }

    public async Task InsertErpActivityLogAsync(
        BaseEntity updatedEntity,
        ErpActivityType activityType,
        BaseEntity databaseCopyOfEntity = null
    )
    {
        if (updatedEntity == null)
            throw new ArgumentNullException(nameof(updatedEntity));

        if (activityType == ErpActivityType.Update && databaseCopyOfEntity == null)
            throw new ArgumentNullException(nameof(databaseCopyOfEntity));

        var type = updatedEntity.GetType();
        var propertyInfoList = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var propertyNamesToIgnore = new List<string>
        {
            "Id",
            "CreatedOnUtc",
            "CreatedById",
            "UpdatedOnUtc",
            "UpdatedById",
        };
        var propertyTypesToAllow = new List<Type>
        {
            typeof(DateTime),
            typeof(decimal),
            typeof(string),
        };
        var erpActivityLogList = new List<ErpActivityLogs>();
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        foreach (var propertyInfo in propertyInfoList)
        {
            var propertyName = propertyInfo.Name;
            var propertyType = propertyInfo.PropertyType;

            if (
                !propertyNamesToIgnore.Contains(propertyName, StringComparer.OrdinalIgnoreCase)
                && (propertyType.IsPrimitive || propertyTypesToAllow.Contains(propertyType))
            )
            {
                var value = propertyInfo.GetValue(updatedEntity)?.ToString();
                var erpActivityLog = new ErpActivityLogs
                {
                    EntityId = updatedEntity.Id,
                    EntityName = type.Name ?? string.Empty,
                    EntityDescription =
                        await GetFormattedEntityInfoAsync(updatedEntity) ?? string.Empty,
                    PropertyName = propertyName,
                    ErpActivityLogTypeId = (int)activityType,
                    CustomerId = currentCustomer.Id,
                    IpAddress = currentCustomer.LastIpAddress,
                    CreatedOnUtc = DateTime.UtcNow,
                };

                if (activityType == ErpActivityType.Insert)
                {
                    erpActivityLog.NewValue = await GetFormattedPropertyValuesAsync(
                        propertyInfo.Name,
                        value
                    );
                    erpActivityLog.EntityName = type.Name ?? string.Empty;

                    if (
                        updatedEntity is ErpSalesRepErpAccountMap
                        && propertyName == "ErpShipToAddressId"
                    )
                        await AppendErpAccountInfoAsync(
                            erpActivityLog,
                            ErpActivityType.Insert,
                            newValue: value
                        );

                    erpActivityLogList.Add(erpActivityLog);
                }
                else if (activityType == ErpActivityType.Delete)
                {
                    erpActivityLog.OldValue = await GetFormattedPropertyValuesAsync(
                        propertyInfo.Name,
                        value
                    );
                    erpActivityLog.EntityName = type.Name ?? string.Empty;

                    if (
                        updatedEntity is ErpShiptoAddressErpAccountMap
                        && propertyName == "ErpShipToAddressId"
                    )
                        await AppendErpAccountInfoAsync(
                            erpActivityLog,
                            ErpActivityType.Delete,
                            newValue: value
                        );

                    erpActivityLogList.Add(erpActivityLog);
                }
                else if (activityType == ErpActivityType.Update)
                {
                    var oldValue = propertyInfo.GetValue(databaseCopyOfEntity)?.ToString();
                    if (oldValue != null && value != null && oldValue != value)
                    {
                        erpActivityLog.EntityName = type.Name ?? string.Empty;
                        erpActivityLog.OldValue = await GetFormattedPropertyValuesAsync(
                            propertyInfo.Name,
                            oldValue
                        );
                        erpActivityLog.NewValue = await GetFormattedPropertyValuesAsync(
                            propertyInfo.Name,
                            value
                        );

                        if (
                            updatedEntity is ErpShiptoAddressErpAccountMap
                            && propertyName == "ErpShipToAddressId"
                        )
                            await AppendErpAccountInfoAsync(
                                erpActivityLog,
                                ErpActivityType.Update,
                                oldValue: oldValue,
                                newValue: value
                            );

                        erpActivityLogList.Add(erpActivityLog);
                    }
                }
            }
        }

        if (erpActivityLogList.Count > 0)
            _erpActivityLogRepository.Insert(erpActivityLogList);
    }

    public async Task InsertErpActivityLogForCustomerRolesAsync(
        Customer customer,
        List<CustomerRole> oldCustomerRoles
    )
    {
        if (oldCustomerRoles == null)
            oldCustomerRoles = new List<CustomerRole>();
        var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
        var newCustomerRoles = customerRoles?.ToList() ?? new List<CustomerRole>();
        var differencesInCustomerRoles = oldCustomerRoles
            .Except(newCustomerRoles)
            .Concat(newCustomerRoles.Except(oldCustomerRoles))
            ?.Count();
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (differencesInCustomerRoles > 0)
        {
            var resource = await _localizationService.GetResourceAsync(
                "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.CustomerCustomerRoleMapping"
            );
            resource = resource.Replace(@"\n", Environment.NewLine);
            var erpActivityLog = new ErpActivityLogs
            {
                EntityId = customer.Id,
                EntityName = nameof(CustomerCustomerRoleMapping),
                EntityDescription = string.Format(resource, customer.Id, customer.Email),
                PropertyName = nameof(customerRoles),
                ErpActivityLogTypeId = (int)ErpActivityType.Update,
                OldValue = string.Join(',', oldCustomerRoles.Select(x => x.Name)),
                NewValue = string.Join(',', newCustomerRoles.Select(x => x.Name)),
                CustomerId = currentCustomer.Id,
                IpAddress = currentCustomer.LastIpAddress,
                CreatedOnUtc = DateTime.UtcNow,
            };

            await InsertErpActivityLogAsync(erpActivityLog);
        }
    }

    public async Task InsertErpActivityLogForCustomerGenericAttributesAsync(
        Customer customer,
        List<GenericAttribute> oldGenericAttributes
    )
    {
        oldGenericAttributes ??= new List<GenericAttribute>();

        var erpActivityLogList = new List<ErpActivityLogs>();
        var newGenAttributes = await _genericAttributeService.GetAttributesForEntityAsync(
            customer.Id,
            nameof(Customer)
        );
        var newGenericAttributes = newGenAttributes.ToList();
        var differencesInGenericAttributes = (
            from item1 in oldGenericAttributes
            join item2 in newGenericAttributes on item1.Key equals item2.Key
            where item1.Value != item2.Value
            select new
            {
                Key = item1.Key,
                OldValue = item1.Value,
                NewValue = item2.Value,
            }
        ).ToList();

        var reduceDifference = oldGenericAttributes
            .Where(x => !newGenericAttributes.Select(y => y.Key).Contains(x.Key))
            .Select(x => x)
            .ToList();
        var extendedDifference = newGenericAttributes
            .Where(x => !oldGenericAttributes.Select(y => y.Key).Contains(x.Key))
            .Select(x => x)
            .ToList();

        if (differencesInGenericAttributes != null)
        {
            if (reduceDifference != null)
            {
                foreach (var item in reduceDifference)
                {
                    differencesInGenericAttributes.Add(
                        new
                        {
                            Key = item.Key,
                            OldValue = item.Value,
                            NewValue = string.Empty,
                        }
                    );
                }
            }

            if (extendedDifference != null)
            {
                foreach (var item in extendedDifference)
                {
                    differencesInGenericAttributes.Add(
                        new
                        {
                            Key = item.Key,
                            OldValue = string.Empty,
                            NewValue = item.Value,
                        }
                    );
                }
            }
        }
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        foreach (var difference in differencesInGenericAttributes)
        {
            var resource = await _localizationService.GetResourceAsync(
                "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.GenericAttribute"
            );
            resource = resource.Replace(@"\n", Environment.NewLine);
            var erpActivityLog = new ErpActivityLogs
            {
                EntityId = customer.Id,
                CustomerId = currentCustomer.Id,
                IpAddress = currentCustomer.LastIpAddress,
                EntityDescription = string.Format(resource, customer.Id, customer.Email),
                PropertyName = difference.Key,
                OldValue = difference.OldValue,
                NewValue = difference.NewValue,
                ErpActivityLogTypeId = (int)ErpActivityType.Update,
                CreatedOnUtc = DateTime.UtcNow,
                EntityName = nameof(GenericAttribute),
            };

            erpActivityLogList.Add(erpActivityLog);
        }

        if (erpActivityLogList.Count > 0)
            _erpActivityLogRepository.Insert(erpActivityLogList);
    }

    public async Task InsertErpActivityLogForCustomerPasswordAsync(Customer customer)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var resource = await _localizationService.GetResourceAsync(
            "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.CustomerPassword"
        );
        resource = resource.Replace(@"\n", Environment.NewLine);
        var erpActivityLog = new ErpActivityLogs
        {
            EntityId = customer.Id,
            CustomerId = currentCustomer.Id,
            IpAddress = currentCustomer.LastIpAddress,
            EntityDescription = string.Format(resource, customer.Id, customer.Email),
            ErpActivityLogTypeId = (int)ErpActivityType.Update,
            CreatedOnUtc = DateTime.UtcNow,
            EntityName = nameof(CustomerPassword),
        };

        await InsertErpActivityLogAsync(erpActivityLog);
    }

    /// <summary>
    /// Deletes an erp activity log item
    /// </summary>
    /// <param name="activityLog">Erp Activity log</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteErpActivityAsync(ErpActivityLogs erpActivityLogs)
    {
        await _erpActivityLogRepository.DeleteAsync(erpActivityLogs);
    }

    /// <summary>
    /// Gets all erp activity log items
    /// </summary>
    /// <param name="createdOnFrom">Log item creation from; pass null to load all records</param>
    /// <param name="createdOnTo">Log item creation to; pass null to load all records</param>
    /// <param name="customerId">Customer identifier; pass null to load all records</param>
    /// <param name="activityLogTypeId">Activity log type identifier; pass null to load all records</param>
    /// <param name="ipAddress">IP address; pass null or empty to load all records</param>
    /// <param name="entityName">Entity name; pass null to load all records</param>
    /// <param name="entityId">Entity identifier; pass null to load all records</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erp activity log items
    /// </returns>
    public virtual async Task<IPagedList<ErpActivityLogs>> GetAllErpActivitiesAsync(
        DateTime? createdOnFrom = null,
        DateTime? createdOnTo = null,
        int? customerId = null,
        int? activityLogTypeId = null,
        string ipAddress = null,
        string entityName = null,
        string entityDescription = null,
        string propertyName = null,
        string newValue = null,
        string oldValue = null,
        int? entityId = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue
    )
    {
        return await _erpActivityLogRepository.GetAllPagedAsync(
            query =>
            {
                //filter by IP
                if (!string.IsNullOrEmpty(ipAddress))
                    query = query.Where(logItem => logItem.IpAddress.Contains(ipAddress));

                //filter by creation date
                if (createdOnFrom.HasValue)
                    query = query.Where(logItem => createdOnFrom.Value <= logItem.CreatedOnUtc);
                if (createdOnTo.HasValue)
                    query = query.Where(logItem => createdOnTo.Value >= logItem.CreatedOnUtc);

                //filter by log type
                if (activityLogTypeId.HasValue && activityLogTypeId.Value > 0)
                    query = query.Where(logItem =>
                        activityLogTypeId == logItem.ErpActivityLogTypeId
                    );

                //filter by customer
                if (customerId.HasValue && customerId.Value > 0)
                    query = query.Where(logItem => customerId.Value == logItem.CustomerId);

                //filter by entity
                if (!string.IsNullOrEmpty(entityName) && entityName != "0")
                    query = query.Where(logItem => logItem.EntityName.Equals(entityName));

                if (!string.IsNullOrEmpty(entityDescription))
                    query = query.Where(logItem =>
                        logItem.EntityDescription.Equals(entityDescription)
                    );

                if (!string.IsNullOrEmpty(propertyName))
                    query = query.Where(logItem => logItem.PropertyName.Equals(propertyName));

                if (!string.IsNullOrEmpty(newValue))
                    query = query.Where(logItem => logItem.NewValue.Equals(newValue));

                if (!string.IsNullOrEmpty(oldValue))
                    query = query.Where(logItem => logItem.OldValue.Equals(oldValue));

                if (entityId.HasValue && entityId.Value > 0)
                    query = query.Where(logItem => entityId.Value == logItem.EntityId);

                query = query
                    .OrderByDescending(logItem => logItem.CreatedOnUtc)
                    .ThenBy(logItem => logItem.Id);

                return query;
            },
            pageIndex,
            pageSize
        );
    }

    /// <summary>
    /// Gets an erp activity log item
    /// </summary>
    /// <param name="erpActivityLogId">Erp Activity log identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erp activity log item
    /// </returns>
    public virtual async Task<ErpActivityLogs> GetErpActivityByIdAsync(int erpActivityLogId)
    {
        return await _erpActivityLogRepository.GetByIdAsync(erpActivityLogId);
    }

    /// <summary>
    /// Clears erp activity log
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task ClearAllErpActivitiesAsync()
    {
        await _erpActivityLogRepository.TruncateAsync();
    }

    /// <summary>
    /// Insert Customer Date Of Terms And Condition Checked
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task InsertCustomerDateOfTermsAndConditionCheckedAsync(
        Customer customer,
        string insertCustomerDateOfTermsAndConditionChecked
    )
    {
        var resource = await _localizationService.GetResourceAsync(
            "NopStation.Plugin.B2B.ERPIntegrationCore.ErpActivityLog.EntityInfo.GenericAttribute"
        );
        resource = resource.Replace(@"\n", Environment.NewLine);
        var erpActivityLogGenericAttr = new ErpActivityLogs
        {
            EntityId = customer.Id,
            EntityName = nameof(GenericAttribute),
            EntityDescription = string.Format(resource, customer.Id, customer.Email),
            PropertyName =
                ERPIntegrationCoreDefaults.CustomerDateOfTermsAndConditionCheckedAttributeName,
            ErpActivityLogTypeId = (int)ErpActivityType.Update,
            OldValue = insertCustomerDateOfTermsAndConditionChecked,
            NewValue = string.Empty,
            CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
            IpAddress = (await _workContext.GetCurrentCustomerAsync()).LastIpAddress,
            CreatedOnUtc = DateTime.UtcNow,
        };
        await InsertErpActivityLogAsync(erpActivityLogGenericAttr);

        var erpActivityLogGenericAttrBool = new ErpActivityLogs
        {
            EntityId = customer.Id,
            EntityName = nameof(GenericAttribute),
            EntityDescription = string.Format(resource, customer.Id, customer.Email),
            PropertyName = ERPIntegrationCoreDefaults.IsDateOfTermsAndConditionCheckedAttributeName,
            ErpActivityLogTypeId = (int)ErpActivityType.Update,
            OldValue = "True",
            NewValue = "False",
            CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
            IpAddress = (await _workContext.GetCurrentCustomerAsync()).LastIpAddress,
            CreatedOnUtc = DateTime.UtcNow,
        };
        await InsertErpActivityLogAsync(erpActivityLogGenericAttrBool);
    }

    #endregion
}
