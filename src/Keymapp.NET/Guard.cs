using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Keymapp.NET;

internal static class Guard
{
    [DoesNotReturn]
    internal static void ThrowInvalidOperationException()
    {
        throw new InvalidOperationException("Not connected to keyboard");
    }

    [DoesNotReturn]
    internal static void ThrowBrightnessOutOfRangeException([CallerMemberName] string? name = null, int value = 0)
    {
        throw new ArgumentOutOfRangeException(name, value, "Brightness steps must be between 1 and 255");
    }
}