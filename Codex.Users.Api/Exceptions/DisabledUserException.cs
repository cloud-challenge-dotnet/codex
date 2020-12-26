using Codex.Models.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Users.Api.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class DisabledUserException : FunctionnalException
    {
        public DisabledUserException(string message, string? code = null) : base(message, code)
        {
        }
    }
}
