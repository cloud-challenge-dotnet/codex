using Codex.Models.Users;
using Codex.Web.Services.Tools.Interfaces;
using System.Threading.Tasks;

namespace Codex.Web.Services.Tools.Implementations
{
    public class ApplicationData: IApplicationData
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly string _authKey = "auth";
        private readonly string _tenantIdKey = "tenantId";

        public ApplicationData(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        private Auth? _auth;

        public Auth? Auth { get => _auth; }

        public Task SetAuthAsync(Auth? value)
        {
            _auth = value;
            return _localStorageService.SetItem(_authKey, value);
        }

        private string? _tenantId;
        public string? TenantId
        {
            get => _tenantId;
        }

        public Task SetTenantIdAsync(string? value) {
            _tenantId = value;
            return _localStorageService.SetItem(_tenantIdKey, value);
        }

        public async Task InitializeAsync()
        {
            _auth = await _localStorageService.GetItem<Auth>(_authKey);
            _tenantId = await _localStorageService.GetItem<string>(_tenantIdKey);
        }
    }
}
