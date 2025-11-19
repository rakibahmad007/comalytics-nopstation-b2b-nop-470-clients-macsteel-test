using System;
using Newtonsoft.Json;
using Nop.Core;

namespace Nop.Plugin.Misc.ErpWebhook.Models.ErpDeliveryDates
{
    public class DeliveryDatesModel
    {
        // Copy constructor for deep copy
        public DeliveryDatesModel Clone()
        {
            var clone = new DeliveryDatesModel();

            clone.SalesOrgOrPlant = SalesOrgOrPlant;
            clone.CutOffTime = CutOffTime;
            clone.City = City;
            clone.AllWeekIndicator = AllWeekIndicator;
            clone.Monday = Monday;
            clone.Tuesday = Tuesday;
            clone.Wednesday = Wednesday;
            clone.Thursday = Thursday;
            clone.Friday = Friday;
            clone.DelDate1 = DelDate1;
            clone.DelDate2 = DelDate2;
            clone.DelDate3 = DelDate3;
            clone.DelDate4 = DelDate4;
            clone.DelDate5 = DelDate5;
            clone.DelDate6 = DelDate6;
            clone.DelDate7 = DelDate7;
            clone.DelDate8 = DelDate8;
            clone.DelDate9 = DelDate9;
            clone.DelDate10 = DelDate10;
            clone.CreatedOn = CreatedOn;
            clone.UpdatedOn = UpdatedOn;
            clone.Deleted = Deleted;
            clone.IsFullLoadRequired = IsFullLoadRequired;

            return clone;
        }

        public string SalesOrgOrPlant { get; set; }
        public string CutOffTime { get; set; }
        public string City { get; set; }
        public string AllWeekIndicator { get; set; }
        public string Monday { get; set; }
        public string Tuesday { get; set; }
        public string Wednesday { get; set; }
        public string Thursday { get; set; }
        public string Friday { get; set; }
        [JsonProperty("Del_Date1")]
        public DateTime? DelDate1 { get; set; }

        [JsonProperty("Del_Date2")]
        public DateTime? DelDate2 { get; set; }

        [JsonProperty("Del_Date3")]
        public DateTime? DelDate3 { get; set; }

        [JsonProperty("Del_Date4")]
        public DateTime? DelDate4 { get; set; }

        [JsonProperty("Del_Date5")]
        public DateTime? DelDate5 { get; set; }

        [JsonProperty("Del_Date6")]
        public DateTime? DelDate6 { get; set; }

        [JsonProperty("Del_Date7")]
        public DateTime? DelDate7 { get; set; }

        [JsonProperty("Del_Date8")]
        public DateTime? DelDate8 { get; set; }

        [JsonProperty("Del_Date9")]
        public DateTime? DelDate9 { get; set; }

        [JsonProperty("Del_Date10")]
        public DateTime? DelDate10 { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string Deleted { get; set; }

        [JsonProperty("FullLoad")]
        public string IsFullLoadRequired { get; set; }
    }
}
