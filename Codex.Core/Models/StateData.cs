using System.Diagnostics.CodeAnalysis;

namespace Codex.Core.Models
{
    [ExcludeFromCodeCoverage]
    public record StateData<T>(T Data, long CreationTimestamp, int ExpireTimeInMinutes);
}
