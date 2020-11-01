using System.Threading.Tasks;

namespace Codex.Tenants.Framework.Interfaces
{
    public interface ITenantResolutionStrategy
    {
        Task<string> GetTenantIdentifierAsync();
    }
}
