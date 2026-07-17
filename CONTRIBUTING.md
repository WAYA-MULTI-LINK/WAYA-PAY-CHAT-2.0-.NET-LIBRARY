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
    WayaQuickClient.cs        # Entry point — holds the HttpClient, retry loop, auth headers
    WayaQuickOptions.cs       # Configuration passed to the constructor

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
export WAYAQUICK_MERCHANT_ID=MER_xxxxxxxxxxxxxxxx
export WAYAQUICK_SECRET_KEY=WAYASECK_TEST_xxxxxxxxxxxxxxxx

dotnet test tests/Wayaquick.Tests/Wayaquick.Tests.csproj --filter "Category=Live"

unset WAYAQUICK_MERCHANT_ID
unset WAYAQUICK_SECRET_KEY
```

Live tests are intentionally not run in CI to avoid flakiness from network conditions or credential availability.

## Pack the library

```bash
dotnet pack src/Wayaquick/Wayaquick.csproj -c Release
# Output: src/Wayaquick/bin/Release/WayaQuick.Integration.<version>.nupkg
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

## Releasing & publishing to NuGet

The library is published as [`WayaQuick.Integration`](https://www.nuget.org/packages/WayaQuick.Integration) on nuget.org. Publishing is automated: pushing a version tag runs `.github/workflows/publish.yml`, which builds, tests, packs, pushes the `.nupkg` to nuget.org, and creates a GitHub Release with the package attached.

### Cutting a release

1. Bump `<Version>` in `src/Wayaquick/Wayaquick.csproj` and add an entry to `CHANGELOG.md`.
2. Restage the committed artifact bundle (see `artifact/README.md` for the recipe).
3. Commit everything, then tag and push:

   ```bash
   git tag v<x.y.z>
   git push origin main v<x.y.z>
   ```

4. Watch the **Publish to NuGet** workflow under the repo's Actions tab. The package is indexed and installable on nuget.org within ~15 minutes of a green run.

> **Caution — versions on nuget.org are permanent.** A published version can be *unlisted* (hidden from search) but never deleted or re-pushed: once `2.0.1` exists, no one can ever publish a different `2.0.1`. Make sure `<Version>` was bumped and the build is final **before** tagging — the tag and the commit it points at must include the version bump, or CI will pack the old version.

### CI credentials — Trusted Publishing (no API key)

The workflow authenticates with [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing): GitHub Actions presents an OIDC token that nuget.org exchanges for a 1-hour API key at publish time, so there is no long-lived key to store or rotate. Two things must be configured:

1. **A Trusted Publishing policy on nuget.org** (username → **Trusted Publishing** → add policy):

   | Field | Value |
   |-------|-------|
   | Repository Owner | `WAYA-MULTI-LINK` |
   | Repository | `WAYA-PAY-CHAT-2.0-.NET-LIBRARY` |
   | Workflow File | `publish.yml` (file name only, no path) |
   | Environment | leave empty |

2. **One repository secret** (Settings → Secrets and variables → Actions):

   | Secret | Value |
   |--------|-------|
   | `NUGET_USER` | The nuget.org **profile name** the policy belongs to (not an email address) |

> A newly created policy can start as *temporarily active* for 7 days. If no publish happens in that window it goes inactive — reactivate it from the Trusted Publishing page (one click) and publish again. After the first successful publish it becomes permanently active.

### Publishing manually (fallback)

Trusted Publishing only covers CI. For a manual push from your machine, create a short-lived API key on nuget.org (account → API Keys, *Push* scope, glob `WayaQuick.*`):

```bash
dotnet pack src/Wayaquick/Wayaquick.csproj -c Release -o ./artifacts
dotnet nuget push ./artifacts/WayaQuick.Integration.<x.y.z>.nupkg \
  --api-key <your-nuget-api-key> \
  --source https://api.nuget.org/v3/index.json
```

Prefer the tag-driven CI release — manual publishes skip the test gate and make it easy to burn a version number.

## Code style

- Records for all model types (`sealed record`)
- `required` members for mandatory fields — let the compiler enforce them
- `ArgumentNullException.ThrowIfNull` / `ArgumentException` for runtime validation at the boundary
- No comments explaining what the code does — only add one when the *why* is non-obvious
