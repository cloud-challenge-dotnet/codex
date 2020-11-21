using System;

namespace Codex.Core.Exceptions
{
    [Serializable]
    public class FunctionnalException : Exception
    {
        public FunctionnalException(string message, string? code) : base(message)
        {
            Code = code;
        }

        public string? Code { get; init; }
    }
}
