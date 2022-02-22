using System;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Core.Extensions;

[ExcludeFromCodeCoverage]
public static class ObjectExtensions
{
    // Kotlin: fun <T, R> T.let(block: (T) -> R): R
    public static TR Let<T, TR>(this T self, Func<T, TR> block)
    {
        return block(self);
    }

    // Kotlin: fun <T> T.also(block: (T) -> Unit): T
    public static T Also<T>(this T self, Action<T> block)
    {
        block(self);
        return self;
    }
}