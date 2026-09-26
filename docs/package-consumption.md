# Consuming Flamoris.Logging

FLAMORIS applications should consume the shared logging library as a NuGet package. Do not vendor or copy the built DLL into application repositories.

## Package

- Package ID: `Flamoris.Logging`
- Current stable version: `1.0.0`
- Feed: `https://api.nuget.org/v3/index.json`

## Local development

`Flamoris.Logging` is published publicly on nuget.org. Ordinary restore does not require GitHub authentication, a PAT, or a FLAMORIS-specific package source.

Reference the package normally:

~~~xml
<ItemGroup>
  <PackageReference Include="Flamoris.Logging" Version="1.0.0" />
</ItemGroup>
~~~

With the standard nuget.org source enabled, restore is simply:

~~~powershell
dotnet restore
~~~

## GitHub Actions consumers

Consumer workflows need no package-read permission or NuGet credential for `Flamoris.Logging`.

~~~yaml
permissions:
  contents: read

steps:
  - uses: actions/setup-dotnet@v4
    with:
      dotnet-version: 10.0.x

  - run: dotnet restore
~~~

## Publishing a new version

Publishing is normally tag-driven.

1. Update `<Version>` in `src/Flamoris.Logging/Flamoris.Logging.csproj`.
2. Merge the reviewed change to `main`.
3. Create the matching tag, for example `v1.0.1`.
4. The publish workflow builds, tests, packs, verifies that the tag matches the project version, obtains a short-lived nuget.org API key through Trusted Publishing (GitHub Actions OIDC), and pushes to nuget.org.

The workflow also supports a guarded manual dispatch from the exact current `main` commit for feed migration or release recovery. Normal releases should use matching version tags.

Package versions are immutable. The workflow intentionally does not use `--skip-duplicate`; attempting to republish an existing version should fail.

Do not create release tags from unreviewed feature branches.
