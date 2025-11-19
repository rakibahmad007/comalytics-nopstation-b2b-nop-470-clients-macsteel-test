using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Catalog.DTO;
using SevenSpikes.Nop.Services.Helpers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;

public class AttributeFilterOptionsHelper : IAttributeFilterOptionsHelper
{
	private IProductAttributeService7Spikes ProductAttributeService7Spikes { get; set; }

	private Dictionary<int, List<int>> AvailableAttributeOptionIds { get; set; }

	private bool NoAttributeFiltersSelected { get; set; }

	public Dictionary<int, List<Product>> PotentiallyAvailableAttributeOptionIds { get; private set; }

	public AttributeFilterOptionsHelper(IProductAttributeService7Spikes productAttributeService7Spikes)
	{
		ProductAttributeService7Spikes = productAttributeService7Spikes;
		AvailableAttributeOptionIds = new Dictionary<int, List<int>>();
		PotentiallyAvailableAttributeOptionIds = new Dictionary<int, List<Product>>();
	}

	public async Task<IQueryable<Product>> GetProductsForAttributeFiltersAndDetermineAvailableAttributeOptionsForLaterRerievalAsync(IQueryable<Product> query, AttributeFilterModelDTO attributeFilterModelDTO)
	{
		IList<int> productIdsToRemove = new List<int>();
		if (attributeFilterModelDTO != null && attributeFilterModelDTO.AttributeFilterDTOs != null && attributeFilterModelDTO.AttributeFilterDTOs.Count > 0)
		{
			foreach (Product product in query)
			{
				if (!(await DetermineWhetherProductMeetsAttributeFiltersAsync(product, attributeFilterModelDTO.AttributeFilterDTOs)))
				{
					productIdsToRemove.Add(((BaseEntity)product).Id);
				}
			}
			query = query.Where((Expression<Func<Product, bool>>)((Product p) => !productIdsToRemove.Contains(((BaseEntity)p).Id)));
		}
		else
		{
			NoAttributeFiltersSelected = true;
		}
		return query;
	}

	private async Task<bool> DetermineWhetherProductMeetsAttributeFiltersAsync(Product product, IList<AttributeFilterDTO> attributeFilterDTOs)
	{
		if (attributeFilterDTOs == null || attributeFilterDTOs.Count == 0)
		{
			return true;
		}
		IList<AttributeFilterDTO> attributeFilterDtosLocal = attributeFilterDTOs.ToList();
		IList<AttributeFilterDTO> potentiallyOkGroups = new List<AttributeFilterDTO>();
		List<int> potentiallyOkProductVariantIds = new List<int>();
		IList<ProductAttributeMapping> list = await ProductAttributeService7Spikes.GetAllProductVariantAttributesWhichHaveValuesByProductIdAsync(((BaseEntity)product).Id);
		foreach (ProductAttributeMapping item in list)
		{
			int productVariantAttributeId = ((BaseEntity)item).Id;
			AttributeFilterDTO attributeFilterDTO = attributeFilterDtosLocal.FirstOrDefault((AttributeFilterDTO x) => x.SelectedProductVariantIds.Contains(productVariantAttributeId));
			if (attributeFilterDTO != null)
			{
				attributeFilterDtosLocal.Remove(attributeFilterDTO);
			}
			else
			{
				attributeFilterDTO = attributeFilterDtosLocal.FirstOrDefault((AttributeFilterDTO x) => x.AllProductVariantIds.Contains(productVariantAttributeId));
				if (attributeFilterDTO != null)
				{
					potentiallyOkGroups.Add(attributeFilterDTO);
					potentiallyOkProductVariantIds.Add(productVariantAttributeId);
				}
			}
			if (attributeFilterDtosLocal.FirstOrDefault() == null)
			{
				break;
			}
		}
		bool result;
		if (attributeFilterDtosLocal.FirstOrDefault() == null)
		{
			result = true;
			foreach (ProductAttributeMapping item2 in list)
			{
				AddAvailableAttributeOptionId(((BaseEntity)item2).Id, ((BaseEntity)product).Id);
			}
		}
		else if (attributeFilterDtosLocal.Count == 1 && potentiallyOkGroups.Contains(attributeFilterDtosLocal[0]))
		{
			result = false;
			foreach (int item3 in potentiallyOkProductVariantIds)
			{
				AddPotentialAttributeOptionId(item3, product);
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	public async Task<bool> DetermineWhetherPotentialProductMeetsAttributeFiltersAsync(Product product, AttributeFilterModelDTO attributeFilterModelDTO)
	{
		if (attributeFilterModelDTO == null || attributeFilterModelDTO.AttributeFilterDTOs.Count == 0)
		{
			return true;
		}
		IList<AttributeFilterDTO> attributeFilterDtosLocal = attributeFilterModelDTO.AttributeFilterDTOs.ToList();
		foreach (ProductAttributeMapping item in await ProductAttributeService7Spikes.GetAllProductVariantAttributesWhichHaveValuesByProductIdAsync(((BaseEntity)product).Id))
		{
			int productVariantAttributeId = ((BaseEntity)item).Id;
			AttributeFilterDTO attributeFilterDTO = attributeFilterDtosLocal.FirstOrDefault((AttributeFilterDTO x) => x.SelectedProductVariantIds.Contains(productVariantAttributeId));
			if (attributeFilterDTO != null)
			{
				attributeFilterDtosLocal.Remove(attributeFilterDTO);
			}
			if (attributeFilterDtosLocal.FirstOrDefault() == null)
			{
				break;
			}
		}
		return attributeFilterDtosLocal.FirstOrDefault() == null;
	}

	private void AddAvailableAttributeOptionId(int attributeOptionId, int productId)
	{
		if (!AvailableAttributeOptionIds.ContainsKey(attributeOptionId))
		{
			List<int> value = new List<int> { productId };
			AvailableAttributeOptionIds.Add(attributeOptionId, value);
			return;
		}
		IList<int> list = AvailableAttributeOptionIds[attributeOptionId];
		if (!list.Contains(productId))
		{
			list.Add(productId);
		}
	}

	private void AddPotentialAttributeOptionId(int attributeOptionId, Product product)
	{
		if (!PotentiallyAvailableAttributeOptionIds.ContainsKey(attributeOptionId))
		{
			List<Product> value = new List<Product> { product };
			PotentiallyAvailableAttributeOptionIds.Add(attributeOptionId, value);
			return;
		}
		IList<Product> list = PotentiallyAvailableAttributeOptionIds[attributeOptionId];
		if (!list.Contains(product))
		{
			list.Add(product);
		}
	}

	public async Task<IList<int>> GetAvailableProductVariantAttributeIdsForFilteredProductsAsync(IList<Product> products)
	{
		List<int> productVariantAttributeIds = new List<int>();
		productVariantAttributeIds.AddRange(PotentiallyAvailableAttributeOptionIds.Keys);
		IList<int> list = products.Select((Product x) => ((BaseEntity)x).Id).ToList();
		if (NoAttributeFiltersSelected && list.Count > 0)
		{
			IList<int> collection = (await ProductAttributeService7Spikes.GetAllProductVariantAttributesWhichHaveValuesByProductIdsAsync(list)).Select((ProductAttributeMapping x) => ((BaseEntity)x).Id).ToList();
			productVariantAttributeIds.AddRange(collection);
		}
		else
		{
			foreach (int key in AvailableAttributeOptionIds.Keys)
			{
				if (!productVariantAttributeIds.Contains(key) && AvailableAttributeOptionIds[key].Intersect(list).Any())
				{
					productVariantAttributeIds.Add(key);
				}
			}
		}
		return productVariantAttributeIds;
	}
}
