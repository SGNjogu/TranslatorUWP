using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechlyTouch.Core.Domain
{
    public class Device : BaseEntity
    {
        public int SessionId { get; set; }

        public string Name { get; set; }

        public bool Certified { get; set; } = false;

        public bool SyncedToServer { get; set; } = false;
    }
}
