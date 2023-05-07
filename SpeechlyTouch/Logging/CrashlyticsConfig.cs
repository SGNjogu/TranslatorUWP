using Microsoft.AppCenter.Crashes;
using SpeechlyTouch.Services.Settings;
using System;
using System.Threading.Tasks;

namespace SpeechlyTouch.Logging
{
    public class CrashlyticsConfig : ICrashlytics
    {
        private readonly ISettingsService _settingsService;
        public CrashlyticsConfig(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<ErrorAttachmentLog[]> Attachments()
        {
            var settings = await _settingsService.GetUser();

            var environment = "production";

#if DEBUG
            environment = "development";
#elif STAGING
            environment = "staging";
#endif

            var isLoggedIn = Convert.ToBoolean(settings.IsLoggedIn);

            if (isLoggedIn)
            {
                var userId = settings.UserID;
                var userName = settings.UserName;
                var email = settings.UserEmail;

                return new ErrorAttachmentLog[]
                {
                    ErrorAttachmentLog.AttachmentWithText(
                        $"Username: {userName} \n" +
                        $"UserId: {userId} \n" +
                        $"Email: {email} \n" +
                        $"AppVersion: {Constants.GetSoftwareVersion()} \n" +
                        $"Environment: {environment} \n", "Details.txt")
                };
            }

            return new ErrorAttachmentLog[]
            {
                ErrorAttachmentLog.AttachmentWithText(
                        $"AppVersion: {Constants.GetSoftwareVersion()} \n" +
                        $"Environment: {environment}", "Details.txt")
            };
        }
    }
}
