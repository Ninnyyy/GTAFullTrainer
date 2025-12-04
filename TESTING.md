# Testing Notes

The automated tests for this repository require the .NET SDK. In the current container environment the `dotnet` CLI is unavailable, so builds and test runs cannot execute here. To verify locally, install the appropriate .NET SDK and run:

```bash
# Build all projects
 dotnet build

# Run available test projects
 dotnet test
```

If the SDK is already installed on your machine, these commands should complete without errors.
