using Codex.Core.Exceptions;
using Codex.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Codex.Core.Interfaces
{
    public interface IExceptionHandler
    {
        CustomProblemDetails? Intercept(Exception exception);
    }

    public class CoreExceptionHandler : IExceptionHandler
    {
        CustomProblemDetails? IExceptionHandler.Intercept(Exception exception)
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

    public class ExceptionHandler2 : IExceptionHandler
    {
        CustomProblemDetails? IExceptionHandler.Intercept(Exception exception)
        {
            return null;
        }
    }
}
