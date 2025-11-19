using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services;
using Nop.Services.Localization;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Helpers
{
    public class CommonHelper : ICommonHelper
    {
        private readonly ILocalizationService _localizationService;

        public CommonHelper(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }
        public virtual async Task PrepareDefaultItemAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null, string defaultItemValue = "0", bool selected = false)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //whether to insert the first special item for the default value
            if (!withSpecialDefaultItem)
                return;

            //prepare item text
            defaultItemText ??= await _localizationService.GetResourceAsync("Admin.Common.Select");

            //insert this default item at first
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = defaultItemValue, Selected = selected });
        }

        public async Task PrepareDropdownFromEnumAsync<TEnum>(
            IList<SelectListItem> items, 
            TEnum enumItem, 
            bool withSpecialDefaultItem = true, 
            string defaultItemText = null, 
            string defaultItemValue = "0", 
            bool selected = false) 
            where TEnum : struct, Enum
        {
            ArgumentNullException.ThrowIfNull(items);

            var availableOptions = await enumItem.ToSelectListAsync(false);
            foreach (var option in availableOptions)
            {
                items.Add(option);
            }
            defaultItemText ??= await _localizationService.GetResourceAsync("Admin.Common.All");

            await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText , defaultItemValue, selected);
        }
    }
}