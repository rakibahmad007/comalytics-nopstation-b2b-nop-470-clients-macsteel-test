using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Pickup.PickupInStore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace Nop.Plugin.Pickup.PickupInStore.Services;

/// <summary>
/// Store pickup point service
/// </summary>
public class StorePickupPointService : IStorePickupPointService
{
    #region Constants

    /// <summary>
    /// Cache key for pickup points
    /// </summary>
    /// <remarks>
    /// {0} : current store ID
    /// </remarks>
    protected readonly CacheKey _pickupPointAllKey = new("Nop.pickuppoint.all-{0}", PICKUP_POINT_PATTERN_KEY);
    protected const string PICKUP_POINT_PATTERN_KEY = "Nop.pickuppoint.";

    #endregion

    #region Fields

    protected readonly IRepository<StorePickupPoint> _storePickupPointRepository;
    protected readonly IShortTermCacheManager _shortTermCacheManager;
    protected readonly IStaticCacheManager _staticCacheManager;
    private readonly IB2BSalesOrgPickupPointService _b2BSalesOrgPickupPointService;
    private readonly IWorkContext _workContext;
    private readonly IErpAccountService _erpAccountService;

    #endregion

    #region Ctor

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="storePickupPointRepository">Store pickup point repository</param>
    /// <param name="shortTermCacheManager">Short term cache manager</param>
    /// <param name="staticCacheManager">Cache manager</param>
    public StorePickupPointService(IRepository<StorePickupPoint> storePickupPointRepository,
        IShortTermCacheManager shortTermCacheManager,
        IStaticCacheManager staticCacheManager,
        IB2BSalesOrgPickupPointService b2BSalesOrgPickupPointService,
        IWorkContext workContext,
        IErpAccountService erpAccountService)
    {
        _storePickupPointRepository = storePickupPointRepository;
        _shortTermCacheManager = shortTermCacheManager;
        _staticCacheManager = staticCacheManager;
        _b2BSalesOrgPickupPointService = b2BSalesOrgPickupPointService;
        _workContext = workContext;
        _erpAccountService = erpAccountService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets all pickup points
    /// </summary>
    /// <param name="storeId">The store identifier; pass 0 to load all records</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the pickup points
    /// </returns>
    public virtual async Task<IPagedList<StorePickupPoint>> GetAllStorePickupPointsAsync(int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        List<int> salesOrgPickupIds = null;
        var salesOrgPickupIdsForCache = string.Empty;

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(
            currCustomer.Id
        );

        if (erpAccount != null)
        {
            salesOrgPickupIds = new List<int>();
            var b2BSalesOrgPickups = await _b2BSalesOrgPickupPointService.GetAllB2BSalesOrgPickupPointsBySalesOrganisationIdAsync(erpAccount.ErpSalesOrgId);
            if (b2BSalesOrgPickups != null && b2BSalesOrgPickups.Any())
            {
                salesOrgPickupIds = b2BSalesOrgPickups.Select(x => x.NopPickupPointId).ToList();
            }

            salesOrgPickupIdsForCache = string.Join(",", salesOrgPickupIds);
        }

        var keyString = string.Format("Nop.pickuppoint.all-{0}-{1}-{2}", pageIndex, pageSize, storeId);
        var key = new CacheKey($"{keyString}-{0}", salesOrgPickupIdsForCache);

        var rez = await _shortTermCacheManager.GetAsync(async () => await _storePickupPointRepository.GetAllAsync(query =>
        {
            if (storeId > 0)
                query = query.Where(point => point.StoreId == storeId || point.StoreId == 0);

            if (salesOrgPickupIds != null && salesOrgPickupIds.Any())
            {
                query = query.Where(point => salesOrgPickupIds.Contains(point.Id));
            }

            query = query.OrderBy(point => point.DisplayOrder).ThenBy(point => point.Name);

            return query;
        }), key, storeId);

        return new PagedList<StorePickupPoint>(rez, pageIndex, pageSize);
    }

    /// <summary>
    /// Gets a pickup point
    /// </summary>
    /// <param name="pickupPointId">Pickup point identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the pickup point
    /// </returns>
    public virtual async Task<StorePickupPoint> GetStorePickupPointByIdAsync(int pickupPointId)
    {
        return await _storePickupPointRepository.GetByIdAsync(pickupPointId);
    }

    /// <summary>
    /// Inserts a pickup point
    /// </summary>
    /// <param name="pickupPoint">Pickup point</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertStorePickupPointAsync(StorePickupPoint pickupPoint)
    {
        await _storePickupPointRepository.InsertAsync(pickupPoint, false);
        await _staticCacheManager.RemoveByPrefixAsync(PICKUP_POINT_PATTERN_KEY);
    }

    /// <summary>
    /// Updates the pickup point
    /// </summary>
    /// <param name="pickupPoint">Pickup point</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateStorePickupPointAsync(StorePickupPoint pickupPoint)
    {
        await _storePickupPointRepository.UpdateAsync(pickupPoint, false);
        await _staticCacheManager.RemoveByPrefixAsync(PICKUP_POINT_PATTERN_KEY);
    }

    /// <summary>
    /// Deletes a pickup point
    /// </summary>
    /// <param name="pickupPoint">Pickup point</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteStorePickupPointAsync(StorePickupPoint pickupPoint)
    {
        await _storePickupPointRepository.DeleteAsync(pickupPoint, false);
        await _staticCacheManager.RemoveByPrefixAsync(PICKUP_POINT_PATTERN_KEY);
    }

    #endregion
}