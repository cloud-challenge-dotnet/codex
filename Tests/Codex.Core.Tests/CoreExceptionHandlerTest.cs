using Codex.Models.Exceptions;
using Codex.Core.Interfaces;
using Codex.Tests.Framework;
using System;
using System.Net;
using Xunit;


namespace Codex.Core.Tests
{
    public class CoreExceptionHandlerTest : IClassFixture<Fixture>
    {
        public CoreExceptionHandlerTest()
        {
        }

        [Fact]
        public void Intercept_Exception()
        {
            Exception exception = new();

            CoreExceptionHandler handler = new();

            var customProblemDetails = handler.Intercept(exception);

            Assert.Null(customProblemDetails);
        }

        [Fact]
        public void Intercept_ArgumentException()
        {
            ArgumentException exception = new("invalid data");

            CoreExceptionHandler handler = new();

            var customProblemDetails = handler.Intercept(exception);

            Assert.NotNull(customProblemDetails);
            Assert.Equal((int)HttpStatusCode.BadRequest, customProblemDetails!.Status);
            Assert.Equal("invalid data", customProblemDetails!.Title);
        }

        [Fact]
        public void Intercept_IllegalArgumentException()
        {
            IllegalArgumentException exception = new("invalid data", "INVALID_DATA");

            CoreExceptionHandler handler = new();

            var customProblemDetails = handler.Intercept(exception);

            Assert.NotNull(customProblemDetails);
            Assert.Equal((int)HttpStatusCode.BadRequest, customProblemDetails!.Status);
            Assert.Equal("invalid data", customProblemDetails!.Title);
            Assert.Equal("INVALID_DATA", customProblemDetails!.Code);
        }

        [Fact]
        public void Intercept_TechnicalException()
        {
            TechnicalException exception = new("technical exception", "TECHNICAL_EXCEPTION");

            CoreExceptionHandler handler = new();

            var customProblemDetails = handler.Intercept(exception);

            Assert.NotNull(customProblemDetails);
            Assert.Equal((int)HttpStatusCode.InternalServerError, customProblemDetails!.Status);
            Assert.Equal("technical exception", customProblemDetails!.Title);
            Assert.Equal("TECHNICAL_EXCEPTION", customProblemDetails!.Code);
        }
    }
}
