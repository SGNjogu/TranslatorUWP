using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SpeechlyTouch.Core.Domain
{
    public class AppVersion
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("forceUpdate")]
        public bool IsForcedUpdate { get; set; } = false;

        [JsonProperty("releaseNotes")]
        public string ReleaseNotes { get; set; }

        [JsonProperty("appType")]
        public int AppType { get; set; }

        [JsonProperty("releaseNotesList")]
        public List<string> ReleaseNotesList { get; set; }

        [JsonProperty("releaseDate")]
        public DateTime ReleaseDate { get; set; }
    }
}
