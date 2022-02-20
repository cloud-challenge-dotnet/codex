using System;
using System.Diagnostics.CodeAnalysis;
using Codex.Models.Exceptions;

namespace Codex.Users.Api.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class InvalidUserValidationCodeException : FunctionalException
{
    public InvalidUserValidationCodeException(string message, string? code = null) : base(message, code)
    {
    }
}