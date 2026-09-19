# Design and behavior contract

Flamoris.Logging is a synchronous, structured, local logging library. It has no MCP dependency and owns no application state. MCP is only a hierarchical category family. Formatters and sinks are separate so JSON Lines can be added without replacing the logging API.

## Time and paths

Every event timestamp is normalized to UTC. Text renders local time by default with its numeric offset; set UseLocalTime to false for UTC.

Absolute file paths are used as supplied. Relative paths resolve against the explicit basePath passed to FlamorisLogger.Create, or AppContext.BaseDirectory when omitted—never the process current directory. Packaged applications should pass a writable per-user application-data directory.

## Rotation

Rotation occurs before an append only when a non-empty active file plus the next encoded event would exceed maxFileSizeMb. An exact boundary remains active. The active file moves to .1; older archives move upward. maxFiles counts the active file, so at most maxFiles - 1 archives exist.

File sinks write through a capped UTF-8 buffer, so formatting stops as soon as the configured byte limit is reached; a pathological message, property, or exception is never first materialized as a similarly large text or byte array. An event that reaches the bound is replaced with a compact truncated-event entry before rotation and append. This is deterministic and ensures one pathological event cannot bypass the configured local-storage limit.

## Threading and failures

Configuration is immutable after creation. Each built-in sink serializes writes. There is no queue or unbounded buffer. Sink failures are caught independently and never escape to the host. Thread safety is per logger instance; multiple processes or independent instances must not target the same file.

## Sensitive data

Common structured keys such as token, password, authorization, apiKey, and privateKey are redacted. Applications can add names through RedactedPropertyNames. Redaction is key-based and does not inspect prose, exception messages, or object graphs.
