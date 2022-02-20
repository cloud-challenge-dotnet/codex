
using System.Diagnostics.CodeAnalysis;

namespace Codex.Models.Users;

[ExcludeFromCodeCoverage]
public record UserCriteria(string? Login = null, string? Email = null);