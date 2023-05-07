using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace SpeechlyTouch.Models
{
    public class User : ObservableObject
    {
        public bool IsLoggedIn { get; set; } = false;

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }

        public string ImageUrl { get; set; }
        public byte[] ProfileImage { get; set; }
        public bool HasProfileImage { get; set; } = false;
        public string UserType { get; set; }

        private string _userEmail;
        public string UserEmail
        {
            get { return _userEmail; }
            set { SetProperty(ref _userEmail, value); }
        }

        private string _userDataExportEmail;
        public string UserDataExportEmail
        {
            get { return _userDataExportEmail; }
            set { SetProperty(ref _userDataExportEmail, value); }
        }

        public string UserID { get; set; }
        public int? UserIntID { get; set; }
        public string TenantID { get; set; }
        public string DomainName { get; set; }
        public string Organization { get; set; }
        public int OrganizationId { get; set; }
        public bool HasValidLicense { get; set; } = false;
        public string LicenseKey { get; set; }

        private string _policyType;
        public string PolicyType
        {
            get { return _policyType; }
            set { SetProperty(ref _policyType, value); }
        }

        public string IMCustomerId { get; set; }
        public string CurrentEndpointId { get; set; }
        public DateTime PolicyExpiryDate { get; set; }
        public string DeviceOneDefaultLanguageCode { get; set; }
        public string DeviceTwoDefaultLanguageCode { get; set; }
        public string DefaultPlaybackLanguage { get; set; } = "Language 1";
        public bool CanShowHelpTips { get; set; } = true;
        public bool DataConsentStatus { get; set; } = true;
        public bool IsJabraLocked { get; set; } = true;
    }
}
