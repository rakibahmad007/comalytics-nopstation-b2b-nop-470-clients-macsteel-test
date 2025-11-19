using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Events;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class B2CShoppingCartItemService : IB2CShoppingCartItemService
{
    #region Feilds

    private readonly IRepository<B2CShoppingCartItem> _b2CSciRepository;
    private readonly IEventPublisher _eventPublisher;

    #endregion

    #region Ctor

    public B2CShoppingCartItemService(IRepository<B2CShoppingCartItem> b2CSciRepository, IEventPublisher eventPublisher)
    {
        _b2CSciRepository = b2CSciRepository;
        _eventPublisher = eventPublisher;
    }

    #endregion

    #region Methods

    public async Task<B2CShoppingCartItem> GetB2CShoppingCartItemByNopShoppingCartItemIdAsync(
        int nopShoppingCartItemId
    )
    {
        if (nopShoppingCartItemId < 1)
            return null;

        return await _b2CSciRepository.Table.FirstOrDefaultAsync(a =>
            a.ShoppingCartItemId == nopShoppingCartItemId
        );
    }

    public async Task DeleteB2CShoppingCartItemAsync(B2CShoppingCartItem shoppingCartItem)
    {
        ArgumentNullException.ThrowIfNull(shoppingCartItem);

        await _b2CSciRepository.DeleteAsync(shoppingCartItem);

        await _eventPublisher.EntityDeletedAsync(shoppingCartItem);
    }

    public async Task InsertB2CShoppingCartItemAsync(B2CShoppingCartItem shoppingCartItem)
    {
        ArgumentNullException.ThrowIfNull(shoppingCartItem);

        await _b2CSciRepository.InsertAsync(shoppingCartItem);
    }

    public async Task UpdateB2CShoppingCartItemAsync(B2CShoppingCartItem shoppingCartItem)
    {
        if (shoppingCartItem == null || shoppingCartItem.Id < 1)
        {
            throw new ArgumentNullException(nameof(shoppingCartItem));
        }

        await _b2CSciRepository.UpdateAsync(shoppingCartItem);
    }

    public async Task<IList<B2CShoppingCartItem>> GetB2CShoppingCartItemsByNopShoppingCartItemIdsAsync(IEnumerable<int> ids)
    {
        return await _b2CSciRepository.Table
            .Where(x => ids.Contains(x.ShoppingCartItemId))
            .ToListAsync();
    }

    public async Task InsertB2CShoppingCartItemsAsync(IList<B2CShoppingCartItem> items)
    {
        if (items == null || !items.Any())
        {
            throw new ArgumentNullException(nameof(B2CShoppingCartItem));
        }
        await _b2CSciRepository.InsertAsync(items);
    }

    public async Task UpdateB2CShoppingCartItemsAsync(IList<B2CShoppingCartItem> items)
    {
        if (items == null || !items.Any())
        {
            throw new ArgumentNullException(nameof(B2CShoppingCartItem));
        }
        await _b2CSciRepository.UpdateAsync(items);
    }

    public async Task DeleteB2CShoppingCartItemsAsync(IList<B2CShoppingCartItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        await _b2CSciRepository.DeleteAsync(items);
    }

    #endregion
}
