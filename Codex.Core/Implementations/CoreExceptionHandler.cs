using Codex.Models.Exceptions;
using Codex.Core.Models;
using MongoDB.Driver;
using System;
using System.Net;

namespace Codex.Core.Interfaces
{
    public class CoreExceptionHandler : IExceptionHandler
    {
        public CustomProblemDetails? Intercept(Exception exception)
        {
            if(exception is ArgumentException || exception is ArgumentNullException ||
                exception is MongoDuplicateKeyException || exception is InvalidOperationException)
            {
                return new()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = exception.Message
                };
            }
            else if (exception is FunctionnalException functionnalException)
            {
                return new()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = exception.Message,
                    Code = functionnalException.Code
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
