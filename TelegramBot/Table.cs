using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    internal class Table
    {

    }



public class Schedule
    {
        [JsonProperty("Pair")]
        public string? Pair { get; set; }

        [JsonProperty("Monday")]
        public string? Monday { get; set; }

        [JsonProperty("Tuesday")]
        public string? Tuesday { get; set; }

        [JsonProperty("Wednesday")]
        public string? Wednesday { get; set; }

        [JsonProperty("Thursday")]
        public string? Thursday { get; set; }

        [JsonProperty("Friday")]
        public string? Friday { get; set; }

        [JsonProperty("links")]
        public Links? Links { get; set; }
    }

    public class Links
    {
        [JsonProperty("Monday_link")]
        public string? MondayLink { get; set; }

        [JsonProperty("Tuesday_link")]
        public string? TusdayLink { get; set; }

        [JsonProperty("Wednesday_link")]
        public string? WednesdayLink { get; set; }

        [JsonProperty("Thursday_link")]
        public string? ThursdayLink { get; set; }

        [JsonProperty("Friday_link")]
        public string? FridayLink { get; set; }
    }

}
