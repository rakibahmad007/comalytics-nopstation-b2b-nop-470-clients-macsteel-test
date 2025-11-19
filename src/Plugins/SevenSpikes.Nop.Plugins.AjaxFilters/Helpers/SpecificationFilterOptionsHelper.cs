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

public class SpecificationFilterOptionsHelper : ISpecificationFilterOptionsHelper
{
	private bool _shouldRebuildSpecificationOptionsDictionary;

	public ISpecificationAttributeService7Spikes SpecificationAttributeService7Spikes { get; set; }

	private Dictionary<int, List<int>> AvailableSpecificationAttributeOptionIds { get; set; }

	private bool NoSpecificationFiltersSelected { get; set; }

	private IDictionary<int, IList<int>> SpecificationOptionsDictionary { get; set; }

	public Dictionary<int, List<Product>> PotentiallyAvailableSpecificationOptionIds { get; private set; }

	public SpecificationFilterOptionsHelper(ISpecificationAttributeService7Spikes specificationAttributeService7Spikes)
	{
		SpecificationAttributeService7Spikes = specificationAttributeService7Spikes;
		AvailableSpecificationAttributeOptionIds = new Dictionary<int, List<int>>();
		PotentiallyAvailableSpecificationOptionIds = new Dictionary<int, List<Product>>();
	}

	public async Task<IQueryable<Product>> GetProductsForSpecificationFiltersAndDetermineAvailableSpecificationOptionsForLaterRerievalAsync(IQueryable<Product> query, SpecificationFilterModelDTO specifiationFilterModelDTO)
	{
		IList<int> productIdsToRemove = new List<int>();
		if (specifiationFilterModelDTO != null && specifiationFilterModelDTO.SpecificationFilterDTOs != null && specifiationFilterModelDTO.SpecificationFilterDTOs.Count > 0)
		{
			NoSpecificationFiltersSelected = false;
			IDictionary<int, IList<int>> productAttributeSpecificationOptionsDictionary = await SpecificationAttributeService7Spikes.GetSpecificationAttributeOptionsDictionaryForProductsAsync(query);
			_shouldRebuildSpecificationOptionsDictionary = true;
			foreach (Product product in query)
			{
				if (!(await DetermineWhetherProductsMeetsSpecificationFiltersAsync(product, productAttributeSpecificationOptionsDictionary, specifiationFilterModelDTO.SpecificationFilterDTOs)))
				{
					productIdsToRemove.Add(((BaseEntity)product).Id);
				}
			}
			query = query.Where((Expression<Func<Product, bool>>)((Product p) => !productIdsToRemove.Contains(((BaseEntity)p).Id)));
		}
		else
		{
			NoSpecificationFiltersSelected = true;
		}
		return query;
	}

	private async Task<bool> DetermineWhetherProductsMeetsSpecificationFiltersAsync(Product product, IDictionary<int, IList<int>> productSpecificationAttributeOptionsDictionary, IList<SpecificationFilterDTO> specificationFilterDtos)
	{
		if (specificationFilterDtos == null || specificationFilterDtos.Count == 0)
		{
			return true;
		}
		IList<SpecificationFilterDTO> specificationFilterDtosLocal = specificationFilterDtos.ToList();
		IList<SpecificationFilterDTO> potentiallyOkGroups = new List<SpecificationFilterDTO>();
		List<int> potentiallyOkOptionIds = new List<int>();
		if (specificationFilterDtosLocal.Count > 0 && productSpecificationAttributeOptionsDictionary.ContainsKey(((BaseEntity)product).Id))
		{
			IList<int> list = productSpecificationAttributeOptionsDictionary[((BaseEntity)product).Id];
			foreach (int item in list)
			{
				int specificationAttributeOptionIdLocal = item;
				SpecificationFilterDTO productSpecificationOptionsGroup = specificationFilterDtosLocal.FirstOrDefault((SpecificationFilterDTO x) => x.SelectedFilterIds.Contains(specificationAttributeOptionIdLocal));
				if (productSpecificationOptionsGroup != null)
				{
					specificationFilterDtosLocal.Remove(productSpecificationOptionsGroup);
				}
				else
				{
					foreach (SpecificationFilterDTO dto in specificationFilterDtosLocal)
					{
						if (dto.IsMain)
						{
							return false;
						}
						IList<int> second = (await GetSpecificationOptionsDictionaryAsync())[specificationAttributeOptionIdLocal];
						if (dto.SelectedFilterIds.Intersect(second).Any())
						{
							potentiallyOkOptionIds.Add(specificationAttributeOptionIdLocal);
							productSpecificationOptionsGroup = dto;
							break;
						}
					}
					if (productSpecificationOptionsGroup != null)
					{
						potentiallyOkGroups.Add(productSpecificationOptionsGroup);
					}
				}
				if (specificationFilterDtosLocal.FirstOrDefault() == null)
				{
					break;
				}
			}
		}
		bool result;
		if (specificationFilterDtosLocal.FirstOrDefault() == null)
		{
			result = true;
			productSpecificationAttributeOptionsDictionary.TryGetValue(((BaseEntity)product).Id, out var value);
			if (value != null)
			{
				foreach (int item2 in value)
				{
					AddAvailableSpecificationOptionId(item2, ((BaseEntity)product).Id);
				}
			}
		}
		else if (specificationFilterDtosLocal.Count == 1 && potentiallyOkGroups.Contains(specificationFilterDtosLocal[0]))
		{
			result = false;
			foreach (int item3 in potentiallyOkOptionIds)
			{
				AddPotentialSpecificationOptionId(item3, product);
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	private async Task<IDictionary<int, IList<int>>> GetSpecificationOptionsDictionaryAsync()
	{
		if (_shouldRebuildSpecificationOptionsDictionary)
		{
			SpecificationOptionsDictionary = await SpecificationAttributeService7Spikes.GetSpecificationAttributeOptionsDictionaryAsync();
			_shouldRebuildSpecificationOptionsDictionary = false;
		}
		return SpecificationOptionsDictionary;
	}

	private void AddAvailableSpecificationOptionId(int specificationOptionId, int productId)
	{
		if (!AvailableSpecificationAttributeOptionIds.ContainsKey(specificationOptionId))
		{
			List<int> value = new List<int> { productId };
			AvailableSpecificationAttributeOptionIds.Add(specificationOptionId, value);
			return;
		}
		IList<int> list = AvailableSpecificationAttributeOptionIds[specificationOptionId];
		if (!list.Contains(productId))
		{
			list.Add(productId);
		}
	}

	private void AddPotentialSpecificationOptionId(int specificationOptionId, Product product)
	{
		if (!PotentiallyAvailableSpecificationOptionIds.ContainsKey(specificationOptionId))
		{
			List<Product> value = new List<Product> { product };
			PotentiallyAvailableSpecificationOptionIds.Add(specificationOptionId, value);
			return;
		}
		IList<Product> list = PotentiallyAvailableSpecificationOptionIds[specificationOptionId];
		if (!list.Contains(product))
		{
			list.Add(product);
		}
	}

	public async Task<IList<int>> GetAvailableSpecificationAttributeOptionsIdsForFilteredProductsAsync(IList<Product> products)
	{
		List<int> specificationAttributeOptionIds = new List<int>();
		specificationAttributeOptionIds.AddRange(PotentiallyAvailableSpecificationOptionIds.Keys);
		IList<int> list = products.Select((Product x) => ((BaseEntity)x).Id).ToList();
		if (NoSpecificationFiltersSelected && list.Count > 0)
		{
			List<SpecificationAttributeOption> source = (await SpecificationAttributeService7Spikes.GetSpecificationAttributeOptionsByProductIdsAsync(list)).ToList();
			specificationAttributeOptionIds.AddRange(source.Select((SpecificationAttributeOption x) => ((BaseEntity)x).Id));
		}
		else
		{
			foreach (int key in AvailableSpecificationAttributeOptionIds.Keys)
			{
				if (!specificationAttributeOptionIds.Contains(key) && AvailableSpecificationAttributeOptionIds[key].Intersect(list).Any())
				{
					specificationAttributeOptionIds.Add(key);
				}
			}
		}
		return specificationAttributeOptionIds;
	}
}
