using SpeechlyTouch.DataService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        public async Task<List<OrgQuestions>> GetOrgQuestionsAsync()
        {
            return await Dataservice.Table<OrgQuestions>().ToListAsync();
        }

        public async Task<OrgQuestions>GetQuestionAsync(int id)
        {
            return await Dataservice.Table<OrgQuestions>().Where(s => s.ID == id).FirstOrDefaultAsync();
        }
    }
}
