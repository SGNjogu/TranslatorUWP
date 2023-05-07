using Microsoft.Toolkit.Mvvm.ComponentModel;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.Services.Settings;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class LicenseViewModel : ObservableObject
    {
        private string _licenceType;
        public string LicenceType
        {
            get { return _licenceType; }
            set { SetProperty(ref _licenceType, value); }
        }

        private string _licenseExpiryDate;
        public string LicenseExpiryDate
        {
            get { return _licenseExpiryDate; }
            set { SetProperty(ref _licenseExpiryDate, value); }
        }

        private string _appVersion;
        public string AppVersion
        {
            get { return _appVersion; }
            set { SetProperty(ref _appVersion, value); }
        }

        private string _resellerName;
        public string ResellerName
        {
            get { return _resellerName; }
            set { SetProperty(ref _resellerName, value); }
        }

        private string _resellerEmail;
        public string ResellerEmail
        {
            get { return _resellerEmail; }
            set { SetProperty(ref _resellerEmail, value); }
        }

        private Visibility _resellerInfoVisibility;
        public Visibility ResellerInfoVisibility
        {
            get { return _resellerInfoVisibility; }
            set { SetProperty(ref _resellerInfoVisibility, value); }
        }

        private readonly ISettingsService _settingsService;
        private readonly IDataService _dataService;

        public LicenseViewModel(ISettingsService settingsService, IDataService dataService)
        {
            _settingsService = settingsService;
            _dataService = dataService;
            Initialize();
            ResellerInfoVisibility = Visibility.Collapsed;
        }

        private async void Initialize()
        {
            var user = await _settingsService.GetUser();
            LicenceType = user.PolicyType;
            LicenseExpiryDate = user.PolicyExpiryDate.ToString();
            AppVersion = Constants.GetSoftwareVersion();

            var reseller = await _dataService.GetFirstResellerInfoAsync();
            if (reseller != null)
            {
                ResellerName = reseller.Name;
                ResellerEmail = reseller.Email;
            }

            if (!string.IsNullOrEmpty(ResellerName) && !string.IsNullOrEmpty(ResellerEmail))
                ResellerInfoVisibility = Visibility.Visible;
        }
    }
}
