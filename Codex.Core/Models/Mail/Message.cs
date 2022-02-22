using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Core.Models.Mail;

[ExcludeFromCodeCoverage]
public record Message(Recipient From, List<Recipient> To, string Subject, string? TextPart = null, string? HtmlPart = null);