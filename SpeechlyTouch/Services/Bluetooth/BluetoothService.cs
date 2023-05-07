using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;

namespace SpeechlyTouch.Services.Bluetooth
{
    public class BluetoothService : IBluetoothService
    {
        public event BluetoothDeviceAddedEvent BluetoothDeviceAddedEvent;
        public event BluetoothDeviceRemovedEvent BluetoothDeviceRemovedEvent;
        public event BluetoothEnumerationCompletedEvent BluetoothEnumerationCompletedEvent;
        public Radio BluetoothRadio { get; set; }
        private DeviceWatcher _deviceWatcher;

        public BluetoothService()
        {
            string BleSelector = "System.Devices.DevObjectType:=5 AND (System.Devices.Aep.ProtocolId:=\"{BB7BB05E-5972-42B5-94FC-76EAA7084D49}\" OR System.Devices.Aep.ProtocolId:=\"{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}\")";
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.ProtocolId" };
            _deviceWatcher = DeviceInformation.CreateWatcher(BleSelector, requestedProperties, DeviceInformationKind.AssociationEndpoint);
            _deviceWatcher.Added += OnDeviceAdded;
            _deviceWatcher.Updated += OnDeviceUpdated;
            _deviceWatcher.Removed += OnDeviceRemoved;
            _deviceWatcher.EnumerationCompleted += EnumerationCompleted;
        }

        private void OnDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            
        }

        public void StartScan()
        {
            _deviceWatcher.Start();
        }

        public void StopScan()
        {
            _deviceWatcher.Stop();
        }

        public void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            BluetoothDeviceRemovedEventArgs eventArgs = new BluetoothDeviceRemovedEventArgs
            {
                DeviceWatcher = sender,
                DeviceInformationUpdate = args
            };
            BluetoothDeviceRemovedEvent?.Invoke(this, eventArgs);
        }

        public void OnDeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            BluetoothDeviceAddedEventArgs eventArgs = new BluetoothDeviceAddedEventArgs
            {
                DeviceWatcher = sender,
                DeviceInformation = args
            };

            BluetoothDeviceAddedEvent?.Invoke(this, eventArgs);
        }

        private void EnumerationCompleted(DeviceWatcher sender, object args)
        {
            BluetoothEnumerationCompletedEventArgs eventArgs = new BluetoothEnumerationCompletedEventArgs
            {
                DeviceWatcher = sender,
                Args = args
            };

            BluetoothEnumerationCompletedEvent?.Invoke(this, eventArgs);
        }

        public async Task<DeviceInformationCollection> GetPairedBluetoothDevices()
        {
            var selector = BluetoothDevice.GetDeviceSelectorFromPairingState(true);
            var devices = await DeviceInformation.FindAllAsync(selector);
            return devices;
        }

        public bool IsBluetoothOn()
        {
            return BluetoothRadio != null && BluetoothRadio.State == RadioState.On;
        }

        public async Task<bool> IsBluetoothSupported()
        {
            var radios = await Radio.GetRadiosAsync();
            BluetoothRadio = radios.FirstOrDefault(radio => radio.Kind == RadioKind.Bluetooth);
            return BluetoothRadio != null;
        }

        public async Task<bool> Pair(DeviceInformation device)
        {
            var pairing = await device.Pairing.PairAsync();

            if (pairing.Status == DevicePairingResultStatus.Paired || pairing.Status == DevicePairingResultStatus.AlreadyPaired)
                return true;

            return false;
        }
    }
}
