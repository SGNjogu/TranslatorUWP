using SpeechlyTouch.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface ICustomTagService
    {
        Task<List<CustomTag>> GetOrganizationCustomTags(int organizationId, string token);
    }
}