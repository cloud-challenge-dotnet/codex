using System;
using System.Threading.Tasks;
using Codex.Core.Implementations;
using Codex.Models.Exceptions;
using Codex.Tests.Framework;
using Grpc.Core;
using Xunit;

namespace Codex.Core.Tests.Implementations;

public class CoreExceptionGrpcInterceptorTest : IClassFixture<Fixture>
{
    [Fact]
    public async Task Intercept_Exception()
    {
        Exception exception = new();

        CoreExceptionGrpcInterceptor interceptor = new();

        var rpcException = await Assert.ThrowsAsync<RpcException>(() => interceptor.UnaryServerHandler<string, RpcException>("",
            Fixture.CreateServerCallContext(),(request, context) => throw exception));

        Assert.NotNull(rpcException);
        Assert.Equal(Status.DefaultCancelled, rpcException.Status);
    }
    
    [Fact]
    public async Task Intercept_ArgumentException()
    {
        ArgumentException exception = new("invalid data");

        CoreExceptionGrpcInterceptor interceptor = new();

        var rpcException = await Assert.ThrowsAsync<RpcException>(() => interceptor.UnaryServerHandler<string, RpcException>("",
            Fixture.CreateServerCallContext(),(request, context) => throw exception));

        Assert.NotNull(rpcException);
        Assert.Equal(StatusCode.InvalidArgument, rpcException.Status.StatusCode);
        //Assert.Equal("invalid data", rpcException.Status.Detail);
    }

    [Fact]
    public async Task Intercept_IllegalArgumentException()
    {
        IllegalArgumentException exception = new("invalid data", "INVALID_DATA");

        CoreExceptionGrpcInterceptor interceptor = new();
        
        var rpcException = await Assert.ThrowsAsync<RpcException>(() => interceptor.UnaryServerHandler<string, RpcException>("",
            Fixture.CreateServerCallContext(),(request, context) => throw exception));

        Assert.NotNull(rpcException);
        Assert.Equal(StatusCode.Cancelled, rpcException.Status.StatusCode);
    }

    [Fact]
    public async Task Intercept_TechnicalException()
    {
        TechnicalException exception = new("technical exception", "TECHNICAL_EXCEPTION");

        CoreExceptionGrpcInterceptor interceptor = new();
        
        var rpcException = await Assert.ThrowsAsync<RpcException>(() => interceptor.UnaryServerHandler<string, RpcException>("",
            Fixture.CreateServerCallContext(),(request, context) => throw exception));

        Assert.NotNull(rpcException);
        Assert.Equal(StatusCode.Internal, rpcException.Status.StatusCode);
    }
}