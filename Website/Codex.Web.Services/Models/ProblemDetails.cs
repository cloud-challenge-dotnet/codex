using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Web.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class ProblemDetails
    {
        public ProblemDetails()
        {
        }

        public string? Code { get; set; }

        public string? Type { get; set; }

        public string? Title { get; set; }

        public int? Status { get; set; }

        public string? Detail { get; set; }

        public IDictionary<string, object>? Extensions { get; }
    }
}
