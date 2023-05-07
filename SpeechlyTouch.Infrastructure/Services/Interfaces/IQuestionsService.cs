using System.Collections.Generic;
using System.Threading.Tasks;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface IQuestionsService
    {
        Task<List<Core.Domain.UserQuestion>> GetUserQuestions(int userId, string token);
        Task<Core.Domain.UserQuestion> CreateUserQuestion(int userId, Core.Domain.UserQuestion userQuestion, string token);

        Task<bool> DeleteUserQuestion(int userId, Core.DTO.UserQuestionsDTO userQuestionDTO, string token);
    }
}