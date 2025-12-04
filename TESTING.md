# Testing Notes

The automated checks for this repository require the .NET SDK. In the current container environment the `dotnet` CLI is unavailable, so builds and test runs cannot execute here. To verify locally, install the appropriate .NET SDK and run:

```bash
# Build all projects
dotnet build

# Run available test projects
dotnet test
```

For a quick end-to-end validation of the launcher and trainer (including dependency checks for ScriptHookVDotNet), run the helper script:

```pwsh
pwsh ./tools/ValidateAll.ps1 -Configuration Release
```

If you only want to validate the launcher (for example, on a machine without ScriptHookVDotNet), include `-SkipTrainer`.
