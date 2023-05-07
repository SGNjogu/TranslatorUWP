namespace SpeechlyTouch.Services.Connectivity
{
    public interface IConnectivityService
    {
        bool IsInternetConnectionAvailable { get; set; }

        event ConnectionChangedEvent ConnectionChangedEvent;

        bool IsConnectionAvailable();
    }
}
