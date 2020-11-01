namespace Codex.Core.Exceptions
{
    public class IllegalArgumentException : InfoException
    {
        public IllegalArgumentException(string message, string? code) : base(message, code)
        {
        }
    }
}
