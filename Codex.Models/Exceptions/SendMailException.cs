using System;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Models.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class SendMailException : TechnicalException
{
    public SendMailException(string message, string? code) : base(message, code)
    {
    }
}