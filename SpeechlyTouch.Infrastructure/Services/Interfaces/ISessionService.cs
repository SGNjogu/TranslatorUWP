using SpeechlyTouch.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface ISessionService
    {
        Task<Session> Create(Session session, string token);
        Task<IEnumerable<Session>> GetSessionsForUser(int userId, int organizationId, string token);
        Task<SessionNumber> GetSessionNumber(string token);
        Task<bool> CreateSessionEmail(string sessionNumber, string emailAddress, string token);
        Task<SessionTag> CreateSessionTag(SessionTag sessionTag, string token);
        Task<List<SessionTag>> GetSessionTags(int sessionId, string token);
    }
}