using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpShoppingCartItemService;

public interface IErpShoppingCartItemService
{
    Task UpdateErpShoppingCartItemAsync(ShoppingCartItem shoppingCartItem);

    Task InsertErpShoppingCartItemAsync(ShoppingCartItem shoppingCartItem);

    Task DeleteErpShoppingCartItemAsync(ShoppingCartItem shoppingCartItem);

    Task<ShoppingCartItem> GetErpShoppingCartItemByIdAsync(int shoppingCartItemId);

    Task AddB2CShoppingCartItem(int id);

    Task RemoveItemFromB2CShoppingCartByNopSciId(int id);

    Task ShoppingCartEventCheckerAsync();

    Task InsertBulkShoppingCartItemsAsync(IList<ShoppingCartItem> shoppingCartItems, bool cartActivityFromB2CUser = false);

    Task UpdateBulkShoppingCartItemsAsync(IList<ShoppingCartItem> shoppingCartItems, bool cartActivityFromB2CUser = false);

    Task AddB2CShoppingCartItems(IList<int> ids);

    Task RemoveItemsFromB2CShoppingCartByNopSciIds(IList<int> ids);
}
