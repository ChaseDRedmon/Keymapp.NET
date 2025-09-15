using System.Drawing;
using Api;
using Grpc.Core;
using Grpc.Net.Client;

namespace Keymapp.NET;

/// <summary>
/// Defines the contract for the Keymapp.NET API, providing methods to interact with ZSA keyboards through the Keymapp application.
/// </summary>
/// <remarks>
/// This interface provides asynchronous methods for keyboard connection management, status queries, layer control,
/// LED lighting effects, and brightness adjustments. All methods are designed to work with the Keymapp gRPC service.
/// </remarks>
public interface IKeymappApi : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Establishes a connection to the Keymapp service through the specified gRPC channel.
    /// </summary>
    /// <param name="channel">The gRPC channel to use for connecting to the Keymapp service.</param>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous connection operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    /// <exception cref="RpcException">Thrown when the connection to the Keymapp service fails or the server is unavailable.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <remarks>
    /// This method should be called before using any other API methods. It attempts to connect to any available keyboard
    /// and handles common connection scenarios like already connected keyboards or no keyboards being available.
    /// </remarks>
    ValueTask ConnectAsync(GrpcChannel channel, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the current status of the Keymapp service and any connected keyboard.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation, containing the status reply with service version and keyboard information.</returns>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// The returned status includes the Keymapp application version and information about the currently connected keyboard,
    /// if any. If no keyboard is connected, the connected keyboard information will be null.
    /// </remarks>
    ValueTask<GetStatusReply> GetStatusAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a list of all keyboards detected by the Keymapp service.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation, containing an array of detected keyboards.</returns>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// Each keyboard in the returned array contains an ID, friendly name, and connection status.
    /// The array may be empty if no keyboards are detected by the service.
    /// </remarks>
    ValueTask<Keyboard[]> GetKeyboardsAsync(CancellationToken ct = default);

    /// <summary>
    /// Connects to a specific keyboard identified by its ID.
    /// </summary>
    /// <param name="request">The connection request containing the keyboard ID to connect to.</param>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation, containing the connection result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// The keyboard ID should be obtained from a previous call to <see cref="GetKeyboardsAsync"/>.
    /// If the specified keyboard is already connected, the method returns a successful result without error.
    /// </remarks>
    ValueTask<ConnectKeyboardReply> ConnectKeyboardAsync(ConnectKeyboardRequest request, CancellationToken ct = default);

    /// <summary>
    /// Connects to the first available keyboard detected by the Keymapp service.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation, containing the connection result.</returns>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// This is a convenience method that automatically connects to any available keyboard.
    /// If a keyboard is already connected, the method returns a successful result.
    /// If no keyboards are available, the method returns an unsuccessful result without throwing an exception.
    /// </remarks>
    ValueTask<ConnectKeyboardReply> ConnectAnyKeyboardAsync(CancellationToken ct = default);

    /// <summary>
    /// Disconnects from the currently connected keyboard.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation, containing the disconnection result.</returns>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// If no keyboard is currently connected, this method may still return a successful result.
    /// After disconnection, keyboard-specific operations will not be available until a new connection is established.
    /// </remarks>
    ValueTask<DisconnectKeyboardReply> DisconnectKeyboardAsync(CancellationToken ct = default);

    /// <summary>
    /// Activates the specified layer on the connected keyboard.
    /// </summary>
    /// <param name="layer">The layer number to activate (typically 0-based).</param>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation, containing the layer activation result.</returns>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// Layers represent different key mappings on the keyboard. The exact behavior and valid layer numbers
    /// depend on the specific keyboard model and its configuration. Invalid layer numbers may result in
    /// an unsuccessful operation result rather than an exception.
    /// </remarks>
    ValueTask<SetLayerReply> SetLayerAsync(int layer, CancellationToken ct = default);

    /// <summary>
    /// Deactivates the specified layer on the connected keyboard.
    /// </summary>
    /// <param name="layer">The layer number to deactivate (typically 0-based).</param>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation, containing the layer deactivation result.</returns>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// This method deactivates a previously activated layer, returning the keyboard to its previous layer state.
    /// The exact behavior depends on the keyboard's layer implementation and may vary between models.
    /// </remarks>
    ValueTask<SetLayerReply> UnsetLayerAsync(int layer, CancellationToken ct = default);

    /// <summary>
    /// Sets the color of a specific RGB LED on the connected keyboard.
    /// </summary>
    /// <param name="led">The LED index to modify (0-based, specific to the keyboard model).</param>
    /// <param name="color">The color to set for the LED, including RGB values.</param>
    /// <param name="sustain">The duration in milliseconds to sustain the color (0 for permanent, default is 0).</param>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation, containing the LED color setting result.</returns>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// The LED index and available LEDs depend on the specific keyboard model. Invalid LED indices may result
    /// in an unsuccessful operation result. The sustain parameter allows for temporary color changes that
    /// automatically revert after the specified duration.
    /// </remarks>
    ValueTask<SetRGBLedReply> SetRGBLedAsync(int led, Color color, int sustain = 0, CancellationToken ct = default);

    /// <summary>
    /// Sets the color of all RGB LEDs on the connected keyboard.
    /// </summary>
    /// <param name="color">The color to set for all LEDs, including RGB values.</param>
    /// <param name="sustain">The duration in milliseconds to sustain the color (0 for permanent, default is 0).</param>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation, containing the LED color setting result.</returns>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// This method provides a convenient way to set all keyboard LEDs to the same color simultaneously.
    /// The sustain parameter allows for temporary color changes that automatically revert after the specified duration.
    /// </remarks>
    ValueTask<SetRGBAllReply> SetRGBAllAsync(Color color, int sustain = 0, CancellationToken ct = default);

    /// <summary>
    /// Controls the state of a specific status LED on the connected keyboard.
    /// </summary>
    /// <param name="led">The status LED index to control (0-based, specific to the keyboard model).</param>
    /// <param name="on">True to turn the LED on, false to turn it off.</param>
    /// <param name="sustain">The duration in milliseconds to sustain the state (0 for permanent, default is 0).</param>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation, containing the LED control result.</returns>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// Status LEDs are typically non-RGB indicator lights that show keyboard status information.
    /// The available status LEDs and their indices depend on the specific keyboard model.
    /// The sustain parameter allows for temporary state changes that automatically revert after the specified duration.
    /// </remarks>
    ValueTask<SetStatusLedReply> SetStatusLedAsync(int led, bool on, int sustain = 0, CancellationToken ct = default);

    /// <summary>
    /// Increases the brightness of the RGB LEDs on the connected keyboard.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation, containing the brightness adjustment result.</returns>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// The brightness increase amount depends on the keyboard's implementation and current brightness level.
    /// If the keyboard is already at maximum brightness, the operation may return a successful result without change.
    /// </remarks>
    ValueTask<BrightnessUpdateReply> IncreaseBrightnessAsync(CancellationToken ct = default);

    /// <summary>
    /// Decreases the brightness of the RGB LEDs on the connected keyboard.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation, containing the brightness adjustment result.</returns>
    /// <exception cref="RpcException">Thrown when the service is unavailable or the request fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection to the Keymapp service has been established.</exception>
    /// <remarks>
    /// The brightness decrease amount depends on the keyboard's implementation and current brightness level.
    /// If the keyboard is already at minimum brightness, the operation may return a successful result without change.
    /// </remarks>
    ValueTask<BrightnessUpdateReply> DecreaseBrightnessAsync(CancellationToken ct = default);
}

public sealed class KeymappApi : IKeymappApi
{
    private readonly Lazy<ValueTask>? _lazyConnect;
    private readonly KeyboardService.KeyboardServiceClient? _client;
    private bool _isConnected;

    // Explicit constructor for automatic connection
    public KeymappApi(GrpcChannel channel)
    {
        Guard.ThrowIfNull(channel);
        
        _client ??= new KeyboardService.KeyboardServiceClient(channel);
        _lazyConnect = new Lazy<ValueTask>(() => ConnectAsync(channel));
    }

    public async ValueTask ConnectAsync(GrpcChannel channel, CancellationToken ct = default)
    {
        Guard.ThrowIfNull(channel);

        try
        {
            // Try to connect to any keyboard with a short timeout to detect server unavailability
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

            var response = await _client!.ConnectAnyKeyboardAsync(new ConnectAnyKeyboardRequest(), cancellationToken: combinedCts.Token);
            _isConnected = true;
        }
        catch (RpcException ex) when (ex.Status.Detail?.Contains("keyboard already connected", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Already connected, this is expected and not an error
            _isConnected = true;
            return;
        }
        catch (RpcException ex) when (ex.Status.Detail?.Contains("no keyboard available", StringComparison.OrdinalIgnoreCase) == true)
        {
            // No keyboard available, this is expected in test scenarios
            _isConnected = true; // We're connected to the server, just no keyboard available
            return;
        }
        catch (RpcException)
        {
            // Connection failed (e.g., server not available) - don't set connected
            _isConnected = false;
            throw;
        }
        catch (Exception)
        {
            // Other connection failures (timeouts, HttpRequestException, etc.) - server unavailable
            _isConnected = false;
            throw new RpcException(new Status(StatusCode.Unavailable, "Server unavailable"));
        }
    }

    public async ValueTask<ConnectKeyboardReply> ConnectKeyboardAsync(ConnectKeyboardRequest request,
        CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);
        try
        {
            return await _client!.ConnectKeyboardAsync(request, cancellationToken: ct);
        }
        catch (RpcException ex) when (ex.Status.Detail?.Contains("already connected",
                                          StringComparison.OrdinalIgnoreCase) == true)
        {
            // Keyboard is already connected, this is a success scenario
            return new ConnectKeyboardReply { Success = true };
        }
    }

    public async ValueTask<ConnectKeyboardReply> ConnectAnyKeyboardAsync(CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);
        try
        {
            return await _client!.ConnectAnyKeyboardAsync(new ConnectAnyKeyboardRequest(), cancellationToken: ct);
        }
        catch (RpcException ex) when (ex.Status.Detail?.Contains("keyboard already connected",
                                          StringComparison.OrdinalIgnoreCase) == true)
        {
            // Already connected to a keyboard, this is a success scenario
            return new ConnectKeyboardReply { Success = true };
        }
        catch (RpcException ex) when (ex.Status.Detail?.Contains("no keyboard available",
                                          StringComparison.OrdinalIgnoreCase) == true)
        {
            // No keyboard available, this is a failure scenario
            return new ConnectKeyboardReply { Success = false };
        }
    }

    public async ValueTask<GetStatusReply> GetStatusAsync(CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);

        // Perform a quick connectivity test - try to make the call with a very short timeout
        // If the server is unavailable, this will throw an exception
        using var quickTest = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, quickTest.Token);

        try
        {
            return await _client!.GetStatusAsync(new GetStatusRequest(), cancellationToken: combinedCts.Token);
        }
        catch (OperationCanceledException) when (quickTest.Token.IsCancellationRequested)
        {
            // Timeout occurred - server is likely unavailable
            throw new RpcException(new Status(StatusCode.Unavailable, "Server unavailable - connection timeout"));
        }
        catch (RpcException)
        {
            // Re-throw RpcException as-is
            throw;
        }
        catch (Exception ex)
        {
            // Wrap other connection failures in RpcException
            throw new RpcException(new Status(StatusCode.Unavailable, $"Server unavailable: {ex.GetType().Name} - {ex.Message}"));
        }
    }

    public async ValueTask<Keyboard[]> GetKeyboardsAsync(CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);

        var keyboards = await _client!.GetKeyboardsAsync(new GetKeyboardsRequest(), cancellationToken: ct);
        return keyboards.Keyboards.ToArray();
    }

    public async ValueTask<DisconnectKeyboardReply> DisconnectKeyboardAsync(CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);
        return await _client!.DisconnectKeyboardAsync(new DisconnectKeyboardRequest(), cancellationToken: ct);
    }

    public async ValueTask<SetLayerReply> SetLayerAsync(int layer, CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);
        return await _client!.SetLayerAsync(new SetLayerRequest { Layer = layer }, cancellationToken: ct);
    }

    public async ValueTask<SetLayerReply> UnsetLayerAsync(int layer, CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);
        return await _client!.UnsetLayerAsync(new SetLayerRequest { Layer = layer }, cancellationToken: ct);
    }

    public async ValueTask<SetRGBLedReply> SetRGBLedAsync(int led, Color color, int sustain = 0,
        CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);

        return await _client!.SetRGBLedAsync(
            new SetRGBLedRequest
            {
                Led = led,
                Red = color.R,
                Green = color.G,
                Blue = color.B,
                Sustain = sustain
            }, cancellationToken: ct);
    }

    public async ValueTask<SetRGBAllReply> SetRGBAllAsync(Color color, int sustain = 0, CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);

        return await _client!.SetRGBAllAsync(new SetRGBAllRequest { Red = color.R, Green = color.G, Blue = color.B, Sustain = sustain },
            cancellationToken: ct);
    }

    public async ValueTask<SetStatusLedReply> SetStatusLedAsync(int led, bool on, int sustain = 0,
        CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);

        return await _client!.SetStatusLedAsync(new SetStatusLedRequest { Led = led, On = on, Sustain = sustain }, cancellationToken: ct);
    }

    public async ValueTask<BrightnessUpdateReply> IncreaseBrightnessAsync(CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);
        return await _client!.IncreaseBrightnessAsync(new IncreaseBrightnessRequest(), cancellationToken: ct);
    }

    public async ValueTask<BrightnessUpdateReply> DecreaseBrightnessAsync(CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);
        return await _client!.DecreaseBrightnessAsync(new DecreaseBrightnessRequest(), cancellationToken: ct);
    }

    public void Dispose()
    {
        try
        {
            _client?.DisconnectKeyboard(new DisconnectKeyboardRequest());
        }
        catch (ObjectDisposedException)
        {
            // Channel may already be disposed, ignore
        }
        catch (RpcException)
        {
            // Disconnection may fail if no keyboard is connected or other RPC issues, ignore
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is null) return;
        try
        {
            await _client.DisconnectKeyboardAsync(new DisconnectKeyboardRequest());
        }
        catch (ObjectDisposedException)
        {
            // Channel may already be disposed, ignore
        }
        catch (RpcException)
        {
            // Disconnection may fail if no keyboard is connected or other RPC issues, ignore
        }
    }

    private async ValueTask<bool> EnsureConnectedAsync(CancellationToken ct = default)
    {
        if (_client is not null && _isConnected)
            return true; // Already connected

        if (_lazyConnect is not null)
        {
            await _lazyConnect.Value;
            return true; // Connected through lazy initialization
        }

        Guard.ThrowInvalidOperationException();
        return false; // This line will never be reached due to exception above
    }
}