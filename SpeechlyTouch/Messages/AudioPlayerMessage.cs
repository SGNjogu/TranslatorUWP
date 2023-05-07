using SpeechlyTouch.DataService.Models;

namespace SpeechlyTouch.Messages
{
    public class AudioPlayerMessage
    {
        public Session Session { get; set; }
        public bool IsShowingAudioPlayer { get; set; }
    }
}
