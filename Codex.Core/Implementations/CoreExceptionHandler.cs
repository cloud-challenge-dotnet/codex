using Codex.Core.Models;
using Codex.Models.Exceptions;
using MongoDB.Driver;
using System;
using System.Net;
using Codex.Core.Interfaces;

namespace Codex.Core.Implementations;

public class CoreExceptionHandler : IExceptionHandler
{
    public CustomProblemDetails? Intercept(Exception exception)
    {
        if (exception is ArgumentException || exception is ArgumentNullException ||
            exception is MongoDuplicateKeyException || exception is InvalidOperationException)
        {
            return new()
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = exception.Message
            };
        }
        else if (exception is FunctionalException functionalException)
        {
            return new()
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = exception.Message,
                Code = functionalException.Code
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