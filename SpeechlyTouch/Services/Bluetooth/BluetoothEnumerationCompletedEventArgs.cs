using System;
using Windows.Devices.Enumeration;

namespace SpeechlyTouch.Services.Bluetooth
{
    public class BluetoothEnumerationCompletedEventArgs : EventArgs
    {
        public DeviceWatcher DeviceWatcher { get; set; } 
        public object Args { get; set; }
    }

    public delegate void BluetoothEnumerationCompletedEvent(object sender, BluetoothEnumerationCompletedEventArgs args);
}
