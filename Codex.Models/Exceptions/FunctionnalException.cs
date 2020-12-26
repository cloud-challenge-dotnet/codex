using System;

namespace Codex.Models.Exceptions
{
    [Serializable]
    public class FunctionnalException : Exception
    {
        public FunctionnalException(string message, string? code = null) : base(message)
        {
            Code = code;
        }

        public string? Code { get; init; }
    }
}
