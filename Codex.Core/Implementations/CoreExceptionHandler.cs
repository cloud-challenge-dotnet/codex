using Codex.Core.Exceptions;
using Codex.Core.Models;
using System;
using System.Net;

namespace Codex.Core.Interfaces
{
    public class CoreExceptionHandler : IExceptionHandler
    {
        public CustomProblemDetails? Intercept(Exception exception)
        {
            if(exception is ArgumentException || exception is ArgumentNullException)
            {
                return new()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = exception.Message
                };
            }
            else if (exception is IllegalArgumentException illegalArgumentException)
            {
                return new()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = exception.Message,
                    Code = illegalArgumentException.Code
                };
            }
            else if (exception is TechnicalException technicalException)
            {
                return new()
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = exception.Message,
                    Code = technicalException.Code
                };
            }
            return null;
        }
    }
}
