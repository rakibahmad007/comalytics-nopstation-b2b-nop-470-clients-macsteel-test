using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName == "Admin")
            {
                viewLocations = new[] {
                    $"/Plugins/NopStation.Plugin.B2B.ManageB2CandB2BCustomer/Areas/Admin/Views/Shared/{{0}}.cshtml",
                    $"/Plugins/NopStation.Plugin.B2B.ManageB2CandB2BCustomer/Areas/Admin/Views/{{1}}/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
            else
            {
                viewLocations = new[] {
                    $"/Plugins/NopStation.Plugin.B2B.ManageB2CandB2BCustomer/Views/Shared/{{0}}.cshtml",
                    $"/Plugins/NopStation.Plugin.B2B.ManageB2CandB2BCustomer/Views/{{1}}/{{0}}.cshtml",
                    $"/Views/Checkout/{{0}}.cshtml"
                }.Concat(viewLocations);
            }

            return viewLocations;
        }
    }
}