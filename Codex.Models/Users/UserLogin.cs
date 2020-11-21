namespace Codex.Models.Users
{
    public record UserLogin(string Login = "", string Password = "", string TenantId = "");
}
