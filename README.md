# Keymapp.NET

A .NET client library for controlling ZSA keyboards through the Keymapp application's gRPC API.

## Overview

Keymapp.NET provides a comprehensive C# wrapper around the Keymapp gRPC service, allowing you to programmatically control ZSA keyboards (like the Moonlander, Planck EZ, and Ergodox EZ) from your .NET applications.

## Features

- **Keyboard Management**: Connect to and manage multiple ZSA keyboards
- **Layer Control**: Set and unset keyboard layers programmatically
- **RGB LED Control**: Control individual LEDs or all LEDs with custom colors
- **Status LED Control**: Manage status LEDs on supported keyboards
- **Brightness Control**: Adjust keyboard brightness levels
- **Async/Await Support**: Full async support with cancellation tokens
- **Dependency Injection**: Built-in support for Microsoft.Extensions.DependencyInjection
- **Extension Methods**: Convenient extension methods for common operations

## Installation

Add the project reference to your application:

```xml
<ProjectReference Include="path/to/Keymapp.NET/Keymapp.NET.csproj"/>
```

## Prerequisites

- Keymapp application must be running with gRPC API enabled (typically on `http://127.0.0.1:50051`)
- A compatible ZSA keyboard connected to your system

## Quick Start

### Basic Setup with Dependency Injection

```csharp
using Keymapp.NET;
using Microsoft.Extensions.DependencyInjection;
using Grpc.Net.Client;

var services = new ServiceCollection()
    .AddKeymappServices(GrpcChannel.ForAddress("http://127.0.0.1:50051"))
    .BuildServiceProvider();

using var scope = services.CreateScope();
var keymappApi = scope.ServiceProvider.GetRequiredService<IKeymappApi>();
```

### Manual Setup

```csharp
using Grpc.Net.Client;
using Keymapp.NET;

using var channel = GrpcChannel.ForAddress("http://127.0.0.1:50051");
await using var api = new KeymappApi(channel);
```

### Basic Usage Examples

#### Get Keymapp Status
```csharp
var status = await keymappApi.GetStatusAsync();
Console.WriteLine($"Keymapp Version: {status.KeymappVersion}");
if (status.ConnectedKeyboard != null)
{
    Console.WriteLine($"Connected: {status.ConnectedKeyboard.FriendlyName}");
}
```

#### Connect to a Keyboard
```csharp
// Connect to any available keyboard
var connectResult = await keymappApi.ConnectAnyKeyboardAsync();

// Or connect to a specific keyboard
var keyboards = await keymappApi.GetKeyboardsAsync();
if (keyboards.Length > 0)
{
    var request = new ConnectKeyboardRequest { Id = keyboards[0].Id };
    var result = await keymappApi.ConnectKeyboardAsync(request);
}
```

#### Control RGB LEDs
```csharp
// Set a specific LED to red
var redColor = Color.FromArgb(255, 0, 0);
await keymappApi.SetRGBLedAsync(0, redColor, 1000); // LED 0, red, 1 second

// Set all LEDs to blue
var blueColor = Color.FromArgb(0, 0, 255);
await keymappApi.SetRGBAllAsync(blueColor, 2000); // All LEDs, blue, 2 seconds

// Restore original colors
await keymappApi.RestoreKeyboardColorsAsync();
```

#### Layer Management
```csharp
// Activate layer 1
await keymappApi.SetLayerAsync(1);

// Deactivate layer 1
await keymappApi.UnsetLayerAsync(1);
```

#### Brightness Control
```csharp
// Increase brightness
await keymappApi.IncreaseBrightnessAsync();

// Decrease brightness
await keymappApi.DecreaseBrightnessAsync();

// Bulk brightness update (extension method)
await keymappApi.UpdateBrightnessAsync(increase: true, steps: 3);
```

## API Reference

### Core Interface: `IKeymappApi`

#### Connection Methods
- `ConnectAsync(GrpcChannel channel, CancellationToken ct = default)` - Initialize connection
- `ConnectKeyboardAsync(ConnectKeyboardRequest request, CancellationToken ct = default)` - Connect specific keyboard
- `ConnectAnyKeyboardAsync(CancellationToken ct = default)` - Connect any available keyboard
- `DisconnectKeyboardAsync(CancellationToken ct = default)` - Disconnect current keyboard

#### Status Methods
- `GetStatusAsync(CancellationToken ct = default)` - Get Keymapp status and connected keyboard info
- `GetKeyboardsAsync(CancellationToken ct = default)` - List all available keyboards

#### Layer Control
- `SetLayerAsync(int layer, CancellationToken ct = default)` - Activate a keyboard layer
- `UnsetLayerAsync(int layer, CancellationToken ct = default)` - Deactivate a keyboard layer

#### LED Control
- `SetRGBLedAsync(int led, Color color, int sustain = 0, CancellationToken ct = default)` - Control individual RGB LED
- `SetRGBAllAsync(Color color, int sustain = 0, CancellationToken ct = default)` - Control all RGB LEDs
- `SetStatusLedAsync(int led, bool on, int sustain = 0, CancellationToken ct = default)` - Control status LEDs

#### Brightness Control
- `IncreaseBrightnessAsync(CancellationToken ct = default)` - Increase brightness by one step
- `DecreaseBrightnessAsync(CancellationToken ct = default)` - Decrease brightness by one step

### Extension Methods

The library includes helpful extension methods in `KeymappExtensions`:

- `RestoreKeyboardColorsAsync()` - Restore all RGB LEDs to default
- `RestoreStatusLedAsync()` - Restore status LED to default
- `UpdateBrightnessAsync(bool increase, int steps)` - Bulk brightness adjustment

## Error Handling

All API methods properly handle gRPC exceptions and cancellation tokens. Wrap calls in try-catch blocks to handle network issues:

```csharp
try
{
    var status = await keymappApi.GetStatusAsync();
    // Handle success
}
catch (RpcException ex)
{
    // Handle gRPC communication errors
    Console.WriteLine($"gRPC Error: {ex.Message}");
}
catch (OperationCanceledException)
{
    // Handle cancellation
    Console.WriteLine("Operation was cancelled");
}
```

## Dependencies

- Google.Protobuf
- Grpc.Net.Client
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Options.ConfigurationExtensions

## License

This project integrates with ZSA's Keymapp ecosystem. The protobuf definitions are sourced from the official [ZSA Kontroll repository](https://github.com/zsa/kontroll).

## Related Projects

- [Sample](../Sample/) - Comprehensive example application demonstrating all API features
- [Integration Tests](../Keymapp.NET.Tests.Integration/) - Test suite for API functionality