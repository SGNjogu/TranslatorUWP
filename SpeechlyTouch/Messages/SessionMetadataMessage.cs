using System.Collections.ObjectModel;
using SpeechlyTouch.Models;

namespace SpeechlyTouch.Messages
{
    public class SessionMetadataMessage
    {
        public string SessionName { get; set; } = string.Empty;
        public ObservableCollection<string> CustomTags { get; set; }
        public ObservableCollection<SessionTag> SessionTags { get; set; }
    }
}
