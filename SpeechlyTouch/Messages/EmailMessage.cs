namespace SpeechlyTouch.Messages
{
    public class EmailMessage
    {
        public bool CloseEmailPopup { get; set; }
        public bool SendEmail { get; set; }
        public string EmailingAddress { get; set; }
    }
}
