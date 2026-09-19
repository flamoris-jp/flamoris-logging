# AGENTS.md

This repository is part of the FLAMORIS Commons shared infrastructure family.

Read and follow:

- `flamoris-jp/flamoris-commons/AGENTS.md`
- `flamoris-jp/flamoris-commons/docs/repository-policy.md`

## Repository-specific rules

FLAMORIS Logging provides logging and diagnostics infrastructure only.

- Do not create application-domain authority here.
- Do not make applications depend on MCP or other higher-level services just to log.
- Never log secrets, credentials, tokens, private keys, or unnecessary personal information.
- Redaction behavior must be explicit and tested.
- Logging failures must fail safe and must not crash the host application.
- File I/O, rotation, retention, memory use, queue depth, and diagnostic bundle size must be bounded.
- Prefer structured events and typed/contextual fields over unstructured string concatenation.
- Host applications own event meaning; this repository owns common logging mechanics and contracts.
- Avoid vendor lock-in in the public API unless an explicit architecture decision justifies it.
- Keep platform-specific implementations behind small boundaries.

Before extracting code from 2D, Cutwork, or Kachinco, compare the real implementations and move only genuinely reusable behavior.
