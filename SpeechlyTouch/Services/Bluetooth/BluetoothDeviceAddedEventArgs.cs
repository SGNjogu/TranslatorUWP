using System;
using Windows.Devices.Enumeration;

namespace SpeechlyTouch.Services.Bluetooth
{
    public class BluetoothDeviceAddedEventArgs : EventArgs
    {
        public DeviceWatcher DeviceWatcher { get; set; } 
        public DeviceInformation DeviceInformation { get; set; }
    }

    public delegate void BluetoothDeviceAddedEvent(object sender, BluetoothDeviceAddedEventArgs args);
}
