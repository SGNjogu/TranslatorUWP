
using SpeechlyTouch.DataService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        /// <summary>
        /// Method to get all Sessions
        /// </summary>
        /// <returns>All Sessions</returns>
        public async Task<List<Session>> GetSessionsAsync()
        {
            var sessions = await Dataservice.Table<Session>().ToListAsync();
            return sessions.OrderByDescending(s => s.StartTime).ToList();
        }

        /// <summary>
        /// Method to get one Session
        /// </summary>
        /// <returns>Single Session</returns>
        public async Task<Session> GetOneSessionAsync(int sessionId)
        {
            return await Dataservice.Table<Session>().Where(s => s.ID == sessionId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets Sessions Count
        /// </summary>
        /// <returns>Number of Sessions</returns>
        public async Task<int> GetSessionCountAsync()
        {
            return await Dataservice.Table<Session>().CountAsync();
        }

        public async Task<bool> IsSessionValid(int sessionId)
        {
            int transcriptionsCount = await Dataservice.Table<Transcription>().Where(s => s.SessionId == sessionId).CountAsync();
            return transcriptionsCount == 0 ? false : true;
        }

        public async Task DeleteSessionAsync(Session session)
        {
            await Dataservice.DeleteAsync(session);
        }
    }
}
