using System;

namespace Codex.Models.Exceptions;

[Serializable]
public class TechnicalException : Exception
{
    public TechnicalException(string message, string? code = null) : base(message)
    {
        Code = code;
    }

    public string? Code { get; init; }
}