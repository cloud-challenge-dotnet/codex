using System;

namespace Codex.Models.Exceptions;

[Serializable]
public class FunctionalException : Exception
{
    public FunctionalException(string message, string? code = null) : base(message)
    {
        Code = code;
    }

    public string? Code { get; init; }
}