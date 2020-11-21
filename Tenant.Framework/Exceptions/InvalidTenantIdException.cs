using Codex.Core.Exceptions;
using System;

namespace Codex.Tenants.Framework.Exceptions
{
    [Serializable]
    public class InvalidTenantIdException : FunctionnalException
    {
        public InvalidTenantIdException(string message, string? code) : base(message, code)
        {
        }
    }
}
