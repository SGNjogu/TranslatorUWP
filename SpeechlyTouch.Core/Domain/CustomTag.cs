using Newtonsoft.Json;

namespace SpeechlyTouch.Core.Domain
{
    public class CustomTag
    {
        [JsonProperty("tagName")]
        public string TagName { get; set; }
    }
}
