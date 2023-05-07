namespace SpeechlyTouch.Models
{
    public class SignalRTranslateMessage
    {
        public string ConnectionId { get; set; } = string.Empty;
        public bool CanTranslate { get; set; } = false;
        public string UserEmail { get; set; } = string.Empty;
    }
}
