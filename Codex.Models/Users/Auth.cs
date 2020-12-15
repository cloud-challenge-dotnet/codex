namespace Codex.Models.Users
{
    public record Auth(string Id, string Login, string Token, string? FirstName = null, string? LastName = null);
}
