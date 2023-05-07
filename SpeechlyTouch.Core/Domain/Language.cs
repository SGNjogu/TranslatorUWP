using System;
using System.Collections.Generic;

namespace SpeechlyTouch.Core.Domain
{
    public class Language : ICloneable
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public IEnumerable<string> Voice { get; set; }

        // TODO: Make this property nullable
        public string Flag { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            return $"Name: {Name} Code: {Code} Voice: {Voice}";
        }
    }
}
