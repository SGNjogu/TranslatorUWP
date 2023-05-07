using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechlyTouch.DataService.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Device : BaseModel
    {
        [JsonProperty("sessionId")]
        public int SessionId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("certified")]
        public bool Certified { get; set; } = false;

        [JsonIgnore]
        public bool SyncedToServer { get; set; } = false;
    }
}
