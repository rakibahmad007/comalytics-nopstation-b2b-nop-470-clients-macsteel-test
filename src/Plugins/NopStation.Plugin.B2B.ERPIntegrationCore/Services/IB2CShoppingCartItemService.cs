using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public interface IB2CShoppingCartItemService
{
    Task<B2CShoppingCartItem> GetB2CShoppingCartItemByNopShoppingCartItemIdAsync(
        int nopShoppingCartItemId
    );
    Task<IList<B2CShoppingCartItem>> GetB2CShoppingCartItemsByNopShoppingCartItemIdsAsync(IEnumerable<int> ids);
    Task InsertB2CShoppingCartItemAsync(B2CShoppingCartItem shoppingCartItem);
    Task UpdateB2CShoppingCartItemAsync(B2CShoppingCartItem shoppingCartItem);
    Task DeleteB2CShoppingCartItemAsync(B2CShoppingCartItem shoppingCartItem);
    Task InsertB2CShoppingCartItemsAsync(IList<B2CShoppingCartItem> items);
    Task UpdateB2CShoppingCartItemsAsync(IList<B2CShoppingCartItem> items);
    Task DeleteB2CShoppingCartItemsAsync(IList<B2CShoppingCartItem> items);
}
