using Codex.Core.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Users.Api.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class ExpiredUserValidationCodeException : FunctionnalException
    {
        public ExpiredUserValidationCodeException(string message, string? code = null) : base(message, code)
        {
        }
    }
}
