using System.Collections.Generic;

namespace SpeechlyTouch.Core.Domain
{
    public class BulkTranscriptions
    {
        public int SessionId { get; set; }
        public List<Transcription> Transcriptions { get; set; }
    }
}
