using SpeechlyTouch.Core.Domain;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface IUsageTrackingService
    {
        Task<OrganizationUsageLimit> GetOrganizationUsageLimit(int organizationId, string token);
        Task<UserUsageLimit> GetUserUsageLimit(int userId, string token);
    }
}