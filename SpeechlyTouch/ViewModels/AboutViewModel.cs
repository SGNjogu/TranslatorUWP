using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.Settings;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace SpeechlyTouch.ViewModels
{
    public class AboutViewModel : ObservableObject
    {
        private string _appVersion;
        public string AppVersion
        {
            get { return _appVersion; }
            set { SetProperty(ref _appVersion, value); }
        }

        private string _dataConsentStatus;
        public string DataConsentStatus
        {
            get { return _dataConsentStatus; }
            set { SetProperty(ref _dataConsentStatus, value); }
        }

        private readonly ISettingsService _settingsService;
        private ResourceLoader _resourceLoader;

        public AboutViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _resourceLoader = ResourceLoader.GetForCurrentView();
            StrongReferenceMessenger.Default.Register<InternationalizationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            Initialize();
        }

        private async void HandleMessage(InternationalizationMessage message)
        {
            if (!string.IsNullOrEmpty(message.LanguageCode))
            {
                await Task.Delay(300);
                Initialize();
            }
        }

        private async void Initialize()
        {
            var user = await _settingsService.GetUser();

            if (user.DataConsentStatus)
                DataConsentStatus = _resourceLoader.GetString("AboutPage_DataConsent");
            else
                DataConsentStatus = _resourceLoader.GetString("AboutPage_NoDataConsent");

            AppVersion = Constants.GetSoftwareVersion();
        }
    }
}
