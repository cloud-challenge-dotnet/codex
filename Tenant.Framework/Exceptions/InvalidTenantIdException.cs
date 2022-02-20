using Codex.Models.Exceptions;
using System;

namespace Codex.Tenants.Framework.Exceptions;

[Serializable]
public class InvalidTenantIdException : FunctionalException
{
    public InvalidTenantIdException(string message, string? code) : base(message, code)
    {
    }
}