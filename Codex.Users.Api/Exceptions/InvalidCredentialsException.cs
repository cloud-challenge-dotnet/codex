using Codex.Models.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Users.Api.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class InvalidCredentialsException : FunctionnalException
    {
        public InvalidCredentialsException(string message, string? code = null) : base(message, code)
        {
        }
    }
}
