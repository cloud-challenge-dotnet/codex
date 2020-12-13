using System.Collections.Generic;

namespace Codex.Core.Models.Mail
{
    public record Message(Recipient From, List<Recipient> To, string Subject, string? TextPart = null, string? HtmlPart = null);
}
