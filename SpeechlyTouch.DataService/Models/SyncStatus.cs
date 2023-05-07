using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechlyTouch.DataService.Models
{
    public class SyncStatus : BaseModel
    {
        public int LocalSessionId { get; set; }

        public bool SessionSync { get; set; } = false;

        public bool TranscriptionsSync { get; set; } = false;

        public bool DevicesSync { get; set; } = false;
    }
}
