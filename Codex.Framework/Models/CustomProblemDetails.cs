using Microsoft.AspNetCore.Mvc;

namespace Codex.Core.Models
{
    public class CustomProblemDetails : ProblemDetails
    {
        public CustomProblemDetails() : base()
        {
        }

        public string? Code { get; set; }
    }
}
