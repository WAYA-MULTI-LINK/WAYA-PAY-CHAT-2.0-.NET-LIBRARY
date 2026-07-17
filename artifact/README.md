# WayaQuick artifact feed

Pre-built NuGet packages for the WayaQuick .NET client, committed to the repo so they ship via GitHub.
Use this to consume the library without publishing to nuget.org.

Each release lives in its own `version<x.y.z>/` folder. The current release is
[`version2.0.0/`](version2.0.0/):

| File | What it is |
|------|------------|
| `WayaQuick.2.0.0.zip` | **Download this.** Bundles the `.nupkg` below for one-click download. |
| `WayaQuick.2.0.0.nupkg` | The NuGet package — install it from a local folder source. |

> A NuGet `--source` must be a **local folder** or a **NuGet feed URL** — never a raw GitHub/`.nupkg`
> link. To install from GitHub, download the zip and unzip it (or clone the repo), then install from
> the local folder. Full walkthrough in the repo's top-level `README.md` → **Install**.

## Rebuild the package

From the repo root (the version comes from `<Version>` in `Wayaquick.csproj`):

```bash
VERSION=2.0.0
dotnet pack src/Wayaquick/Wayaquick.csproj -c Release --output artifact/version$VERSION
(cd artifact/version$VERSION && zip -q WayaQuick.$VERSION.zip WayaQuick.$VERSION.nupkg)
# -> artifact/version2.0.0/WayaQuick.2.0.0.nupkg  (+ .zip)
```

## Consume it

**Inside this repo** — the root `nuget.config` registers `artifact/` as a source and NuGet searches
its subfolders recursively, so just reference the version you want:

```bash
dotnet add package WayaQuick --version 2.0.0
```

**From another machine / project** — point at this folder explicitly:

```bash
dotnet add package WayaQuick --version 2.0.0 --source /path/to/artifact
```

…or copy the `.nupkg` somewhere and add that folder as a source once:

```bash
dotnet nuget add source /path/to/artifact -n wayaquick
dotnet add package WayaQuick --version 2.0.0
```

The package targets `net8.0` and has no dependencies outside the framework.
