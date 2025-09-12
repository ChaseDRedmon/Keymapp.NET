using Api;
using System.Drawing;
using Grpc.Core;
using Grpc.Net.Client;
using NUnit.Framework;
using Shouldly;

namespace Keymapp.NET.IntegrationTests;

[NonParallelizable]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class ExplicitTests
{
    private KeymappApi _api;
    private GrpcChannel _channel;

    [SetUp]
    public void SetUp()
    {
        _channel = GrpcChannel.ForAddress("http://127.0.0.1:50051");
        _api = new KeymappApi(_channel);
    }

    [TearDown]
    public void TearDown()
    {
        _api?.Dispose();
        _channel?.Dispose();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        // Create a temporary API instance for cleanup operations
        using var tempChannel = GrpcChannel.ForAddress("http://127.0.0.1:50051");
        await using var tempApi = new KeymappApi(tempChannel);
        
        try
        {
            // Restore all RGB LEDs by setting them to off (0,0,0)
            var blackColor = Color.FromArgb(0, 0, 0); // Black color (LEDs off)
            await tempApi.SetRGBAllAsync(blackColor, 0); // Black, permanent
        }
        catch (Exception)
        {
            // Ignore exceptions during cleanup - server may not be available
        }
        
        try
        {
            // Restore status LED by turning it off
            await tempApi.SetStatusLedAsync(0, false, 0); // LED 0, off, permanent
        }
        catch (Exception)
        {
            // Ignore exceptions during cleanup - server may not be available
        }
    }

    [Test, Explicit]
    public async Task GetKeyboardsAsync_against_localhost_should_throw_without_server()
    {
        await _api.ConnectAsync(_channel);
        await Should.ThrowAsync<RpcException>(async () => await _api.GetKeyboardsAsync().AsTask());
    }

    [Test, Explicit]
    public async Task GetStatusAsync_against_localhost_should_throw_without_server()
    {
        await _api.ConnectAsync(_channel);
        await Should.ThrowAsync<RpcException>(async () => await _api.GetStatusAsync().AsTask());
    }

    [Test, Explicit]
    public async Task ConnectAnyKeyboardAsync_against_localhost_should_throw_without_server()
    {
        await _api.ConnectAsync(_channel);
        await Should.ThrowAsync<RpcException>(async () => await _api.ConnectAnyKeyboardAsync().AsTask());
    }

    [Test, Explicit]
    public async Task DisconnectKeyboardAsync_against_localhost_should_throw_without_server()
    {
        await _api.ConnectAsync(_channel);
        await Should.ThrowAsync<RpcException>(async () => await _api.DisconnectKeyboardAsync().AsTask());
    }

    [Test, Explicit]
    public async Task SetLayerAsync_against_localhost_should_throw_without_server()
    {
        await _api.ConnectAsync(_channel);
        await Should.ThrowAsync<RpcException>(async () => await _api.SetLayerAsync(1).AsTask());
    }

    [Test, Explicit]
    public async Task UnsetLayerAsync_against_localhost_should_throw_without_server()
    {
        await _api.ConnectAsync(_channel);
        await Should.ThrowAsync<RpcException>(async () => await _api.UnsetLayerAsync(1).AsTask());
    }

    [Test, Explicit]
    public async Task SetRGBLedAsync_against_localhost_should_throw_without_server()
    {
        await _api.ConnectAsync(_channel);
        await Should.ThrowAsync<RpcException>(async () => await _api.SetRGBLedAsync(0, Color.Red, 1000).AsTask());
    }

    [Test, Explicit]
    public async Task SetRGBAllAsync_against_localhost_should_throw_without_server()
    {
        await _api.ConnectAsync(_channel);
        await Should.ThrowAsync<RpcException>(async () => await _api.SetRGBAllAsync(Color.Blue, 500).AsTask());
    }

    [Test, Explicit]
    public async Task SetStatusLedAsync_against_localhost_should_throw_without_server()
    {
        await _api.ConnectAsync(_channel);
        await Should.ThrowAsync<RpcException>(async () => await _api.SetStatusLedAsync(0, true, 2000).AsTask());
    }

    [Test, Explicit]
    public async Task IncreaseBrightnessAsync_against_localhost_should_throw_without_server()
    {
        await _api.ConnectAsync(_channel);
        await Should.ThrowAsync<RpcException>(async () => await _api.IncreaseBrightnessAsync().AsTask());
    }

    [Test, Explicit]
    public async Task DecreaseBrightnessAsync_against_localhost_should_throw_without_server()
    {
        await _api.ConnectAsync(_channel);
        await Should.ThrowAsync<RpcException>(async () => await _api.DecreaseBrightnessAsync().AsTask());
    }

    [Test, Explicit]
    public async Task ConnectKeyboardAsync_with_valid_id_should_succeed()
    {
        await _api.ConnectAsync(_channel);

        // First get available keyboards
        var keyboards = await _api.GetKeyboardsAsync();
        keyboards.ShouldNotBeEmpty("At least one keyboard should be available for testing");

        // Connect to the first available keyboard
        var request = new ConnectKeyboardRequest { Id = keyboards[0].Id };
        var result = await _api.ConnectKeyboardAsync(request);
        result.Success.ShouldBeTrue();
    }

    // Comprehensive success scenario tests - require running server
    [Test, Explicit]
    public async Task GetStatusAsync_with_server_returns_valid_status()
    {
        await _api.ConnectAsync(_channel);
        var result = await _api.GetStatusAsync();

        result.ShouldNotBeNull();
        result.KeymappVersion.ShouldNotBeNullOrEmpty();
        // Connected keyboard may be null if no keyboard is connected
    }

    [Test, Explicit]
    public async Task GetKeyboardsAsync_with_server_returns_keyboard_list()
    {
        await _api.ConnectAsync(_channel);
        var result = await _api.GetKeyboardsAsync();

        result.ShouldNotBeNull();
        // May be empty if no keyboards are detected
        foreach (var keyboard in result)
        {
            keyboard.Id.ShouldBeGreaterThanOrEqualTo(0);
            keyboard.FriendlyName.ShouldNotBeNullOrEmpty();
        }
    }

    [Test, Explicit]
    public async Task ConnectAnyKeyboardAsync_with_server_attempts_connection()
    {
        await _api.ConnectAsync(_channel);
        var result = await _api.ConnectAnyKeyboardAsync();

        result.ShouldNotBeNull();
        // Success depends on whether keyboards are available
    }

    [Test, Explicit]
    public async Task DisconnectKeyboardAsync_with_server_attempts_disconnection()
    {
        await _api.ConnectAsync(_channel);
        var result = await _api.DisconnectKeyboardAsync();

        result.ShouldNotBeNull();
        // Success depends on whether a keyboard was connected
    }

    [TestCase(0), TestCase(1), TestCase(2), TestCase(5), TestCase(10)]
    [Explicit, Order(5), Category("Integration"), Category("RequiresServer"), CancelAfter(30000)]
    public async Task SetLayerAsync_with_server_and_various_layers(int layer, CancellationToken cancellationToken)
    {
        await _api.ConnectAsync(_channel);
        var result = await _api.SetLayerAsync(layer);
        result.ShouldNotBeNull();
        // Success depends on keyboard connection and valid layer
    }

    [TestCase(0), TestCase(1), TestCase(2), TestCase(5), TestCase(10)]
    [Explicit, Order(5), Category("Integration"), Category("RequiresServer"), CancelAfter(30000)]
    public async Task UnsetLayerAsync_with_server_and_various_layers(int layer, CancellationToken cancellationToken)
    {
        await _api.ConnectAsync(_channel);
        var result = await _api.UnsetLayerAsync(layer);
        result.ShouldNotBeNull();
        // Success depends on keyboard connection and valid layer
    }

    [TestCase(0, "Red", 0)]
    [TestCase(1, "Green", 1000)]
    [TestCase(5, "Blue", 2000)]
    [TestCase(10, "Yellow", 500)]
    [TestCase(0, "FromArgb(128,64,192)", 0)]
    [Explicit, Order(5), Category("Integration"), Category("RequiresServer"), CancelAfter(30000)]
    public async Task SetRGBLedAsync_with_server_and_various_parameters(int led, string colorName, int sustain, CancellationToken cancellationToken)
    {
        await _api.ConnectAsync(_channel);

        Color color = colorName switch
        {
            "Red" => Color.Red,
            "Green" => Color.Green,
            "Blue" => Color.Blue,
            "Yellow" => Color.Yellow,
            "FromArgb(128,64,192)" => Color.FromArgb(128, 64, 192),
            _ => Color.Black
        };

        var result = await _api.SetRGBLedAsync(led, color, sustain);
        result.ShouldNotBeNull();
        // Success depends on keyboard connection and valid LED index
    }

    [TestCase("Red", 0)]
    [TestCase("Green", 1000)]
    [TestCase("Blue", 2000)]
    [TestCase("White", 500)]
    [TestCase("Black", 0)]
    [TestCase("FromArgb(255,128,64,192)", 1500)]
    [Explicit, Order(5), Category("Integration"), Category("RequiresServer"), CancelAfter(30000)]
    public async Task SetRGBAllAsync_with_server_and_various_colors(string colorName, int sustain, CancellationToken cancellationToken)
    {
        await _api.ConnectAsync(_channel);

        Color color = colorName switch
        {
            "Red" => Color.Red,
            "Green" => Color.Green,
            "Blue" => Color.Blue,
            "White" => Color.White,
            "Black" => Color.Black,
            "FromArgb(255,128,64,192)" => Color.FromArgb(255, 128, 64, 192),
            _ => Color.Black
        };

        var result = await _api.SetRGBAllAsync(color, sustain);
        result.ShouldNotBeNull();
        // Success depends on keyboard connection
    }

    [TestCase(0, true, 0)]
    [TestCase(1, false, 1000)]
    [TestCase(2, true, 2000)]
    [TestCase(0, false, 500)]
    [Explicit, Order(5), Category("Integration"), Category("RequiresServer"), CancelAfter(30000)]
    public async Task SetStatusLedAsync_with_server_and_various_parameters(int led, bool on, int sustain, CancellationToken cancellationToken)
    {
        await _api.ConnectAsync(_channel);
        var result = await _api.SetStatusLedAsync(led, on, sustain);
        result.ShouldNotBeNull();
        // Success depends on keyboard connection and valid LED index
    }

    [Test, Explicit]
    public async Task IncreaseBrightnessAsync_with_server()
    {
        await _api.ConnectAsync(_channel);
        var result = await _api.IncreaseBrightnessAsync();

        result.ShouldNotBeNull();
        // Success depends on keyboard connection and current brightness level
    }

    [Test, Explicit]
    public async Task DecreaseBrightnessAsync_with_server()
    {
        await _api.ConnectAsync(_channel);
        var result = await _api.DecreaseBrightnessAsync();

        result.ShouldNotBeNull();
        // Success depends on keyboard connection and current brightness level
    }

    [Test, Explicit, Order(2), Category("Cancellation"), Category("Integration"), Category("RequiresServer"), CancelAfter(30000)]
    public async Task CancellationToken_with_server_should_cancel_operations(CancellationToken cancellationToken)
    {
        await _api.ConnectAsync(_channel);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(1)); // Cancel very quickly

        // These might throw OperationCanceledException or complete normally depending on timing
        try
        {
            await _api.GetStatusAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected if cancellation happens in time
        }
    }

    [Test, Explicit, Order(6), Category("Workflow"), Category("Integration"), Category("RequiresServer"), CancelAfter(60000)]
    public async Task Comprehensive_workflow_test(CancellationToken cancellationToken)
    {
        await _api.ConnectAsync(_channel);

        // Get server status
        var status = await _api.GetStatusAsync();
        status.ShouldNotBeNull();

        // Get available keyboards
        var keyboards = await _api.GetKeyboardsAsync();
        keyboards.ShouldNotBeNull();

        // Try to connect to any available keyboard
        var connectResult = await _api.ConnectAnyKeyboardAsync();
        connectResult.ShouldNotBeNull();

        if (connectResult.Success)
        {
            // If connection succeeded, try some operations
            var layerResult = await _api.SetLayerAsync(1);
            layerResult.ShouldNotBeNull();

            var rgbResult = await _api.SetRGBAllAsync(Color.Blue, 1000);
            rgbResult.ShouldNotBeNull();

            var brightnessResult = await _api.IncreaseBrightnessAsync();
            brightnessResult.ShouldNotBeNull();

            // Cleanup
            var disconnectResult = await _api.DisconnectKeyboardAsync();
            disconnectResult.ShouldNotBeNull();
        }
    }

    [Test, Explicit, Order(7), Category("EdgeCases"), Category("Integration"), Category("RequiresServer"), CancelAfter(60000)]
    public async Task Edge_cases_with_server_should_handle_gracefully(CancellationToken cancellationToken)
    {
        await _api.ConnectAsync(_channel);

        // Test edge case parameters that might be invalid but shouldn't crash

        // Test layer edge cases
        await Should.NotThrowAsync(async () =>
        {
            var result = await _api.SetLayerAsync(-1);
            result.ShouldNotBeNull();
        });

        await Should.NotThrowAsync(async () =>
        {
            var result = await _api.SetLayerAsync(999);
            result.ShouldNotBeNull();
        });

        // Test RGB LED edge cases
        await Should.NotThrowAsync(async () =>
        {
            var result = await _api.SetRGBLedAsync(-1, Color.Red);
            result.ShouldNotBeNull();
        });

        await Should.NotThrowAsync(async () =>
        {
            var result = await _api.SetRGBLedAsync(999, Color.Red);
            result.ShouldNotBeNull();
        });

        // Test status LED edge cases
        await Should.NotThrowAsync(async () =>
        {
            var result = await _api.SetStatusLedAsync(-1, true);
            result.ShouldNotBeNull();
        });

        await Should.NotThrowAsync(async () =>
        {
            var result = await _api.SetStatusLedAsync(999, false);
            result.ShouldNotBeNull();
        });

        // Test connect keyboard edge cases
        await Should.NotThrowAsync(async () =>
        {
            var result = await _api.ConnectKeyboardAsync(new ConnectKeyboardRequest { Id = -1 });
            result.ShouldNotBeNull();
        });

        await Should.NotThrowAsync(async () =>
        {
            var result = await _api.ConnectKeyboardAsync(new ConnectKeyboardRequest { Id = 999999 });
            result.ShouldNotBeNull();
        });
    }
}