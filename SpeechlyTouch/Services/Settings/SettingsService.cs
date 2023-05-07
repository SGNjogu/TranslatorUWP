using SpeechlyTouch.Helpers;
using SpeechlyTouch.Models;
using System;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        Windows.Storage.ApplicationDataContainer applicationData;
        public SettingsService()
        {
            applicationData = Windows.Storage.ApplicationData.Current.RoamingSettings;
        }

        public string Passcode
        {
            get
            {
                if (applicationData.Values["Passcode"] != null)
                    return applicationData.Values["Passcode"].ToString();
                else
                    return null;
            }
            set { applicationData.Values["Passcode"] = value; }
        }

        public int? AdminModeTimeout
        {
            get
            {
                if (applicationData.Values["AdminModeTimeout"] != null)
                    return (int)applicationData.Values["AdminModeTimeout"];
                else
                    return null;
            }
            set { applicationData.Values["AdminModeTimeout"] = value; }
        }

        public string DefaultTranslationLanguageCode
        {
            get
            {
                if (applicationData.Values["DefaultTranslationLanguageCode"] != null)
                    return applicationData.Values["DefaultTranslationLanguageCode"].ToString();
                else
                    return null;
            }
            set { applicationData.Values["DefaultTranslationLanguageCode"] = value; }
        }

        public string TargetTranslationLanguageCode
        {
            get
            {
                if (applicationData.Values["TargetTranslationLanguageCode"] != null)
                    return applicationData.Values["TargetTranslationLanguageCode"].ToString();
                else
                    return null;
            }
            set { applicationData.Values["TargetTranslationLanguageCode"] = value; }
        }

        public string ApplicationLanguageCode
        {
            get
            {
                if (applicationData.Values["ApplicationLanguage"] != null)
                    return applicationData.Values["ApplicationLanguage"].ToString();
                else
                    return null;
            }
            set { applicationData.Values["ApplicationLanguage"] = value; }
        }

        public string QuickViewLanguages
        {
            get
            {
                if (applicationData.Values["QuickViewLanguages"] != null)
                    return applicationData.Values["QuickViewLanguages"].ToString();
                else
                    return null;
            }
            set { applicationData.Values["QuickViewLanguages"] = value; }
        }

        private string UserObject
        {
            get
            {
                if (applicationData.Values["UserObject"] != null)
                    return applicationData.Values["UserObject"].ToString();
                else
                    return null;
            }
            set { applicationData.Values["UserObject"] = value; }
        }

        public bool IsUserLoggedIn
        {
            get
            {
                if (applicationData.Values["IsUserLoggedIn"] != null)
                    return Convert.ToBoolean(applicationData.Values["IsUserLoggedIn"].ToString());
                else
                    return false;
            }
            set { applicationData.Values["IsUserLoggedIn"] = value.ToString(); }
        }

        public bool IsUserDoneSettingUp
        {
            get
            {
                if (applicationData.Values["IsUserDoneSettingUp"] != null)
                    return Convert.ToBoolean(applicationData.Values["IsUserDoneSettingUp"].ToString());
                else
                    return false;
            }
            set { applicationData.Values["IsUserDoneSettingUp"] = value.ToString(); }
        }

        public bool IsResetPasscodeEmailSent
        {
            get
            {
                if (applicationData.Values["IsResetPasscodeEmailSent"] != null)
                    return Convert.ToBoolean(applicationData.Values["IsResetPasscodeEmailSent"].ToString());
                else
                    return false;
            }
            set { applicationData.Values["IsResetPasscodeEmailSent"] = value.ToString(); }
        }

        public async Task SaveUser(User user)
        {
            var jsonString = await JsonConverter.ReturnJsonStringFromObject(user);
            UserObject = jsonString;
        }

        public async Task<User> GetUser()
        {
            if (string.IsNullOrEmpty(UserObject))
                return null;

            var user = await JsonConverter.ReturnObjectFromJsonString<User>(UserObject);
            return user;
        }

        public string CurrentEndpointId
        {
            get
            {
                if (applicationData.Values["CurrentEndpointId"] != null)
                    return applicationData.Values["CurrentEndpointId"].ToString();
                else
                    return null;
            }
            set { applicationData.Values["CurrentEndpointId"] = value; }
        }

        public string DefaultPlaybackLanguage
        {
            get
            {
                if (applicationData.Values["DefaultPlaybackLanguage"] != null)
                    return applicationData.Values["DefaultPlaybackLanguage"].ToString();
                else
                    return null;
            }
            set { applicationData.Values["DefaultPlaybackLanguage"] = value; }
        }

        public bool IsCheckedSingleDevice
        {
            get
            {
                if (applicationData.Values["IsCheckedSingleDevice"] != null)
                    return Convert.ToBoolean(applicationData.Values["IsCheckedSingleDevice"].ToString());
                else
                    return true;
            }
            set { applicationData.Values["IsCheckedSingleDevice"] = value.ToString(); }
        }

        public bool IsEnabledAutoLanguageSwitch
        {
            get
            {
                if (applicationData.Values["IsEnabledAutoLanguageSwitch"] != null)
                    return Convert.ToBoolean(applicationData.Values["IsEnabledAutoLanguageSwitch"].ToString());
                else
                    return false;
            }
            set { applicationData.Values["IsEnabledAutoLanguageSwitch"] = value.ToString(); }
        }

        public double TranscriptionsFontSize
        {
            get
            {
                if (applicationData.Values["TranscriptionsFontSize"] != null)
                    return Convert.ToDouble(applicationData.Values["TranscriptionsFontSize"].ToString());
                else
                    return 14;
            }
            set { applicationData.Values["TranscriptionsFontSize"] = value.ToString(); }
        }
    }
}
