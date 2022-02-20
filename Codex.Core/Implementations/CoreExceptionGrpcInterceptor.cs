using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Codex.Core.Models;
using Codex.Models.Exceptions;
using MongoDB.Driver;

namespace Codex.Core.Implementations;

public class CoreExceptionGrpcInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception exception)
        {
            StatusCode statusCode;
            CustomProblemDetails? customProblemDetails = null;
            
            if (exception is ArgumentException || exception is ArgumentNullException ||
                exception is MongoDuplicateKeyException || exception is InvalidOperationException)
            {
                statusCode = StatusCode.InvalidArgument;
                customProblemDetails = new()
                {
                    Title = exception.Message
                };
            }
            else if (exception is FunctionalException functionalException)
            {
                statusCode = StatusCode.Cancelled;
                customProblemDetails = new()
                {
                    Title = exception.Message,
                    Code = functionalException.Code
                };
            }
            else if (exception is TechnicalException technicalException)
            {
                statusCode = StatusCode.Internal;
                customProblemDetails = new()
                {
                    Title = exception.Message,
                    Code = technicalException.Code
                };
            }
            else
            {
                throw new RpcException(Status.DefaultCancelled, exception.Message);
            }

            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            options.Converters.Add(new JsonStringEnumConverter());

            var status = new Status(
                statusCode,
                JsonSerializer.Serialize(customProblemDetails)
            );

            throw new RpcException(status);
        }
    }
}