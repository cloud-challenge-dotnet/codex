using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class CustomProblemDetails : ProblemDetails
    {
        public CustomProblemDetails() : base()
        {
        }

        public string? Code { get; set; }
    }
}
