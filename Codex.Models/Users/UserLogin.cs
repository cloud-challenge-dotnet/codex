using System.Diagnostics.CodeAnalysis;

namespace Codex.Models.Users;

[ExcludeFromCodeCoverage]
public record UserLogin(string Login = "", string Password = "", string TenantId = "");