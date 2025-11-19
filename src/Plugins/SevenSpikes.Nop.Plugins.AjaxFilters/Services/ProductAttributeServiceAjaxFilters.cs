using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.AttributeFilter;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public class ProductAttributeServiceAjaxFilters : IProductAttributeServiceAjaxFilters
{
	private readonly IRepository<PredefinedProductAttributeValue> _predefinedAttributeValueRepository;

	private readonly IStaticCacheManager _staticCacheManager;

	public ProductAttributeServiceAjaxFilters(IRepository<PredefinedProductAttributeValue> predefinedAttributeValueRepository, IStaticCacheManager staticCacheManager)
	{
		_predefinedAttributeValueRepository = predefinedAttributeValueRepository;
		_staticCacheManager = staticCacheManager;
	}

	public async Task<IList<AttributeFilterItem>> GetSortedAttributeValuesBasedOnTheirPredefinedDisplayOrderAsync(IEnumerable<AttributeFilterItem> attributeValues)
	{
		return await AsyncIEnumerableExtensions.ToListAsync<AttributeFilterItem>(from attributeValue in attributeValues
			join predefinedAttribute in await _staticCacheManager.GetAsync<List<PredefinedProductAttributeValue>>(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_PREDEFINED_ATTRIBUTE_VALUES_KEY, (Func<Task<List<PredefinedProductAttributeValue>>>)(async () => await AsyncIQueryableExtensions.ToListAsync<PredefinedProductAttributeValue>(_predefinedAttributeValueRepository.Table))) on new
			{
				AttributeId = attributeValue.AttributeId,
				AttributeValue = attributeValue.Name
			} equals new
			{
				AttributeId = predefinedAttribute.ProductAttributeId,
				AttributeValue = predefinedAttribute.Name
			} into temp
			from predefinedAttributeValue in temp.DefaultIfEmpty(new PredefinedProductAttributeValue
			{
				DisplayOrder = int.MaxValue
			})
			orderby predefinedAttributeValue.DisplayOrder
			select attributeValue);
	}
}
