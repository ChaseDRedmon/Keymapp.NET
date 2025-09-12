# Keymapp.NET Sample Application

A comprehensive demonstration application that showcases all features of the Keymapp.NET library.

## Overview

This sample application provides a complete test suite for the Keymapp.NET API, systematically testing every method in the `IKeymappApi` interface. It serves as both a functional test and a practical example of how to use the library in real applications.

## What This Sample Tests

The sample application performs the following operations:

1. **Status Retrieval** - Gets Keymapp version and connected keyboard information
2. **Keyboard Discovery** - Lists all available ZSA keyboards
3. **Connection Management** - Connects to keyboards (any available or specific)
4. **Layer Management** - Sets and unsets keyboard layers
5. **RGB LED Control** - Controls individual and all RGB LEDs with colors and sustain times
6. **Status LED Control** - Controls status LEDs on supported keyboards
7. **Brightness Control** - Increases/decreases brightness and bulk brightness updates
8. **Proper Disposal** - Tests both sync and async disposal methods

## Prerequisites

Before running this sample:

1. **Keymapp Application** - Must be running with gRPC API enabled
   - Default server: `http://127.0.0.1:50051`
   - The sample will attempt to connect to this address
2. **ZSA Keyboard** (Optional) - A connected ZSA keyboard (Moonlander, Planck EZ, Ergodox EZ, etc.)
   - The sample will work without a keyboard but some operations may fail gracefully

## Running the Sample

### From Command Line
```bash
# Navigate to the Sample directory
cd Sample

# Run the application
dotnet run
```

### From IDE
1. Set the Sample project as the startup project
2. Run or debug the application (F5 or Ctrl+F5)

## Expected Output

The sample will output detailed results for each test:

```
=== Keymapp.NET API Comprehensive Test Sample ===
This sample tests all methods in the KeymappApi class.

1. Testing GetStatusAsync()...
   ✓ Status retrieved - Version: 1.2.3
   ✓ Connected keyboard: Moonlander Mark I

2. Testing GetKeyboardsAsync()...
   ✓ Found 1 keyboard(s)
   - Name: Moonlander Mark I - Status: True - ID: 12345

3. Testing ConnectAnyKeyboardAsync()...
   ✓ ConnectAnyKeyboard result: Success = True

[... additional test results ...]
```

## Sample Features Demonstrated

### Dependency Injection Setup
```csharp
var provider = new ServiceCollection()
    .AddKeymappServices(GrpcChannel.ForAddress("http://127.0.0.1:50051", new GrpcChannelOptions
    {
        MaxRetryAttempts = 3,
    }))
    .BuildServiceProvider();
```

### Comprehensive Error Handling
Every API call is wrapped in try-catch blocks to demonstrate proper error handling:
```csharp
try
{
    var status = await keymappApi.GetStatusAsync();
    Console.WriteLine($"   ✓ Status retrieved - Version: {status.KeymappVersion}");
}
catch (Exception ex)
{
    Console.WriteLine($"   ⚠ GetStatusAsync failed: {ex.GetType().Name} - {ex.Message}");
}
```

### LED Control with Colors
```csharp
var redColor = Color.FromArgb(255, 0, 0); // Red color
var setLedResult = await keymappApi.SetRGBLedAsync(0, redColor, 1000); // LED 0, red, 1 second sustain
```

### Extension Method Usage
```csharp
// Using extension methods for convenience
await keymappApi.RestoreKeyboardColorsAsync();
await keymappApi.RestoreStatusLedAsync();
await keymappApi.UpdateBrightnessAsync(true, 3); // Increase brightness by 3 steps
```

### Proper Resource Management
```csharp
// The sample demonstrates both sync and async disposal
await keymappApi.DisposeAsync();

// And proper using statement patterns
using var scope = provider.CreateScope();
```

## Configuration

The sample uses hardcoded configuration but can be easily modified:

- **gRPC Server Address**: `http://127.0.0.1:50051`
- **Retry Attempts**: 3
- **LED Test Colors**: Red and Blue
- **Sustain Times**: 1000ms for most LED operations
- **Layer Test**: Uses layer 1 for set/unset operations
- **Brightness Steps**: 3 steps for bulk brightness changes

## Troubleshooting

### Common Issues

1. **"gRPC Error: Status(StatusCode=Unavailable)"**
   - Solution: Ensure Keymapp application is running
   - Check if gRPC API is enabled in Keymapp settings

2. **Connection Timeouts**
   - Solution: Verify the server address and port
   - Check firewall settings

3. **"No keyboards available"**
   - This is normal if no ZSA keyboard is connected
   - The sample will continue testing other API methods

4. **LED/Layer operations fail**
   - Normal when no keyboard is connected
   - Operations require an active keyboard connection

### Success Indicators

- ✓ indicates successful operation
- ⚠ indicates failed operation (may be expected without hardware)
- ℹ indicates informational message

## Dependencies

This sample depends on:

- **Keymapp.NET** - The main library (project reference)
- **Microsoft.Extensions.DependencyInjection** - For DI container setup
- **System.Drawing.Common** - For Color support (implicit via Keymapp.NET)

## Related Projects

- [Keymapp.NET](../Keymapp.NET/) - Main library with full API documentation
- [Integration Tests](../Keymapp.NET.Tests.Integration/) - Automated test suite

## Learning Resources

This sample demonstrates:
- Async/await patterns with gRPC
- Dependency injection setup
- Comprehensive error handling
- Resource disposal patterns
- Extension method usage
- Color manipulation for LED control
- Graceful degradation when hardware is unavailable