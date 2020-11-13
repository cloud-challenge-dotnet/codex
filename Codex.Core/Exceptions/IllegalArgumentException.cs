using System;

namespace Codex.Core.Exceptions
{
    [Serializable]
    public class IllegalArgumentException : InfoException
    {
        public IllegalArgumentException(string message, string? code) : base(message, code)
        {
        }
    }
}
