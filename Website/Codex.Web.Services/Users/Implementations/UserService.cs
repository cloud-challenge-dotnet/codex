using Codex.Models.Users;
using Codex.Web.Services.Tools.Interfaces;
using Codex.Web.Services.Users.Interfaces;
using System.Threading.Tasks;

namespace Codex.Web.Services.Users.Implementations
{
    public class UserService : IUserService
    {
        private readonly IHttpManager _httpManager;

        public UserService(
            IHttpManager httpManager
        )
        {
            _httpManager = httpManager;
        }

        public Task<User> FindOneAsync(string userId)
        {
            return _httpManager.GetAsync<User>($"userApi/User/{userId}");
        }

        public Task<User> CreateAsync(UserCreator userCreator)
        {
            return _httpManager.PostAsync<User>("userApi/User", userCreator);
        }

        public Task<User> UpdateAsync(User user)
        {
            return _httpManager.PutAsync<User>($"userApi/User/{user.Id}", user);
        }

        public Task<User> UpdatePasswordAsync(string userId, string password)
        {
            return _httpManager.PutAsync<User>($"userApi/User/{userId}/changePassword", password);
        }
    }
}
