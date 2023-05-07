using SpeechlyTouch.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface IOrganizationSettingsService
    {
        Task<OrganizationSettings> GetUserOrganizationSettings(int organizationId, string token);
        Task<List<OrganizationTag>> GetUserOrganizationTags(int organizationId, string token);
    }
}