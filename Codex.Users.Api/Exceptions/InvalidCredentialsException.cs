using System;
using System.Diagnostics.CodeAnalysis;
using Codex.Models.Exceptions;

namespace Codex.Users.Api.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class InvalidCredentialsException : FunctionalException
{
    public InvalidCredentialsException(string message, string? code = null) : base(message, code)
    {
    }
}