using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.Services.AudioProfileService;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.UI.Core;

namespace SpeechlyTouch.Services.Audio
{
    public class AudioService : IAudioService
    {
        private List<InputDevice> InputDevicesList { get; set; }
        private InputDevice PersonOneSelectedInputDevice { get; set; }
        private InputDevice PersonTwoSelectedInputDevice { get; set; }

        private List<OutputDevice> OutputDevicesList { get; set; }
        private OutputDevice PersonOneSelectedOutputDevice { get; set; }
        private OutputDevice PersonTwoSelectedOutputDevice { get; set; }

        private DeviceWatcher _inputDevicesWatcher;
        private DeviceWatcher _outputDevicesWatcher;

        private readonly IAudioProfileService _audioProfileService;
        private readonly IDataService _dataService;
        private readonly ICrashlytics _crashlytics;

        public AudioService(IAudioProfileService audioProfileService, IDataService dataService, ICrashlytics crashlytics)
        {
            _audioProfileService = audioProfileService;
            _dataService = dataService;
            _crashlytics = crashlytics;

            _inputDevicesWatcher = DeviceInformation.CreateWatcher(DeviceClass.AudioCapture);
            _inputDevicesWatcher.Added += OnAudioDeviceAdded;
            _inputDevicesWatcher.Removed += OnAudioDeviceRemoved;
            _outputDevicesWatcher = DeviceInformation.CreateWatcher(DeviceClass.AudioRender);
            _outputDevicesWatcher.Added += OnAudioDeviceAdded;
            _inputDevicesWatcher.Removed += OnAudioDeviceRemoved;

            _inputDevicesWatcher.Start();
            _outputDevicesWatcher.Start();

            _ = LoadDeviceLists();
        }

        private async void OnAudioDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            await LoadDeviceLists();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                StrongReferenceMessenger.Default.Send(new DevicesMessage { ReloadAudioInputDevices = true });
            });
        }

        private async void OnAudioDeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            await LoadDeviceLists();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                StrongReferenceMessenger.Default.Send(new DevicesMessage { ReloadAudioOuputDevices = true });
            });
        }

        public async Task LoadDeviceLists()
        {
            await LoadInputDevices();
            await LoadOutputDevices();
        }

        public async Task LoadInputDevices()
        {
            try
            {
                var existingInputDevices = await _dataService.GetInputDevicesAsync();
                InputDevicesList = await _audioProfileService.GetInputDevices();

                var personOneInputDevice = existingInputDevices.FirstOrDefault(d => d.Participant == "Person One");
                if (personOneInputDevice != null && InputDevicesList.Any(d => d.DeviceId == personOneInputDevice.DeviceId))
                {
                    PersonOneSelectedInputDevice = InputDevicesList.FirstOrDefault(d => d.DeviceId == personOneInputDevice.DeviceId);
                }
                else
                {
                    PersonOneSelectedInputDevice = null;
                }

                var personTwoInputDevice = existingInputDevices.FirstOrDefault(d => d.Participant == "Person Two");
                if (personTwoInputDevice != null && InputDevicesList.Any(d => d.DeviceId == personTwoInputDevice.DeviceId))
                {
                    PersonTwoSelectedInputDevice = InputDevicesList.FirstOrDefault(d => d.DeviceId == personTwoInputDevice.DeviceId);
                }
                else
                {
                    PersonTwoSelectedInputDevice = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public async Task LoadOutputDevices()
        {
            try
            {
                var existingOutputDevices = await _dataService.GetOuputDevicesAsync();
                OutputDevicesList = await _audioProfileService.GetOutputDevices();
                var personOneOutputDevice = existingOutputDevices.FirstOrDefault(d => d.Participant == "Person One");
                if (personOneOutputDevice != null && OutputDevicesList.Any(d => d.DeviceId == personOneOutputDevice.DeviceId))
                {
                    PersonOneSelectedOutputDevice = OutputDevicesList.FirstOrDefault(d => d.DeviceId == personOneOutputDevice.DeviceId);
                }
                else
                {
                    PersonOneSelectedOutputDevice = null;
                }

                var personTwoOutputDevice = existingOutputDevices.FirstOrDefault(d => d.Participant == "Person Two");
                if (personTwoOutputDevice != null && OutputDevicesList.Any(d => d.DeviceId == personTwoOutputDevice.DeviceId))
                {
                    PersonTwoSelectedOutputDevice = OutputDevicesList.FirstOrDefault(d => d.DeviceId == personTwoOutputDevice.DeviceId);
                }
                else
                {
                    PersonTwoSelectedOutputDevice = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public async Task<InputDevice> ParticipantOneInputDevice()
        {
            if (PersonOneSelectedInputDevice == null)
                await LoadInputDevices();
            return PersonOneSelectedInputDevice;
        }

        public async Task<InputDevice> ParticipantTwoInputDevice()
        {
            if (PersonTwoSelectedInputDevice == null)
                await LoadInputDevices();
            return PersonTwoSelectedInputDevice;
        }

        public async Task<OutputDevice> ParticipantOneOutputDevice()
        {
            if (PersonOneSelectedOutputDevice == null)
                await LoadOutputDevices();
            return PersonOneSelectedOutputDevice;
        }

        public async Task<OutputDevice> ParticipantTwoOutputDevice()
        {
            if (PersonTwoSelectedOutputDevice == null)
                await LoadOutputDevices();
            return PersonTwoSelectedOutputDevice;
        }

        public async Task<List<InputDevice>> InputDevices()
        {
            if (InputDevicesList == null)
                await LoadInputDevices();
            return InputDevicesList;
        }

        public async Task<List<OutputDevice>> OutputDevices()
        {
            if (OutputDevicesList == null)
                await LoadOutputDevices();
            return OutputDevicesList;
        }
    }
}
