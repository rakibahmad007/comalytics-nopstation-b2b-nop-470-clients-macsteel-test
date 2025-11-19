using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Models.Configuration
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpWebhook.Fields.WebhookSecretKey")]
        public string WebhookSecretKey { get; set; }
        public bool WebhookSecretKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpWebhook.Fields.AccountPrefilterFacets")]
        public string AccountPrefilterFacets { get; set; }
        public bool AccountPrefilterFacets_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpWebhook.Fields.AccountsDefaultAllowOverspend")]
        public bool Accounts_Default_AllowOverspend { get; set; }
        public bool AccountsDefaultAllowOverspend_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpWebhook.Fields.OverrideLowStockActivityId")]
        public bool Override_LowStockActivityId { get; set; }
        public bool OverrideLowStockActivityId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpWebhook.Fields.LowStockActivityIdDefaultValue")]
        public int LowStockActivityId_DefaultValue { get; set; }
        public bool LowStockActivityIdDefaultValue_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpWebhook.Fields.OverrideBackorderModeId")]
        public bool Override_BackorderModeId { get; set; }
        public bool OverrideBackorderModeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpWebhook.Fields.BackorderModeIdDefaultValue")]
        public int BackorderModeId_DefaultValue { get; set; }
        public bool BackorderModeIdDefaultValue_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpWebhook.Fields.DefaultCountryThreeLetterIsoCode")]
        public string DefaultCountryThreeLetterIsoCode { get; set; }
        public bool DefaultCountryThreeLetterIsoCode_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; } = new List<SelectListItem>();
    }
}