using System.Diagnostics.CodeAnalysis;

namespace Codex.Core.Models.Mail;

[ExcludeFromCodeCoverage]
public record Recipient(string Email, string Name);