using Codex.Core.Interfaces;
using Codex.Core.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Codex.Core
{
    public static class CustomErrorHandlerHelper
    {
        public static void UseCustomErrors(this IApplicationBuilder app, IHostEnvironment environment, IEnumerable<IExceptionHandler> exceptionHandlers)
        {
            app.Run(async context => await WriteResponse(context,
                includeDetails: environment.IsDevelopment(),
                exceptionHandlers: exceptionHandlers
            ));
        }

        private static async Task WriteResponse(HttpContext httpContext, bool includeDetails, IEnumerable<IExceptionHandler> exceptionHandlers)
        {
            // Try and retrieve the error from the ExceptionHandler middleware
            var exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
            var ex = exceptionDetails?.Error;

            // Should always exist, but best to be safe!
            if (ex != null)
            {
                CustomProblemDetails? problemDetails = null;
                if(exceptionHandlers != null)
                {
                    foreach(var exceptionHandler in exceptionHandlers)
                    {
                        if((problemDetails = exceptionHandler.Intercept(ex)) != null)
                        {
                            break;
                        }
                    }
                }
                // ProblemDetails has it's own content type
                httpContext.Response.ContentType = "application/problem+json";

                if(problemDetails == null)
                {
                    // Get the details to display, depending on whether we want to expose the raw exception
                    var title = includeDetails ? "An error occured: " + ex.Message : "An error occured";
                    problemDetails = new CustomProblemDetails
                    {
                        Status = 500,
                        Title = title
                    };
                }

                if(problemDetails.Detail == null)
                {
                    problemDetails.Detail = includeDetails ? ex.ToString() : null;
                }

                httpContext.Response.StatusCode = problemDetails.Status ?? 500;
                

                // This is often very handy information for tracing the specific request
                var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
                if (traceId != null)
                {
                    problemDetails.Extensions["traceId"] = traceId;
                }

                //Serialize the problem details object to the Response as JSON (using System.Text.Json)
                if(httpContext != null) {
                    var stream = httpContext.Response.Body;

                    JsonSerializerOptions options = new()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true,
                        IgnoreNullValues = true
                    };
                    options.Converters.Add(new JsonStringEnumConverter());

                    await JsonSerializer.SerializeAsync(stream, problemDetails,
                        options: options
                    );
                }
            }
        }
    }
}
