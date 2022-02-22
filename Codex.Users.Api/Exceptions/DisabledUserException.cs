using System;
using System.Diagnostics.CodeAnalysis;
using Codex.Models.Exceptions;

namespace Codex.Users.Api.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class DisabledUserException : FunctionalException
{
    public DisabledUserException(string message, string? code = null) : base(message, code)
    {
    }
}