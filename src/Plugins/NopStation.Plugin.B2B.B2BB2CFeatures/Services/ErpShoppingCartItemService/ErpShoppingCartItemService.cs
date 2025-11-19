using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Logging;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpShoppingCartItemService;

public class ErpShoppingCartItemService : IErpShoppingCartItemService
{
    #region Fields

    private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
    private readonly INopDataProvider _dataProvider;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IB2CShoppingCartItemService _b2CShoppingCartItemService;
    private readonly ILogger _logger;
    private readonly IErpLogsService _erpLogsService;

    #endregion

    #region Ctor

    public ErpShoppingCartItemService(IRepository<ShoppingCartItem> shoppingCartItemRepository,
        INopDataProvider dataProvider,
        IWorkContext workContext,
        IStoreContext storeContext,
        IShoppingCartService shoppingCartService,
        IGenericAttributeService genericAttributeService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IB2CShoppingCartItemService b2CShoppingCartItemService,
        ILogger logger,
        IErpLogsService erpLogsService)
    {
        _shoppingCartItemRepository = shoppingCartItemRepository;
        _dataProvider = dataProvider;
        _workContext = workContext;
        _storeContext = storeContext;
        _shoppingCartService = shoppingCartService;
        _genericAttributeService = genericAttributeService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _b2CShoppingCartItemService = b2CShoppingCartItemService;
        _logger = logger;
        _erpLogsService = erpLogsService;
    }

    #endregion

    #region Methods

    public async Task ShoppingCartEventCheckerAsync()
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        var shoppingCartItems = await _shoppingCartService.GetShoppingCartAsync(currentCustomer,
            ShoppingCartType.ShoppingCart, currentStore.Id);

        var b2bOrderId = await _genericAttributeService.GetAttributeAsync<int>(
            currentCustomer,
            B2BB2CFeaturesDefaults.B2BConvertedQuoteB2BOrderId,
            currentStore.Id);
        if (b2bOrderId > 0)
        {
            await _erpCustomerFunctionalityService.CheckAndUpdateGenericAttributeOfB2BQuoteOrder(b2bOrderId, shoppingCartItems);
        }

        var b2COrderId = await _genericAttributeService.GetAttributeAsync<int>(
            currentCustomer,
            B2BB2CFeaturesDefaults.B2CConvertedQuoteB2COrderId,
            currentStore.Id);
        if (b2COrderId > 0)
        {
            await _erpCustomerFunctionalityService.CheckAndUpdateGenericAttributeOfB2CQuoteOrder(b2COrderId, shoppingCartItems);
        }
    }

    public async Task RemoveItemFromB2CShoppingCartByNopSciId(int id)
    {
        var b2cShoppingCartItem = await _b2CShoppingCartItemService.GetB2CShoppingCartItemByNopShoppingCartItemIdAsync(id);

        if (b2cShoppingCartItem == null)
            return;

        await _b2CShoppingCartItemService.DeleteB2CShoppingCartItemAsync(b2cShoppingCartItem);
    }

    public async Task AddB2CShoppingCartItem(int id)
    {
        if (id > 0)
        {
            try
            {
                var b2cShoppingCartItem = await _b2CShoppingCartItemService.GetB2CShoppingCartItemByNopShoppingCartItemIdAsync(id);

                if (b2cShoppingCartItem == null)
                    await _b2CShoppingCartItemService.InsertB2CShoppingCartItemAsync(new B2CShoppingCartItem { ShoppingCartItemId = id });
                else
                {
                    b2cShoppingCartItem.ShoppingCartItemId = id;
                    await _b2CShoppingCartItemService.UpdateB2CShoppingCartItemAsync(b2cShoppingCartItem);
                }
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLevel.Order, "Error while adding B2C Shopping Cart Item", ex.Message);
            }
        }
    }

    public async Task UpdateErpShoppingCartItemAsync(ShoppingCartItem shoppingCartItem)
    {
        if (shoppingCartItem == null || shoppingCartItem.Id < 1)
        {
            await _logger.InsertLogAsync(LogLevel.Error, "Shopping Cart Item update error. Item is empty or null", "Shopping Cart Item update error. Item is empty or null");
            throw new ArgumentNullException(nameof(shoppingCartItem));
        }
        await _shoppingCartItemRepository.UpdateAsync(shoppingCartItem);
    }

    public async Task InsertErpShoppingCartItemAsync(ShoppingCartItem shoppingCartItem)
    {
        if (shoppingCartItem == null)
        {
            await _logger.InsertLogAsync(LogLevel.Error, "Shopping Cart Item insert error. Item is empty or null", "Shopping Cart Item insert error. Item is empty");
            throw new ArgumentNullException(nameof(shoppingCartItem));
        }
        await _shoppingCartItemRepository.InsertAsync(shoppingCartItem);
    }

    public async Task DeleteErpShoppingCartItemAsync(ShoppingCartItem shoppingCartItem)
    {
        if (shoppingCartItem == null)
        {
            await _logger.InsertLogAsync(LogLevel.Error, "Shopping Cart Item delete error. Item is empty or null", "Shopping Cart Item delete error. Item is empty");
            throw new ArgumentNullException(nameof(shoppingCartItem));
        }
        await _shoppingCartItemRepository.DeleteAsync(shoppingCartItem);
    }

    public async Task<ShoppingCartItem> GetErpShoppingCartItemByIdAsync(int shoppingCartItemId)
    {
        if (shoppingCartItemId < 1)
        {
            return null;
        }
        return await _shoppingCartItemRepository.GetByIdAsync(shoppingCartItemId);
    }

    public async Task InsertBulkShoppingCartItemsAsync(IList<ShoppingCartItem> shoppingCartItems, bool cartActivityFromB2CUser = false)
    {
        if (shoppingCartItems == null || shoppingCartItems.Count == 0)
        {
            await _logger.InsertLogAsync(LogLevel.Error, "Shopping Cart items insert error. Item list is empty or null",
                "Shopping Cart items insert error. Item list is empty or null");
        }

        //using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _dataProvider.BulkInsertEntitiesAsync(shoppingCartItems);
        //transaction.Complete();

        if (cartActivityFromB2CUser)
        {
            await AddB2CShoppingCartItems(shoppingCartItems.Select(x => x.Id).ToList());            
        }
    }

    public async Task UpdateBulkShoppingCartItemsAsync(IList<ShoppingCartItem> shoppingCartItems, bool cartActivityFromB2CUser = false)
    {
        if (shoppingCartItems == null || shoppingCartItems.Count == 0)
        {
            await _logger.InsertLogAsync(LogLevel.Error, "Shopping Cart items update error. Item list is empty or null",
                "Shopping Cart items update error. Item list is empty or null");
        }

        //using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _dataProvider.UpdateEntitiesAsync(shoppingCartItems);
        //transaction.Complete();
    }

    public async Task AddB2CShoppingCartItems(IList<int> ids)
    {
        if (ids == null)
            return;

        var idList = ids.Where(id => id > 0);
        if (!idList.Any())
            return;

        var existingItems = await _b2CShoppingCartItemService.GetB2CShoppingCartItemsByNopShoppingCartItemIdsAsync(idList);

        var toInsert = new List<B2CShoppingCartItem>();
        var toUpdate = new List<B2CShoppingCartItem>();

        foreach (var id in idList)
        {
            var existing = existingItems.FirstOrDefault(x => x.ShoppingCartItemId == id);
            if (existing == null)
            {
                toInsert.Add(new B2CShoppingCartItem { ShoppingCartItemId = id });
            }
            else
            {
                existing.ShoppingCartItemId = id;
                toUpdate.Add(existing);
            }
        }

        if (toInsert.Count != 0)
            await _b2CShoppingCartItemService.InsertB2CShoppingCartItemsAsync(toInsert);

        if (toUpdate.Count != 0)
            await _b2CShoppingCartItemService.UpdateB2CShoppingCartItemsAsync(toUpdate);
    }

    public async Task RemoveItemsFromB2CShoppingCartByNopSciIds(IList<int> ids)
    {
        if (ids == null)
            return;

        var idList = ids.Where(id => id > 0).ToList();
        if (idList.Count == 0)
            return;

        var itemsToRemove = await _b2CShoppingCartItemService.GetB2CShoppingCartItemsByNopShoppingCartItemIdsAsync(idList);

        if (itemsToRemove == null || !itemsToRemove.Any())
            return;

        await _b2CShoppingCartItemService.DeleteB2CShoppingCartItemsAsync(itemsToRemove);
    }

    #endregion

}
