using System.Diagnostics.CodeAnalysis;

namespace Codex.Tests.Framework.Models
{
    [ExcludeFromCodeCoverage]
    public record DataSetContent(string? CollectionName, string? Data);
}
