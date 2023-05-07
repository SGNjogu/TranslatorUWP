using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechlyTouch.Core.Domain
{
    public class PlaybackUsageTracking : BaseEntity
    {
        public int UserId { get; set; }
        public string SessionNumber { get; set; }
        public double PlaybackSeconds { get; set; }
    }
}
