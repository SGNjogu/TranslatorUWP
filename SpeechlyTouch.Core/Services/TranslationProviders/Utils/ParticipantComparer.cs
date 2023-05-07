using SpeechlyTouch.Core.Domain;
using System.Collections.Generic;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Utils
{
    public class ParticipantComparer : IEqualityComparer<Participant>
    {
        public bool Equals(Participant x, Participant y)
        {
            return x.Guid.Equals(y.Guid);
        }

        public int GetHashCode(Participant obj)
        {
            return obj.Guid.GetHashCode();
        }
    }
}
