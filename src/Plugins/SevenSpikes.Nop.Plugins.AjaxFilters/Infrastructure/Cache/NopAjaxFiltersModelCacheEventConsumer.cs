using System;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Events;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;

public class NopAjaxFiltersModelCacheEventConsumer : IConsumer<EntityInsertedEvent<ProductSpecificationAttribute>>, IConsumer<EntityUpdatedEvent<ProductSpecificationAttribute>>, IConsumer<EntityDeletedEvent<ProductSpecificationAttribute>>, IConsumer<EntityInsertedEvent<ProductAttributeMapping>>, IConsumer<EntityUpdatedEvent<ProductAttributeMapping>>, IConsumer<EntityDeletedEvent<ProductAttributeMapping>>, IConsumer<EntityInsertedEvent<ProductAttributeValue>>, IConsumer<EntityUpdatedEvent<ProductAttributeValue>>, IConsumer<EntityDeletedEvent<ProductAttributeValue>>, IConsumer<EntityInsertedEvent<PredefinedProductAttributeValue>>, IConsumer<EntityUpdatedEvent<PredefinedProductAttributeValue>>, IConsumer<EntityDeletedEvent<PredefinedProductAttributeValue>>, IConsumer<EntityInsertedEvent<ProductManufacturer>>, IConsumer<EntityUpdatedEvent<ProductManufacturer>>, IConsumer<EntityDeletedEvent<ProductManufacturer>>, IConsumer<EntityInsertedEvent<Product>>, IConsumer<EntityUpdatedEvent<Product>>, IConsumer<EntityDeletedEvent<Product>>, IConsumer<EntityInsertedEvent<Discount>>, IConsumer<EntityUpdatedEvent<Discount>>, IConsumer<EntityDeletedEvent<Discount>>, IConsumer<EntityInsertedEvent<TaxCategory>>, IConsumer<EntityUpdatedEvent<TaxCategory>>, IConsumer<EntityDeletedEvent<TaxCategory>>, IConsumer<EntityInsertedEvent<Currency>>, IConsumer<EntityUpdatedEvent<Currency>>, IConsumer<EntityDeletedEvent<Currency>>, IConsumer<EntityInsertedEvent<Setting>>, IConsumer<EntityUpdatedEvent<Setting>>, IConsumer<EntityDeletedEvent<Setting>>, IConsumer<EntityInsertedEvent<AclRecord>>, IConsumer<EntityUpdatedEvent<AclRecord>>, IConsumer<EntityDeletedEvent<AclRecord>>, IConsumer<EntityInsertedEvent<Language>>, IConsumer<EntityUpdatedEvent<Language>>, IConsumer<EntityDeletedEvent<Language>>, IConsumer<EntityUpdatedEvent<Category>>
{
	public const int MODEL_CACHE_TIME_MINUTES = 3;

	public const string NOP_AJAX_FILTERS_PATTERN_KEY = "nop.pres.nop.ajax.filters";

	public const string NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_PATTERN_KEY = "nop.pres.nop.ajax.filters.specification.filters";

	public const string NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_PATTERN_KEY = "nop.pres.nop.ajax.filters.attribute.filters";

	public const string NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_PATTERN_KEY = "nop.pres.nop.ajax.filters.manufacturer.filters";

	public const string NOP_AJAX_FILTERS_VENDOR_FILTERS_PATTERN_KEY = "nop.pres.nop.ajax.filters.vednor.filters";

	public static readonly string NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY = "nop.pres.nop.ajax.filters.price.range.filter";

	public const string NOP_AJAX_FILTERS_FILTERED_PRODUCTS_PATTERN = "nop.pres.nop.ajax.filters.filtered.products";

	public static CacheKey NOP_AJAX_FILTERS_MODEL_KEY => new CacheKey("nop.pres.nop.ajax.filters-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}-{11}-{12}-{13}-{14}", new string[1] { "nop.pres.nop.ajax.filters.attribute.filters" });

	public static CacheKey NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_MODEL_KEY => new CacheKey("nop.pres.nop.ajax.filters.specification.filters-{0}-{1}-{2}-{3}-{4}-{5}", new string[1] { "nop.pres.nop.ajax.filters.specification.filters" });

	public static CacheKey NOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY => new CacheKey("nop.pres.nop.ajax.filters.specification.filters-option-ids-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}-{11}", new string[1] { "nop.pres.nop.ajax.filters.specification.filters" })
	{
		CacheTime = 3
	};

	public static CacheKey NOP_AJAX_FILTERS_PREDEFINED_ATTRIBUTE_VALUES_KEY => new CacheKey("nop.pres.nop.ajax.filters.predefined.attribute.values", Array.Empty<string>());

	public static CacheKey NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_MODEL_KEY => new CacheKey("nop.pres.nop.ajax.filters.attribute.filters-{0}-{1}-{2}-{3}-{4}-{5}", new string[1] { "nop.pres.nop.ajax.filters.attribute.filters" });

	public static CacheKey NOP_AJAX_FILTERS_ATTRIBUTE_OPTION_IDS_KEY => new CacheKey("nop.pres.nop.ajax.filters.attribute.filters-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}-{11}", new string[1] { "nop.pres.nop.ajax.filters.attribute.filters" })
	{
		CacheTime = 3
	};

	public static CacheKey NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_MODEL_KEY => new CacheKey("nop.pres.nop.ajax.filters.manufacturer.filters-{0}-{1}-{2}-{3}-{4}", new string[1] { "nop.pres.nop.ajax.filters.manufacturer.filters" });

	public static CacheKey NOP_AJAX_FILTERS_ONSALE_FILTERS_MODEL_KEY => new CacheKey("nop.pres.nop.ajax.filters.onsale.filters-{0}-{1}-{2}-{3}-{4}-{5}", Array.Empty<string>());

	public static CacheKey NOP_AJAX_FILTERS_INSTOCK_FILTERS_MODEL_KEY => new CacheKey("nop.pres.nop.ajax.filters.instock.filters-{0}-{1}-{2}-{3}-{4}-{5}", Array.Empty<string>());

	public static CacheKey NOP_AJAX_FILTERS_VENDOR_FILTERS_MODEL_KEY => new CacheKey("nop.pres.nop.ajax.filters.vendor.filters-{0}-{1}-{2}", new string[1] { "nop.pres.nop.ajax.filters.vednor.filters" });

	public static CacheKey NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_MODEL_KEY => new CacheKey("nop.pres.nop.ajax.filters.price.range.filter-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}", new string[1] { NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY });

	public static CacheKey NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_DTO_KEY => new CacheKey("nop.pres.nop.ajax.filters.price.range.filter-dto-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}", new string[1] { NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY });

	public static CacheKey NOP_AJAX_FILTERS_FILTERED_PRODUCTS_KEY => new CacheKey("nop.pres.nop.ajax.filters.filtered.products.{0}.{1}.{2}.{3}.{4}.{5}.{6}.{7}.{8}.{9}.{10}.{11}.{12}.{13}.{14}", new string[1] { "nop.pres.nop.ajax.filters.filtered.products" })
	{
		CacheTime = 3
	};

	private IStaticCacheManager CacheManager { get; }

	public NopAjaxFiltersModelCacheEventConsumer(IStaticCacheManager cacheManager)
	{
		CacheManager = cacheManager;
	}

	public async Task HandleEventAsync(EntityInsertedEvent<ProductSpecificationAttribute> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.specification.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<ProductSpecificationAttribute> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.specification.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityDeletedEvent<ProductSpecificationAttribute> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.specification.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityInsertedEvent<ProductAttributeMapping> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<ProductAttributeMapping> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityDeletedEvent<ProductAttributeMapping> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityInsertedEvent<ProductAttributeValue> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<ProductAttributeValue> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityDeletedEvent<ProductAttributeValue> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityInsertedEvent<PredefinedProductAttributeValue> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
		await CacheManager.RemoveAsync(NOP_AJAX_FILTERS_PREDEFINED_ATTRIBUTE_VALUES_KEY, Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<PredefinedProductAttributeValue> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
		await CacheManager.RemoveAsync(NOP_AJAX_FILTERS_PREDEFINED_ATTRIBUTE_VALUES_KEY, Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityDeletedEvent<PredefinedProductAttributeValue> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
		await CacheManager.RemoveAsync(NOP_AJAX_FILTERS_PREDEFINED_ATTRIBUTE_VALUES_KEY, Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityInsertedEvent<ProductManufacturer> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.manufacturer.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<ProductManufacturer> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.manufacturer.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityDeletedEvent<ProductManufacturer> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.manufacturer.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityInsertedEvent<Setting> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.specification.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.manufacturer.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<Setting> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.specification.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.manufacturer.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityDeletedEvent<Setting> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.specification.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.manufacturer.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<Category> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityInsertedEvent<Discount> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<Discount> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityDeletedEvent<Discount> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityInsertedEvent<TaxCategory> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<TaxCategory> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityDeletedEvent<TaxCategory> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityInsertedEvent<Currency> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<Currency> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityDeletedEvent<Currency> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityInsertedEvent<AclRecord> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.specification.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.manufacturer.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<AclRecord> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.specification.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.manufacturer.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityDeletedEvent<AclRecord> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.specification.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.attribute.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.manufacturer.filters", Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync(NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_PATTERN_KEY, Array.Empty<object>());
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters.filtered.products", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityInsertedEvent<Language> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<Language> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
	}

	public async Task HandleEventAsync(EntityDeletedEvent<Language> eventMessage)
	{
		await CacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
	}
}
