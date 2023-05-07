using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;

namespace SpeechlyTouch.Services.Bluetooth
{
    public interface IBluetoothService
    {
        event BluetoothDeviceAddedEvent BluetoothDeviceAddedEvent;
        event BluetoothDeviceRemovedEvent BluetoothDeviceRemovedEvent;
        event BluetoothEnumerationCompletedEvent BluetoothEnumerationCompletedEvent;
        Radio BluetoothRadio { get; set; }

        void OnDeviceAdded(DeviceWatcher sender, DeviceInformation args);
        void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args);
        void StartScan();
        void StopScan();
        bool IsBluetoothOn();
        Task<bool> Pair(DeviceInformation device);
        Task<bool> IsBluetoothSupported();
        Task<DeviceInformationCollection> GetPairedBluetoothDevices();
    }
}
