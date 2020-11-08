using System.Diagnostics.CodeAnalysis;

namespace Codex.Tests.Framework.Models
{
    [ExcludeFromCodeCoverage]
    public record DataSetContent
    {
        public string? CollectionName { get; set; }

        public string? Data { get; set; }
    }
}
