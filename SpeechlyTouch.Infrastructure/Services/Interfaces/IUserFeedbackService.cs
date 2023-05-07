using SpeechlyTouch.Core.Domain;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface IUserFeedbackService
    {
        Task<UserFeedback> Create(UserFeedback userFeedback, string token);
    }
}