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

## Status

Initial repository foundation. API and package boundaries are not yet frozen.

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
