# Contributing

## Requirements

- .NET 8 SDK (`dotnet --version`)
- An IDE or editor with C# support (Visual Studio, Rider, VS Code + C# Dev Kit)

## Project layout

```
src/
  Wayaquick/                  # The library — this is what gets packed and published
    Services/               # Collection, Identity, Payouts
    Models/                 # Request and response records per service
    WayaquickClient.cs        # Entry point — holds the HttpClient, retry loop, auth headers
    WayaquickOptions.cs       # Configuration passed to the constructor

tests/
  Wayaquick.Tests/
    Client/                 # Constructor, header, and retry behaviour tests
    Collection/             # Collection.InitiateAsync tests
    Identity/               # Identity.VerifyBvnAsync tests
    Payouts/                # Payouts.ListBanks, VerifyAccount, InitiateAsync tests
    Live/                   # Integration tests that hit the real API (skipped in CI)
    Helpers/
      Factory.cs            # Builds pre-configured clients backed by stub handlers
      LiveFactory.cs        # Builds a real client from env vars for live tests
      StubHandler.cs        # Returns a fixed status + body — use for error path tests
      CapturingHandler.cs   # Records the last request — use to assert what was sent

samples/
  ConsoleDemo/              # Runnable end-to-end demo — kept in sync with the API
```

## Build

```bash
dotnet build Wayaquick.sln
```

## Run unit tests

```bash
# All unit tests
dotnet test tests/Wayaquick.Tests/Wayaquick.Tests.csproj

# With per-test output (see DONE: lines)
dotnet test tests/Wayaquick.Tests/Wayaquick.Tests.csproj --logger "console;verbosity=detailed"

# Filter to one service
dotnet test --filter "FullyQualifiedName~Wayaquick.Tests.Payouts"
dotnet test --filter "FullyQualifiedName~Wayaquick.Tests.Identity"
dotnet test --filter "FullyQualifiedName~Wayaquick.Tests.Collection"
dotnet test --filter "FullyQualifiedName~Wayaquick.Tests.Client"

# Filter to one test by name
dotnet test --filter "DisplayName~ReturnsCheckoutUrl_OnSuccess"
```

Unit tests run entirely against stub/fake HTTP handlers. No credentials, no network.

## Run live integration tests

Live tests are tagged `[Trait("Category", "Live")]` and are excluded from the default test run. They call the real WayaQuick API, so you need valid credentials.

```bash
export WAYAQUICK_MERCHANT_ID=MER_BdVFq17797046929104WEpS
export WAYAQUICK_SECRET_KEY=WAYASECK_PROD_0xdff7910a5b97472a950fd4a2a427470a

dotnet test tests/Wayaquick.Tests/Wayaquick.Tests.csproj --filter "Category=Live"

unset WAYAQUICK_MERCHANT_ID
unset WAYAQUICK_SECRET_KEY
```

Live tests are intentionally not run in CI to avoid flakiness from network conditions or credential availability.

## Pack the library

```bash
dotnet pack src/Wayaquick/Wayaquick.csproj -c Release
# Output: src/Wayaquick/bin/Release/WayaQuick.<version>.nupkg
```

## Run the sample

```bash
WAYA_MERCHANT_ID=MER_... WAYA_SECRET_KEY=WAYASECK_TEST_... dotnet run --project samples/ConsoleDemo
```

## Adding a new feature

1. Add request/response record types under `src/Wayaquick/Models/<Service>/`.
2. Add the method to the relevant service in `src/Wayaquick/Services/`.
3. Add unit tests covering the happy path, error path, correct HTTP method/path, and request body shape.
4. Update `samples/ConsoleDemo/Program.cs` if the feature is user-facing.
5. Update `CHANGELOG.md` under the relevant version.

## Versioning

This project follows [Semantic Versioning](https://semver.org). Releases are cut by pushing a `v*.*.*` tag — the publish workflow handles the rest.

## Code style

- Records for all model types (`sealed record`)
- `required` members for mandatory fields — let the compiler enforce them
- `ArgumentNullException.ThrowIfNull` / `ArgumentException` for runtime validation at the boundary
- No comments explaining what the code does — only add one when the *why* is non-obvious
