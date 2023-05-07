namespace SpeechlyTouch.Messages
{
    public class ConnectivityMessage
    {
        public bool ConnectionPresent { get; set; }
        public bool ConnectionLost { get; set; }
        public bool ConnectionStable { get; set; }
        public bool ConnectionSlow { get; set; }
    }
}
