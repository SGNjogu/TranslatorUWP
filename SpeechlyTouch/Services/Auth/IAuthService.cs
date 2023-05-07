using SpeechlyTouch.Models;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.Auth
{
    public interface IAuthService
    {
        UserLicense UserLicense { get; }
        Task<AuthenticationObject> AttemptSilentLogin();
        Task<OrganizationBranding> GetOrganizationBranding();
        Task<AuthenticationObject> Login();
        Task Logout();
        string IdToken { get; }
        string ErrorMessage { get; }
        Task GetOrganizationSettings();
        Task GetOrganizationTags();
        Task GetCustomTags();
        Task UpdateOrgQuestions();
        Task UpdateBackendLanguages();
    }
}
