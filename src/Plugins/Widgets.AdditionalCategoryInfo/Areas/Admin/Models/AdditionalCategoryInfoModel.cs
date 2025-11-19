using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.AdditionalCategoryInfo.Areas.Admin.Models;

public class AdditionalCategoryInfoModel
{
	public int CategoryId { get; set; }

	[NopResourceDisplayName("Plugins.Widgets.AdditionalCategoryInfo.Fields.Active")]
	public bool Active { get; set; }

	[UIHint("RichEditor")]
	[NopResourceDisplayName("Plugins.Widgets.AdditionalCategoryInfo.Fields.AdditionalInfoField")]
	public string AdditionalInfoField { get; set; }
}
