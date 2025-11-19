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

public static class SpecificationAttributeServiceExtensions
{
	public static SpecificationAttribute GetSpecificationAttributeByName(this ISpecificationAttributeService specificationAttributeService, string name)
	{
		return EngineContext.Current.Resolve<IRepository<SpecificationAttribute>>((IServiceScope)null).Table.Where((Expression<Func<SpecificationAttribute, bool>>)((SpecificationAttribute sa) => sa.Name.Equals(name))).FirstOrDefault();
	}

	public static IList<SpecificationAttribute> GetSpecificationAttributeByIds(this ISpecificationAttributeService specificationAttributeService, int[] ids)
	{
		List<SpecificationAttribute> list = EngineContext.Current.Resolve<IRepository<SpecificationAttribute>>((IServiceScope)null).Table.Where((Expression<Func<SpecificationAttribute, bool>>)((SpecificationAttribute sa) => ids.Contains(((BaseEntity)sa).Id))).OrderBy((Expression<Func<SpecificationAttribute, int>>)((SpecificationAttribute sa) => ((BaseEntity)sa).Id)).ToList();
		List<SpecificationAttribute> list2 = new List<SpecificationAttribute>();
		int[] array = ids;
		foreach (int id in array)
		{
			SpecificationAttribute val = list.Find((SpecificationAttribute x) => ((BaseEntity)x).Id == id);
			if (val != null)
			{
				list2.Add(val);
			}
		}
		return list2;
	}
}
