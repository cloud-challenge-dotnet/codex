namespace Codex.Core.Models
{
    public enum TopicType { Modify, Remove }

    public record TopicData<T>(TopicType TopicType, T Data);
}
