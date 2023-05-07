using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechlyTouch.DataService.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserFeedback : BaseModel
    {
        [JsonProperty("sessionNumber")]
        public string SessionNumber { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("reasonForRating")]
        public string ReasonForRating { get; set; } = null;

        [JsonProperty("comment")]
        public string Comment { get; set; } = null;

        [JsonIgnore]
        public bool SyncedToServer { get; set; } = false;

        [JsonIgnore]
        public int FeedbackType { get; set; }
    }
}
