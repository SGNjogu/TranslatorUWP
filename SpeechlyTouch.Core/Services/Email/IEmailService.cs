using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.Email
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, string mailGunApiKey);
    }
}
