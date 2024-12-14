using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HCGStudio.PaddingStove.Core;

public static class Ensure
{
    [return: NotNull]
    public static T NotNull<T>([NotNull]T value, [CallerArgumentExpression("value")] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }
}