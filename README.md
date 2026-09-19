# FLAMORIS Logging

Shared structured logging and diagnostics foundation for FLAMORIS applications.

Part of the **FLAMORIS Commons** shared infrastructure family.

## Purpose

FLAMORIS Logging will provide a small, reusable diagnostics layer for FLAMORIS desktop applications and shared infrastructure.

The initial boundary is intentionally narrow:

- structured application logs;
- consistent event/category conventions;
- session and correlation identifiers;
- exception/crash diagnostics;
- bounded local log retention;
- redaction of secrets and sensitive data;
- diagnostic export/bundle primitives where useful.

Application-specific domain events remain owned by each application. This library should provide infrastructure, not redefine product behavior.

## Design principles

- Logging must remain usable when MCP or other higher-level services are unavailable.
- Never log secrets, credentials, tokens, private keys, or unnecessary personal information.
- Prefer structured fields over prose-only diagnostics.
- Logging failures must not crash the host application.
- Resource usage must be bounded.
- File locations, retention, and rotation must be explicit and testable.
- Do not create a second application state authority through logging or telemetry.

## Related repositories

- [FLAMORIS Commons](https://github.com/flamoris-jp/flamoris-commons)
- [FLAMORIS MCP Core](https://github.com/flamoris-jp/flamoris-mcp-core)
- [FLAMORIS 2D](https://github.com/flamoris-jp/flamoris-2D)
- [FLAMORIS Cutwork](https://github.com/flamoris-jp/flamoris-cutwork)
- [FLAMORIS Kachinco](https://github.com/flamoris-jp/flamoris-kachinco)

## Quick start

The library targets .NET 10 and has no application or MCP dependency.

~~~csharp
var options = new LoggingOptions
{
    Level = "debug",
    Categories =
    {
        ["mcp"] = "info",
        ["mcp.transport"] = "debug",
        ["mcp.auth"] = "warn",
    },
};

var logger = FlamorisLogger.Create(options, basePath: appDataDirectory);
logger.Info("mcp.transport", "Connected", new Dictionary<string, object?>
{
    ["client"] = "chatgpt",
    ["session"] = sessionId,
});
~~~

Defaults are debug with console and logs/flamoris.log outputs. Options are ordinary mutable POCOs suitable for .NET configuration binding. Supported levels are exactly error, warn, info, and debug. Invalid values safely fall back to debug. Hierarchical categories use the most-specific configured ancestor.

See the [design and behavior contract](docs/design.md) for timestamp, path, rotation, concurrency, failure-isolation, and redaction details.

## NuGet package

FLAMORIS applications should consume this library through the `Flamoris.Logging` NuGet package rather than copying DLLs between repositories.

Package source:

`https://nuget.pkg.github.com/flamoris-jp/index.json`

Package reference:

~~~xml
<ItemGroup>
  <PackageReference Include="Flamoris.Logging" Version="0.1.0" />
</ItemGroup>
~~~

Publishing is tag-driven. The tag must match the project package version, for example `v0.1.0`. Tagged releases are built, tested, packed, and published to GitHub Packages using the workflow `GITHUB_TOKEN`.

See [package consumption and release flow](docs/package-consumption.md) for local authentication, consumer CI, and versioning details.

## Configuration

~~~json
{
  "logging": {
    "level": "debug",
    "categories": {
      "mcp": "info",
      "mcp.transport": "debug",
      "mcp.auth": "warn"
    },
    "outputs": [
      { "type": "console" },
      {
        "type": "file",
        "path": "logs/flamoris.log",
        "format": "text",
        "rotation": {
          "enabled": true,
          "maxFileSizeMb": 20,
          "maxFiles": 10
        }
      }
    ]
  }
}
~~~

## Status

Foundation implementation. Public contracts may still evolve before the first stable package release.

## License

Code in this repository is licensed under the [Apache License 2.0](LICENSE), unless otherwise noted.

Commercial use does not require permission. If you'd like, we'd be happy to hear what you used FLAMORIS for. This is completely optional.

FLAMORIS software is provided as-is and does not include guaranteed individual support. AI-assisted self-support is encouraged.

If FLAMORIS helps you or you find it interesting, your support helps fund development and keeps the project growing. 🌱  
<sub>Mostly GPU bills.</sub>

---

## 日本語

FLAMORIS Loggingは、FLAMORISアプリ共通の構造化ログ・診断基盤です。

ログ、例外、セッション識別、秘密情報のredaction、保持期間、診断bundleなどの共通インフラを提供し、各アプリ固有の状態や挙動は各アプリ側に残します。

商用利用に許可は不要です。もしよければ「こんなのに使ったよ」と教えてもらえるとうれしいです。もちろん強制ではありません。

困ったときは、README、Issue、テスト、ソースコードをAIに読ませて自己サポートしてください。

もしお役に立てたり、面白いと思っていただけたなら、開発費用をご支援いただけるとうれしいです。  
FLAMORISは元気になって育ちます。🌱  
<sub>主にGPU代とか。</sub>
