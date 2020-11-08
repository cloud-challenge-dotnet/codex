using Codex.Core.Models;
using System;

namespace Codex.Core.Interfaces
{
    public interface IExceptionHandler
    {
        CustomProblemDetails? Intercept(Exception exception);
    }
}
