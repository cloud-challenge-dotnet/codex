using Codex.Models.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Users.Api.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class InvalidUserValidationCodeException : FunctionnalException
    {
        public InvalidUserValidationCodeException(string message, string? code = null) : base(message, code)
        {
        }
    }
}
