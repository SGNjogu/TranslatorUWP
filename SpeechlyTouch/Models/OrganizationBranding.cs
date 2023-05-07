using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace SpeechlyTouch.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OrganizationBranding : ObservableObject
    {
        [JsonProperty("organizationId")]
        public int OrganizationId { get; set; }

        [JsonProperty("logoImageURL")]
        public string LogoImageURL { get; set; }

        [JsonProperty("websiteURL")]
        public string WebsiteURL { get; set; }

        [JsonProperty("organizationName")]
        public string OrganizationName { get; set; }
    }
}
