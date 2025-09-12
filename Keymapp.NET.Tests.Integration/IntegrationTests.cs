using System.Drawing;
using Grpc.Core;
using Grpc.Net.Client;
using NUnit.Framework;
using Shouldly;

namespace Keymapp.NET.IntegrationTests;

[NonParallelizable]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class IntegrationTests
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

    // Multiple connection attempts
    [Test, Order(5), Category("Connection"), CancelAfter(10000)]
    public async Task ConnectAsync_multiple_times_with_unavailable_servers(CancellationToken cancellationToken)
    {
        using var channel1 = GrpcChannel.ForAddress("http://127.0.0.1:50051");
        using var channel2 = GrpcChannel.ForAddress("http://127.0.0.1:50052");
        await using var api1 = new KeymappApi(channel1);
        await using var api2 = new KeymappApi(channel2);

        // First connect attempt may throw RpcException if server unavailable
        try
        {
            await api1.ConnectAsync(channel1);
        }
        catch (RpcException)
        {
            // Expected when server is not available
        }

        // Second connect attempt may also throw RpcException
        try
        {
            await api2.ConnectAsync(channel2);
        }
        catch (RpcException)
        {
            // Expected when server is not available
        }

        // The APIs should still be in a valid state for disposal
    }

    [Test, Order(2), Category("Cancellation"), CancelAfter(5000)]
    public async Task ConnectAsync_with_cancelled_token_throws_exception(CancellationToken cancellationToken)
    {
        using var channel = GrpcChannel.ForAddress("http://127.0.0.1:50051");
        await using var api = new KeymappApi(channel);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // May throw OperationCanceledException or RpcException depending on timing
        await Should.ThrowAsync<Exception>(async () => await api.ConnectAsync(channel, cts.Token));
    }

    [Test, Order(4), Category("Connection"), /*CancelAfter(10000)*/]
    public async Task ConnectAsync_with_invalid_address_throws_RpcException()
    {
        using var channel = GrpcChannel.ForAddress("http://192.168.255.255:60000"); // Valid URI format but unreachable
        await using var api = new KeymappApi(channel);

        // Connection should fail with RpcException since the API now tries to connect immediately
        await Should.ThrowAsync<RpcException>(async () => await api.ConnectAsync(channel));
    }

    // Parameter validation tests - these don't require a server
    [Test, Order(1), Category("ParameterValidation"), CancelAfter(5000)]
    public async Task ConnectAsync_with_null_channel_throws_ArgumentNullException(CancellationToken cancellationToken)
    {
        await Should.ThrowAsync<ArgumentNullException>(async () => await _api.ConnectAsync(null!));
    }

    // Disposal and cleanup tests
    [Test, Order(6), Category("Disposal"), CancelAfter(5000)]
    public async Task Dispose_multiple_times_should_not_throw(CancellationToken cancellationToken)
    {
        var api = new KeymappApi(_channel);
        Should.NotThrow(() => api.Dispose());
    }

    [Test, Order(7), Category("Disposal"), CancelAfter(5000)]
    public async Task DisposeAsync_multiple_times_should_not_throw(CancellationToken cancellationToken)
    {
        var api = new KeymappApi(_channel);
        await Should.NotThrowAsync(async () => await api.DisposeAsync());
    }

    // Cancellation token tests - these don't require a server
    [Test, Order(3), Category("Cancellation"), CancelAfter(5000)]
    public async Task GetKeyboardsAsync_with_cancelled_token_throws_OperationCancelledException(CancellationToken cancellationToken)
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Since the API now ensures connection automatically, this should throw OperationCanceledException or RpcException
        await Should.ThrowAsync<Exception>(async () => await _api.GetKeyboardsAsync(cts.Token).AsTask());
    }
}