using System;

namespace SpeechlyTouch.Services.Connectivity
{
    public enum ConnectionState
    {
        ConnectionPresent,
        ConnectionLost,
        ConnectionStable,
        ConnectionSlow
    }

    public class ConnectionChangedEventArgs : EventArgs
    {
        public ConnectionState ConnectionState { get; set; }
    }

    public delegate void ConnectionChangedEvent(object sender, ConnectionChangedEventArgs args);
}
