using System;

namespace Codex.Core.Models
{
    public record StateData<T>(T Data, long CreationTimestamp, int ExpireTimeInMinutes=30);
}
