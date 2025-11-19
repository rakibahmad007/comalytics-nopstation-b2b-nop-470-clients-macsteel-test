using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Widgets.AdditionalCategoryInfo.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    public void PopulateValues(ViewLocationExpanderContext context) { }

    public IEnumerable<string> ExpandViewLocations(
        ViewLocationExpanderContext context,
        IEnumerable<string> viewLocations
    )
    {
        viewLocations = new string[4]
        {
            "~/Plugins/Nop.Plugin.Widgets.AdditionalCategoryInfo/Views/{0}.cshtml",
            "~/Plugins/Nop.Plugin.Widgets.AdditionalCategoryInfo/Views/Shared/{0}.cshtml",
            "~/Plugins/Nop.Plugin.Widgets.AdditionalCategoryInfo/Views/{1}/{0}.cshtml",
            "~/Plugins/Nop.Plugin.Widgets.AdditionalCategoryInfo/Views/{2}/{1}/{0}.cshtml",
        }.Concat(viewLocations);
        if (context.Values.TryGetValue("nop.themename", out string theme))
        {
            viewLocations = new string[4]
            {
                "~/Plugins/Nop.Plugin.Widgets.AdditionalCategoryInfo/Themes/"
                    + theme
                    + "/Views/{0}.cshtml",
                "~/Plugins/Nop.Plugin.Widgets.AdditionalCategoryInfo/Themes/"
                    + theme
                    + "/Views/Shared/{0}.cshtml",
                "~/Plugins/Nop.Plugin.Widgets.AdditionalCategoryInfo/Themes/"
                    + theme
                    + "/Views/{1}/{0}.cshtml",
                "~/Plugins/Nop.Plugin.Widgets.AdditionalCategoryInfo/Themes/"
                    + theme
                    + "/Views/{2}/{1}/{0}.cshtml",
            }.Concat(viewLocations);
        }
        return viewLocations;
    }
}
