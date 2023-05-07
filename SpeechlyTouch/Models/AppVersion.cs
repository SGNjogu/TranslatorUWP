using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpeechlyTouch.Models
{
    public class AppVersion
    {
        [JsonProperty("version")]
        public string Version { get; set; }
        public bool IsLatestVersion { get; set; } = true;
        public bool IsUnsupportedVersion { get; set; } = false;
        [JsonProperty("forceUpdate")]
        public bool IsForcedUpdate { get; set; } = false;
        [JsonProperty("appType")]
        public int AppType { get; set; }
        [JsonProperty("releaseNotesList")]
        public List<string> ReleaseNotesList { get; set; }
        [JsonProperty("releaseDate")]
        public DateTime ReleaseDate { get; set; }
    }
}
