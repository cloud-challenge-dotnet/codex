using Codex.Core.Exceptions;
using System;

namespace Codex.Users.Api.Exceptions
{
    [Serializable]
    public class DisabledUserException : FunctionnalException
    {
        public DisabledUserException(string message, string? code = null) : base(message, code)
        {
        }
    }
}
