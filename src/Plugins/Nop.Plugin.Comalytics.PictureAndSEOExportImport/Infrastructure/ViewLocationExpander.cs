using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Comalytics.PictureAndSEOExportImport.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName == "Admin")
            {
                viewLocations = new[] {
                    $"~/Plugins/Comalytics.PictureAndSEOExportImport/Areas/Admin/Views/Shared/{{0}}.cshtml",
                    $"~/Plugins/Comalytics.PictureAndSEOExportImport/Areas/Admin/Views/{{1}}/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
            else
            {
                viewLocations = new[] {
                    $"~/Plugins/Comalytics.PictureAndSEOExportImport/Views/Shared/{{0}}.cshtml",
                    $"~/Plugins/Comalytics.PictureAndSEOExportImport/Views/{{1}}/{{0}}.cshtml"
                }.Concat(viewLocations);

                if (context.Values.TryGetValue(THEME_KEY, out string theme))
                {
                    viewLocations = new[] {
                        $"~/Plugins/Comalytics.PictureAndSEOExportImport/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                        $"~/Plugins/Comalytics.PictureAndSEOExportImport/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                        $"~/Plugins/Comalytics.PictureAndSEOExportImport/Themes/{theme}/Views/{{0}}.cshtml"
                    }.Concat(viewLocations);
                }
            }

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }
    }
}
