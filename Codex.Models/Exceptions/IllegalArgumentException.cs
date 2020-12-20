using System;

namespace Codex.Models.Exceptions
{
    [Serializable]
    public class IllegalArgumentException : FunctionnalException
    {
        public IllegalArgumentException(string message, string? code = null) : base(message, code)
        {
        }
    }
}
