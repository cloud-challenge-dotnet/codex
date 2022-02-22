using System.Diagnostics.CodeAnalysis;

namespace Codex.Models.Roles;

[ExcludeFromCodeCoverage]
public record Role(string Code, string? UpperRoleCode = null);