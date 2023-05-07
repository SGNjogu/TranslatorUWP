using SpeechlyTouch.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface IInternationalizationService
    {
        Task<List<InternationalizationLanguage>> GetInternationalizationLanguages();
    }
}
