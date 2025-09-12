# Keymapp.NET Integration Tests

NUnit-based integration test suite for the Keymapp.NET library, focusing on connection handling, parameter validation, and resource management.

## Overview

This test project provides comprehensive integration testing for the Keymapp.NET library. The tests are designed to work both with and without a running Keymapp gRPC server, ensuring robust validation of the API's behavior under various conditions.

## Test Categories

The integration tests are organized into several categories:

### 1. Connection Tests (`Category("Connection")`)
- **Multiple Connection Attempts** - Tests connecting to multiple servers simultaneously
- **Invalid Address Handling** - Validates proper exception handling for unreachable servers
- **Connection State Management** - Ensures proper connection state handling

### 2. Parameter Validation Tests (`Category("ParameterValidation")`)
- **Null Parameter Validation** - Tests argument null exception handling
- **Input Validation** - Validates proper parameter checking

### 3. Cancellation Tests (`Category("Cancellation")`)
- **Cancellation Token Handling** - Tests proper cancellation token support
- **Timeout Scenarios** - Validates timeout behavior for long-running operations

### 4. Disposal Tests (`Category("Disposal")`)
- **Multiple Disposal Calls** - Ensures safe multiple disposal
- **Async Disposal** - Tests IAsyncDisposable implementation
- **Resource Cleanup** - Validates proper resource management

## Test Configuration

### Server Configuration
- **Default Server**: `http://127.0.0.1:50051`
- **Test Server**: `http://127.0.0.1:50052` (for multiple connection tests)
- **Invalid Server**: `http://192.168.255.255:60000` (for error testing)

### Test Execution Order
Tests are executed in a specific order using the `[Order]` attribute:
1. Parameter Validation (Order 1)
2. Cancellation Tests (Order 2-3)
3. Connection Tests (Order 4-5)
4. Disposal Tests (Order 6-7)

## Running the Tests

### Prerequisites
- **Optional**: Keymapp application running on `http://127.0.0.1:50051`
- **Note**: Tests are designed to work without a running server

### From Command Line
```bash
# Navigate to the integration tests directory
cd Keymapp.NET.Tests.Integration

# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific category
dotnet test --filter "Category=Connection"
```

### From IDE
1. Open Test Explorer
2. Run all tests or select specific tests/categories
3. Review test results and output

### Using NUnit Console Runner
```bash
# Install NUnit Console Runner (if not already installed)
dotnet tool install -g nunit.consolerunner

# Run tests with NUnit console
nunit3-console Keymapp.NET.Tests.Integration.dll
```

## Test Features

### Robust Error Handling
Tests validate proper exception handling for various scenarios:
```csharp
[Test]
public async Task ConnectAsync_with_invalid_address_throws_RpcException()
{
    using var channel = GrpcChannel.ForAddress("http://192.168.255.255:60000");
    await using var api = new KeymappApi(channel);
    
    await Should.ThrowAsync<RpcException>(async () => await api.ConnectAsync(channel));
}
```

### Cancellation Token Support
```csharp
[Test]
public async Task ConnectAsync_with_cancelled_token_throws_exception()
{
    using var cts = new CancellationTokenSource();
    await cts.CancelAsync();
    
    await Should.ThrowAsync<Exception>(async () => await api.ConnectAsync(channel, cts.Token));
}
```

### Resource Management Testing
```csharp
[Test]
public async Task Dispose_multiple_times_should_not_throw()
{
    var api = new KeymappApi(_channel);
    Should.NotThrow(() => api.Dispose());
    Should.NotThrow(() => api.Dispose()); // Should not throw on second call
}
```

### Cleanup Operations
The test suite includes comprehensive cleanup:
```csharp
[OneTimeTearDown]
public async Task OneTimeTearDown()
{
    // Restore all RGB LEDs to off state
    var blackColor = Color.FromArgb(0, 0, 0);
    await tempApi.SetRGBAllAsync(blackColor, 0);
    
    // Turn off status LED
    await tempApi.SetStatusLedAsync(0, false, 0);
}
```

## Test Attributes

### Execution Control
- `[NonParallelizable]` - Tests run sequentially to avoid conflicts
- `[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]` - Fresh instance per test
- `[Order(n)]` - Controls test execution order
- `[CancelAfter(ms)]` - Sets timeout for individual tests

### Categorization
- `[Category("Connection")]` - Connection-related tests
- `[Category("ParameterValidation")]` - Input validation tests
- `[Category("Cancellation")]` - Cancellation token tests
- `[Category("Disposal")]` - Resource disposal tests

## Expected Test Results

### With Keymapp Server Running
When a Keymapp server is available at localhost:50051:
- Connection tests should pass
- API calls should succeed
- Resource cleanup should work properly

### Without Keymapp Server
When no server is running:
- Parameter validation tests should pass (server-independent)
- Connection tests should throw appropriate RpcExceptions
- Disposal tests should pass (resource management)
- Cancellation tests should behave correctly

## Test Dependencies

This test project depends on:

- **NUnit** - Testing framework
- **NUnit3TestAdapter** - Test adapter for Visual Studio/dotnet test
- **Shouldly** - Assertion library for more readable tests
- **Microsoft.NET.Test.Sdk** - .NET test SDK
- **Keymapp.NET** - The library being tested (project reference)

## Continuous Integration

These tests are suitable for CI/CD pipelines:
- **No Hardware Required** - Tests work without ZSA keyboards
- **Server Independent** - Most tests work without Keymapp server
- **Fast Execution** - Tests complete quickly with appropriate timeouts
- **Comprehensive Coverage** - Tests cover error scenarios and edge cases

## Test Coverage Areas

### ✅ Covered Scenarios
- Connection establishment and failure
- Parameter validation
- Cancellation token handling
- Resource disposal (sync and async)
- Multiple connection attempts
- Invalid server addresses
- Timeout scenarios

### 🚧 Future Test Areas
- LED control operations (requires hardware)
- Layer management (requires connected keyboard)
- Brightness control (requires hardware)
- Keyboard discovery and connection (requires hardware)

## Troubleshooting Tests

### Common Test Failures

1. **Timeout Errors**
   - Check CancelAfter timeout values
   - Verify network connectivity for connection tests

2. **RpcException in Connection Tests**
   - Expected behavior when Keymapp server is not running
   - Verify test expectations match actual server state

3. **Resource Disposal Issues**
   - Check for proper using statements
   - Verify cleanup in TearDown methods

## Related Projects

- [Keymapp.NET](../Keymapp.NET/) - Main library being tested
- [Sample](../Sample/) - Functional demonstration of the library

## Contributing

When adding new tests:
1. Follow the established naming conventions
2. Use appropriate categories and order attributes
3. Include proper cleanup in teardown methods
4. Ensure tests work both with and without server
5. Add comprehensive error handling validation