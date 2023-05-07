using SpeechlyTouch.DataService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        /// <summary>
        /// Method to get all Feedback ReasonForRatings
        /// </summary>
        /// <returns>All ReasonForRatings</returns>
        public async Task<List<UserFeedback>> GetFeedbackRatingsAsync()
        {
            return await Dataservice.Table<UserFeedback>().ToListAsync();
        }
    }
}
