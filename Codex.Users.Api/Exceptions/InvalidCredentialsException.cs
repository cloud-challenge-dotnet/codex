using Codex.Core.Exceptions;
using System;

namespace Codex.Users.Api.Exceptions
{
    [Serializable]
    public class InvalidCredentialsException : FunctionnalException
    {
        public InvalidCredentialsException(string message, string? code = null) : base(message, code)
        {
        }
    }
}
