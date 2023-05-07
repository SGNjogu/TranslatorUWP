using SpeechlyTouch.Models;

namespace SpeechlyTouch.Messages
{
    public class ChatScrollToMessage 
    {
        public Chat ScrollTo { get; set; }
        public bool IsPercentage { get; set; } = false;
        public double Percentage { get; set; }
    }
}
