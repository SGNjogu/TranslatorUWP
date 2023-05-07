namespace SpeechlyTouch.Messages
{
    public class DevicesMessage
    {
        public bool SaveDevicesSettings { get; set; }
        public bool ReloadAudioInputDevices { get; set; }
        public bool ReloadAudioOuputDevices { get; set; }
        public bool ReloadDashboardAudioDevices { get; set; }
        public bool ShowDevicesErrorDialog { get; set; }
        public string DevicesErrorMessage { get; set; }
        public bool CloseDevicesErrorDialog { get; set; }
    }
}
