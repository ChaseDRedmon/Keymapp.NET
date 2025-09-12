// Comprehensive Sample Application for Keymapp.NET API
// This sample calls all methods in the KeymappApi class to ensure the library
// does not crash unexpectedly or throw exceptions unexpectedly.

using System.Drawing;
using Api;
using Grpc.Net.Client;
using Keymapp.NET;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("=== Keymapp.NET API Comprehensive Test Sample ===");
Console.WriteLine("This sample tests all methods in the KeymappApi class.");
Console.WriteLine();

var provider = new ServiceCollection()
    .AddKeymappServices(GrpcChannel.ForAddress("http://127.0.0.1:50051", new GrpcChannelOptions
    {
        MaxRetryAttempts = 3,
    }))
    .BuildServiceProvider();

using var scope = provider.CreateScope();
var keymappApi = scope.ServiceProvider.GetRequiredService<IKeymappApi>();

try
{
    Console.WriteLine("1. Testing GetStatusAsync()...");
    try
    {
        var status = await keymappApi.GetStatusAsync();
        Console.WriteLine($"   ✓ Status retrieved - Version: {status.KeymappVersion}");
        if (status.ConnectedKeyboard != null)
        {
            Console.WriteLine($"   ✓ Connected keyboard: {status.ConnectedKeyboard.FriendlyName}");
        }
        else
        {
            Console.WriteLine("   ℹ No keyboard currently connected");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ GetStatusAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine("2. Testing GetKeyboardsAsync()...");
    try
    {
        var keyboards = await keymappApi.GetKeyboardsAsync();
        Console.WriteLine($"   ✓ Found {keyboards.Length} keyboard(s)");
        
        foreach (var keyboard in keyboards)
        {
            Console.WriteLine($"   - Name: {keyboard.FriendlyName} - Status: {keyboard.IsConnected} - ID: {keyboard.Id}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ GetKeyboardsAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine("3. Testing ConnectAnyKeyboardAsync()...");
    try
    {
        var connectResult = await keymappApi.ConnectAnyKeyboardAsync();
        Console.WriteLine($"   ✓ ConnectAnyKeyboard result: Success = {connectResult.Success}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ ConnectAnyKeyboardAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine("4. Testing ConnectKeyboardAsync() with specific keyboard...");
    try
    {
        var keyboards = await keymappApi.GetKeyboardsAsync();
        if (keyboards.Length > 0)
        {
            var firstKeyboard = keyboards[0];
            var connectRequest = new ConnectKeyboardRequest { Id = firstKeyboard.Id };
            var connectResult = await keymappApi.ConnectKeyboardAsync(connectRequest);
            Console.WriteLine($"   ✓ ConnectKeyboard result for '{firstKeyboard.FriendlyName}': Success = {connectResult.Success}");
        }
        else
        {
            Console.WriteLine("   ℹ No keyboards available to test ConnectKeyboardAsync");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ ConnectKeyboardAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine("5. Testing layer management methods...");
    
    // Test SetLayerAsync
    try
    {
        var setLayerResult = await keymappApi.SetLayerAsync(1);
        Console.WriteLine($"   ✓ SetLayerAsync(1) result: Success = {setLayerResult.Success}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ SetLayerAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    // Test UnsetLayerAsync
    try
    {
        var unsetLayerResult = await keymappApi.UnsetLayerAsync(1);
        Console.WriteLine($"   ✓ UnsetLayerAsync(1) result: Success = {unsetLayerResult.Success}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ UnsetLayerAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine("6. Testing RGB LED methods...");
    
    // Test SetRGBLedAsync
    try
    {
        var redColor = Color.FromArgb(255, 0, 0); // Red color
        var setLedResult = await keymappApi.SetRGBLedAsync(0, redColor, 1000); // LED 0, red, 1 second sustain
        Console.WriteLine($"   ✓ SetRGBLedAsync(0, Red, 1000ms) result: Success = {setLedResult.Success}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ SetRGBLedAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    // Test SetRGBAllAsync
    try
    {
        var blueColor = Color.FromArgb(0, 0, 255); // Blue color
        var setAllResult = await keymappApi.SetRGBAllAsync(blueColor, 1000); // Blue, 1 second sustain
        Console.WriteLine($"   ✓ SetRGBAllAsync(Blue, 1000ms) result: Success = {setAllResult.Success}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ SetRGBAllAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    // Restore all RGB LEDs using extension method
    try
    {
        var restoreAllResult = await keymappApi.RestoreKeyboardColorsAsync();
        Console.WriteLine($"   ✓ RestoreKeyboardColorsAsync() - Restore result: Success = {restoreAllResult.Success}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ RestoreKeyboardColorsAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine("7. Testing status LED methods...");
    try
    {
        var statusLedResult = await keymappApi.SetStatusLedAsync(0, true, 1000); // LED 0, on, 1 second sustain
        Console.WriteLine($"   ✓ SetStatusLedAsync(0, true, 1000ms) result: Success = {statusLedResult.Success}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ SetStatusLedAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    // Restore the Status LED using extension method
    try
    {
        var restoreStatusLedResult = await keymappApi.RestoreStatusLedAsync();
        Console.WriteLine($"   ✓ RestoreStatusLedAsync() - Restore result: Success = {restoreStatusLedResult.Success}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ RestoreStatusLedAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine("8. Testing brightness control methods...");
    
    // Test IncreaseBrightnessAsync
    try
    {
        var increaseBrightnessResult = await keymappApi.IncreaseBrightnessAsync();
        Console.WriteLine($"   ✓ IncreaseBrightnessAsync result: Success = {increaseBrightnessResult.Success}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ IncreaseBrightnessAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    // Test DecreaseBrightnessAsync
    try
    {
        var decreaseBrightnessResult = await keymappApi.DecreaseBrightnessAsync();
        Console.WriteLine($"   ✓ DecreaseBrightnessAsync result: Success = {decreaseBrightnessResult.Success}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ DecreaseBrightnessAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    // Test UpdateBrightnessAsync extension method with multiple steps
    try
    {
        var updateBrightnessResult = await keymappApi.UpdateBrightnessAsync(true, 3); // Increase by 3 steps
        Console.WriteLine($"   ✓ UpdateBrightnessAsync(increase: true, steps: 3) result: Success = {updateBrightnessResult.Success}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ UpdateBrightnessAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    // Test UpdateBrightnessAsync with decrease to balance
    try
    {
        var updateBrightnessResult = await keymappApi.UpdateBrightnessAsync(false, 3); // Decrease by 3 steps
        Console.WriteLine($"   ✓ UpdateBrightnessAsync(increase: false, steps: 3) result: Success = {updateBrightnessResult.Success}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ UpdateBrightnessAsync failed: {ex.GetType().Name} - {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine("9. Testing DisconnectKeyboardAsync()...");
    try
    {
        var disconnectResult = await keymappApi.DisconnectKeyboardAsync();
        Console.WriteLine($"   ✓ DisconnectKeyboardAsync result: Success = {disconnectResult.Success}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ DisconnectKeyboardAsync failed: {ex.GetType().Name} - {ex.Message}");
    }
}
catch (Exception globalEx)
{
    Console.WriteLine($"Global exception caught: {globalEx.GetType().Name} - {globalEx.Message}");
    Console.WriteLine($"Stack trace: {globalEx.StackTrace}");
}
finally
{
    Console.WriteLine();
    Console.WriteLine("10. Testing disposal methods...");
    
    // Test IAsyncDisposable
    try
    {
        await keymappApi.DisposeAsync();
        Console.WriteLine("   ✓ DisposeAsync completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ DisposeAsync failed: {ex.GetType().Name} - {ex.Message}");
    }
    
    // Test IDisposable (create a new instance for testing synchronous disposal)
    try
    {
        using var testScope = provider.CreateScope();
        var testApi = testScope.ServiceProvider.GetRequiredService<IKeymappApi>();
        Console.WriteLine("   ✓ Dispose completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠ Dispose failed: {ex.GetType().Name} - {ex.Message}");
    }
}

Console.WriteLine();
Console.WriteLine("=== Keymapp.NET API Comprehensive Test Completed ===");
Console.WriteLine("All methods in the KeymappApi class have been tested.");
Console.WriteLine("Check the output above for any failures or unexpected exceptions.");