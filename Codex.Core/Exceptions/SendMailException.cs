using System;

namespace Codex.Core.Exceptions
{
    [Serializable]
    public class SendMailException : TechnicalException
    {
        public SendMailException(string message, string? code) : base(message, code)
        {
        }
    }
}
