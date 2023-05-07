using Newtonsoft.Json;

namespace SpeechlyTouch.DataService.Models
{
    public class OrganizationTag
    {
        [JsonProperty("id")]
        public int OrganizationTagId { get; set; }
        public int OrganizationId { get; set; }
        public string TagName { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsShownOnApp { get; set; }
    }
}
