using SpeechlyTouch.Models;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.Settings
{
    public interface ISettingsService
    {
        string Passcode { get; set; }
        int? AdminModeTimeout { get; set; }
        string DefaultTranslationLanguageCode { get; set; }
        string TargetTranslationLanguageCode { get; set; }
        string ApplicationLanguageCode { get; set; }
        string QuickViewLanguages { get; set; }
        string CurrentEndpointId { get; set; }
        bool IsUserLoggedIn { get; set; }
        bool IsUserDoneSettingUp { get; set; }
        bool IsResetPasscodeEmailSent { get; set; }
        Task SaveUser(User user);
        Task<User> GetUser();
        string DefaultPlaybackLanguage { get; set; }
        bool IsCheckedSingleDevice { get; set; }
        bool IsEnabledAutoLanguageSwitch { get; set; }
        double TranscriptionsFontSize { get; set; }
    }
}
