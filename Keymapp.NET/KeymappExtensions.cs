using System.Drawing;
using Api;
using Grpc.Core;

namespace Keymapp.NET;

/// <summary>
/// Extension methods for the Keymapp.NET API providing common operations and utilities.
/// </summary>
/// <remarks>
/// This class provides convenient extension methods for keyboard restoration operations
/// and advanced brightness control functionality.
/// </remarks>
public static class KeymappExtensions
{
    /// <summary>
    /// Restores all RGB LEDs on the keyboard to their default state (off/black).
    /// </summary>
    /// <param name="api">The KeymappApi instance to operate on.</param>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous restoration operation, containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="api"/> is null.</exception>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// This method sets all RGB LEDs to black (0,0,0) with permanent duration (0ms sustain).
    /// It's useful for cleaning up after RGB LED operations or returning the keyboard to a neutral state.
    /// </remarks>
    public static async ValueTask<SetRGBAllReply> RestoreKeyboardColorsAsync(this IKeymappApi api, CancellationToken ct = default)
    {
        var blackColor = Color.FromArgb(0, 0, 0); // Black color (LEDs off)
        return await api.SetRGBAllAsync(blackColor, 0, ct); // Black, permanent
    }

    /// <summary>
    /// Restores the status LED on the keyboard to its default state (off).
    /// </summary>
    /// <param name="api">The KeymappApi instance to operate on.</param>
    /// <param name="led">The status LED index to restore (default is 0).</param>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous restoration operation, containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="api"/> is null.</exception>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// This method turns off the specified status LED with permanent duration (0ms sustain).
    /// It's useful for cleaning up after status LED operations or returning the keyboard to a neutral state.
    /// </remarks>
    public static async ValueTask<SetStatusLedReply> RestoreStatusLedAsync(this IKeymappApi api, int led = 0, CancellationToken ct = default)
    {
        return await api.SetStatusLedAsync(led, false, 0, ct); // LED off, permanent
    }

    /// <summary>
    /// Updates the brightness of the connected keyboard by taking multiple brightness adjustment steps.
    /// </summary>
    /// <param name="api">The KeymappApi instance to operate on.</param>
    /// <param name="increase">True to increase brightness, false to decrease brightness.</param>
    /// <param name="steps">The number of brightness steps to take (must be between 1 and 255).</param>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous brightness update operation, containing the final step result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="api"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="steps"/> is not between 1 and 255.</exception>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// This method performs multiple brightness adjustments in sequence, calling either IncreaseBrightnessAsync 
    /// or DecreaseBrightnessAsync for the specified number of steps. If any individual step fails, the operation
    /// stops and returns the failure result. The method is translated from the Rust version that validates
    /// step count and handles brightness limits gracefully.
    /// </remarks>
    public static async ValueTask<BrightnessUpdateReply> UpdateBrightnessAsync(this IKeymappApi api, bool increase, int steps, CancellationToken ct = default)
    {
        if (steps is < 1 or > 255)
        {
            Guard.ThrowBrightnessOutOfRangeException(nameof(steps), steps);
        }

        BrightnessUpdateReply result = new() { Success = false };

        if (increase)
        {
            for (int i = 0; i < steps; i++)
            {
                ct.ThrowIfCancellationRequested();
                
                result = await api.IncreaseBrightnessAsync(ct);
                if (!result.Success)
                {
                    break; // Stop if brightness adjustment fails (e.g., maximum brightness reached)
                }
            }
        }
        else
        {
            for (int i = 0; i < steps; i++)
            {
                ct.ThrowIfCancellationRequested();
                
                result = await api.DecreaseBrightnessAsync(ct);
                if (!result.Success)
                {
                    break; // Stop if brightness adjustment fails (e.g., minimum brightness reached)
                }
            }
        }

        return result;
    }
}