using Codex.Models.Users;
using Codex.Web.Services.Users.Interfaces;
using System.Threading.Tasks;
using Codex.Web.Services.Tools.Interfaces;

namespace Codex.BackOffice.Services.Users.Implementations
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpManager _httpManager;
        private readonly IApplicationData _applicationData;

        public AuthenticationService(
            IHttpManager httpManager,
            IApplicationData applicationData
        )
        {
            _httpManager = httpManager;
            _applicationData = applicationData;
        }

        public async Task<Auth> AuthenticateAsync(UserLogin userLogin)
        {
            try
            {
                // clear Auth
                _applicationData.Auth = null;
                _applicationData.TenantId = userLogin.TenantId;

                var auth = await _httpManager.PostAsync<Auth>("userApi/Authentication", userLogin);

                _applicationData.Auth = auth;
                _applicationData.TenantId = userLogin.TenantId;
                return auth;
            }
            catch
            {
                _applicationData.Auth = null;
                _applicationData.TenantId = null;
                throw;
            }
        }
    }
}
