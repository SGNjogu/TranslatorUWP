using System;

namespace SpeechlyTouch.Core.Domain
{
    public class CognitiveServiceEndpoint
    {
        public int id { get; set; }
        public string name { get; set; }
        public string region { get; set; }
        public string accessKey { get; set; }
        public int clientCount { get; set; }
        public DateTime createdAt { get; set; }
    }
}
