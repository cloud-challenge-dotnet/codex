using System.Diagnostics.CodeAnalysis;

namespace Codex.Core.Models
{
    public enum TopicType { Modify, Remove }

    [ExcludeFromCodeCoverage]
    public record TopicData<T>(TopicType TopicType, T Data, string TenantId);
}
