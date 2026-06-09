# WayaPay artifact feed

Pre-built NuGet packages for the WayaPay .NET client. Use this when you want to consume
the library without publishing to nuget.org.

## Rebuild the package

From the repo root:

```bash
dotnet pack src/Wayapay/Wayapay.csproj -c Release --output artifact
# -> artifact/WayaPay.<version>.nupkg   (version comes from <Version> in Wayapay.csproj)
```

## Consume it

**Inside this repo** — the root `nuget.config` already registers `artifact/` as a source,
so just reference the version you want:

```bash
dotnet add package WayaPay --version 2.0.0
```

**From another machine / project** — point at this folder explicitly:

```bash
dotnet add package WayaPay --version 2.0.0 --source /path/to/artifact
```

…or copy the `.nupkg` somewhere and add that folder as a source once:

```bash
dotnet nuget add source /path/to/artifact -n wayapay
dotnet add package WayaPay --version 2.0.0
```

The package targets `net8.0` and has no dependencies outside the framework.
