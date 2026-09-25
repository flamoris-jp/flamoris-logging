# Consuming Flamoris.Logging

FLAMORIS applications should consume the shared logging library as a NuGet package. Do not vendor or copy the built DLL into application repositories.

## Package

- Package ID: `Flamoris.Logging`
- Current stable version: `1.0.0`
- Feed: `https://nuget.pkg.github.com/flamoris-jp/index.json`

## Local development

GitHub Packages requires authentication for private organization packages.

Add the FLAMORIS feed to your NuGet configuration using a GitHub token that can read packages. Keep credentials outside source control.

Example source configuration:

~~~xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="flamoris" value="https://nuget.pkg.github.com/flamoris-jp/index.json" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
~~~

Authenticate the `flamoris` source through your normal NuGet/GitHub credential mechanism. Do not commit a username, PAT, `GITHUB_TOKEN`, or other credential into a repository.

Then reference the package:

~~~xml
<ItemGroup>
  <PackageReference Include="Flamoris.Logging" Version="1.0.0" />
</ItemGroup>
~~~

## GitHub Actions consumers

A workflow in another FLAMORIS repository can configure the source with `actions/setup-dotnet`.

~~~yaml
permissions:
  contents: read
  packages: read

steps:
  - uses: actions/setup-dotnet@v4
    with:
      dotnet-version: 10.0.x
      source-url: https://nuget.pkg.github.com/flamoris-jp/index.json
    env:
      NUGET_AUTH_TOKEN: ${{ secrets.GITHUB_TOKEN }}

  - run: dotnet restore
    env:
      NUGET_AUTH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
~~~

Package access is controlled by GitHub package/repository permissions. If a consumer repository cannot restore the package, grant that repository read access to the package rather than copying binaries or adding long-lived credentials to source control.

## Publishing a new version

Publishing is tag-driven.

1. Update `<Version>` in `src/Flamoris.Logging/Flamoris.Logging.csproj`.
2. Merge the reviewed change to `main`.
3. Create the matching tag, for example `v1.0.0`.
4. The publish workflow builds, tests, packs, verifies that the tag matches the project version, and pushes to GitHub Packages.

Package versions are immutable. The workflow intentionally does not use `--skip-duplicate`; attempting to republish an existing version should fail.

Do not create release tags from unreviewed feature branches.
