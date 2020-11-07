using System;

namespace Codex.Core.Exceptions
{
    [Serializable]
    public class TechnicalException : Exception
    {
        public TechnicalException(string message, string? code) : base(message)
        {
            Code = code;
        }

        public string? Code { get; init; }
    }
}
