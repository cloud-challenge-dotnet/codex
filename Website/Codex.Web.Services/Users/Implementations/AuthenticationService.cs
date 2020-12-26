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
                await ClearAuthenticationAsync();
                await _applicationData.SetTenantIdAsync(userLogin.TenantId);

                var auth = await _httpManager.PostAsync<Auth>("userApi/Authentication", userLogin);

                await _applicationData.SetAuthAsync(auth);
                await _applicationData.SetTenantIdAsync(userLogin.TenantId);
                return auth;
            }
            catch
            {
                await ClearAuthenticationAsync();
                throw;
            }
        }

        public async Task ClearAuthenticationAsync()
        {
            // clear Auth
            await _applicationData.SetAuthAsync(null);
            await _applicationData.SetTenantIdAsync(null);
        }
    }
}
