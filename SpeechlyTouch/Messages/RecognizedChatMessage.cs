using SpeechlyTouch.Models;
using System.Collections.Generic;

namespace SpeechlyTouch.Messages
{
    public class RecognizedChatMessage
    {
        public bool IsChatList { get; set; }
        public int SessionId { get; set; }
        public Chat Chat { get; set; }
        public List<Chat> ChatList { get; set; }
        public bool IsCopyPasteEnabled { get; set; }
    }
}
