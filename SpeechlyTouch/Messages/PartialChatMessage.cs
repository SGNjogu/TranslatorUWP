using Microsoft.Toolkit.Mvvm.ComponentModel;
using SpeechlyTouch.Models;

namespace SpeechlyTouch.Messages
{
    public class PartialChatMessage : ObservableObject
    {
        private int _sessionId;
        public int SessionId
        {
            get { return _sessionId; }
            set { SetProperty(ref _sessionId, value); }
        }

        private Chat _chat;
        public Chat Chat
        {
            get { return _chat; }
            set { SetProperty(ref _chat, value); }
        }
    }
}
