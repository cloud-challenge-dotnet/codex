using System;

namespace Codex.Core.Exceptions
{
    [Serializable]
    public class IllegalArgumentException : FunctionnalException
    {
        public IllegalArgumentException(string message, string? code) : base(message, code)
        {
        }
    }
}
