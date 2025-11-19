using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Web.Framework;

namespace Nop.Plugin.Misc.ErpWebhook.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (context.AreaName == AreaNames.ADMIN)
        {
            viewLocations = new string[] { $"~/Plugins/Misc.ErpWebhook/Areas/Admin/Views/{{1}}/{{0}}.cshtml" }.Concat(viewLocations);
            viewLocations = new string[] { $"~/Plugins/Misc.ErpWebhook/Areas/Admin/Views/Shared/{{0}}.cshtml" }.Concat(viewLocations);
        }
        else
        {
            viewLocations = new string[] { $"/Plugins/Misc.ErpWebhook/Views/{{1}}/{{0}}.cshtml" }.Concat(viewLocations);
            viewLocations = new string[] { $"/Plugins/Misc.ErpWebhook/Views/Shared/{{0}}.cshtml" }.Concat(viewLocations);
        }
        return viewLocations;
    }

    public void PopulateValues(ViewLocationExpanderContext context)
    {
    }
}
