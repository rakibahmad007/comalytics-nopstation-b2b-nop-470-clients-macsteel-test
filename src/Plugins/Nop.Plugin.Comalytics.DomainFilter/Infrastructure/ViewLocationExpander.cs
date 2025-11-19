using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Comalytics.DomainFilter.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            viewLocations = new string[] { $"/Plugins/Comalytics.DomainFilter/Views/{{1}}/{{0}}.cshtml" }.Concat(viewLocations);
            viewLocations = new string[] { $"/Plugins/Comalytics.DomainFilter/Views/Shared/{{0}}.cshtml" }.Concat(viewLocations);

            if (context.Values.TryGetValue(THEME_KEY, out string theme))
            {
                viewLocations = new[] {
                    $"/Plugins/Comalytics.DomainFilter/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                    $"/Plugins/Comalytics.DomainFilter/Themes/{theme}/Views/Shared/{{0}}.cshtml"
                }.Concat(viewLocations);
            }

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }
    }
}
