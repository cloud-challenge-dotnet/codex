using Codex.Core.Exceptions;
using Codex.Core.Models;
using System;

namespace Codex.Core.Interfaces
{
    public class CoreExceptionHandler : IExceptionHandler
    {
        public CustomProblemDetails? Intercept(Exception exception)
        {
            if(exception is ArgumentException)
            {
                return new()
                {
                    Status = 400,
                    Title = exception.Message
                };
            }
            else if (exception is IllegalArgumentException illegalArgumentException)
            {
                return new()
                {
                    Status = 400,
                    Title = exception.Message,
                    Code = illegalArgumentException.Code
                };
            }
            return null;
        }
    }
}
