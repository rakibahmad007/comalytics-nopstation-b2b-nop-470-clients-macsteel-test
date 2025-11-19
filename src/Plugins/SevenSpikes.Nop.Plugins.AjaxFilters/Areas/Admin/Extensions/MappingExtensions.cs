using SevenSpikes.Nop.Framework.AutoMapper;
using SevenSpikes.Nop.Plugins.AjaxFilters.Areas.Admin.Models;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Areas.Admin.Extensions;

public static class MappingExtensions
{
	public static NopAjaxFiltersSettingsModel ToModel(this NopAjaxFiltersSettings nopAjaxFiltersSettings)
	{
		return nopAjaxFiltersSettings.MapTo<NopAjaxFiltersSettings, NopAjaxFiltersSettingsModel>();
	}

	public static NopAjaxFiltersSettings ToEntity(this NopAjaxFiltersSettingsModel nopAjaxFiltersSettingsModel)
	{
		return nopAjaxFiltersSettingsModel.MapTo<NopAjaxFiltersSettingsModel, NopAjaxFiltersSettings>();
	}
}
