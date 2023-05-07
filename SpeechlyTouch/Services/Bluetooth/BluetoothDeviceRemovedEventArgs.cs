using System;
using Windows.Devices.Enumeration;

namespace SpeechlyTouch.Services.Bluetooth
{
    public class BluetoothDeviceRemovedEventArgs : EventArgs
    {
        public DeviceWatcher DeviceWatcher { get; set; }
        public DeviceInformationUpdate DeviceInformationUpdate { get; set; }
    }

    public delegate void BluetoothDeviceRemovedEvent(object sender, BluetoothDeviceRemovedEventArgs args);
}
