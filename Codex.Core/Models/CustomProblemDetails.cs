using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Core.Models;

[ExcludeFromCodeCoverage]
public class CustomProblemDetails : ProblemDetails
{
    public string? Code { get; set; }
}