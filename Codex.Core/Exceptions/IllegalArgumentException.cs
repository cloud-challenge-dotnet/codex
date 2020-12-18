using System;

namespace Codex.Core.Exceptions
{
    [Serializable]
    public class IllegalArgumentException : FunctionnalException
    {
        public IllegalArgumentException(string message, string? code = null) : base(message, code)
        {
        }
    }
}
