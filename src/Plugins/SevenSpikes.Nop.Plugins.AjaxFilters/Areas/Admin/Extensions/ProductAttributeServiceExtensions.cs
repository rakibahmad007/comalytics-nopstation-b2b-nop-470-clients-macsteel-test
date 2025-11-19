using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Areas.Admin.Extensions;

public static class ProductAttributeServiceExtensions
{
	public static ProductAttribute GetProductAttributeByName(this IProductAttributeService productAttributeService, string name)
	{
		return EngineContext.Current.Resolve<IRepository<ProductAttribute>>((IServiceScope)null).Table.Where((Expression<Func<ProductAttribute, bool>>)((ProductAttribute sa) => sa.Name.Equals(name))).FirstOrDefault();
	}

	public static IList<ProductAttribute> GetProductAttributesByIds(this IProductAttributeService productAttributeService, int[] ids)
	{
		List<ProductAttribute> list = EngineContext.Current.Resolve<IRepository<ProductAttribute>>((IServiceScope)null).Table.Where((Expression<Func<ProductAttribute, bool>>)((ProductAttribute sa) => ids.Contains(((BaseEntity)sa).Id))).OrderBy((Expression<Func<ProductAttribute, int>>)((ProductAttribute sa) => ((BaseEntity)sa).Id)).ToList();
		List<ProductAttribute> list2 = new List<ProductAttribute>();
		int[] array = ids;
		foreach (int id in array)
		{
			ProductAttribute val = list.Find((ProductAttribute x) => ((BaseEntity)x).Id == id);
			if (val != null)
			{
				list2.Add(val);
			}
		}
		return list2;
	}
}
