using System.Diagnostics.CodeAnalysis;

namespace Codex.Models.Users;

[ExcludeFromCodeCoverage]
public record Auth(string Id, string Login, string Token, string? FirstName = null, string? LastName = null);