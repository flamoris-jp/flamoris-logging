# Security Policy

Do not post vulnerabilities, credentials, tokens, personal data, or exploit details in public Issues. Use GitHub private vulnerability reporting when available, or contact the maintainers privately.

FLAMORIS Logging redacts common sensitive structured-property names, but cannot infer secrets embedded in messages, exception text, file paths, or arbitrary values. Callers must not place secrets or unnecessary personal information in log messages or exceptions. Add application-specific sensitive property names through configuration.

Security fixes generally target the current maintained codebase. No response-time SLA is guaranteed.

## 日本語

脆弱性、認証情報、token、個人情報、攻撃手順を公開Issueへ投稿しないでください。利用可能ならGitHubのPrivate vulnerability reportingを使用してください。

一般的な機密property名は既定でredactionされますが、message、例外文、file path、任意の値に埋め込まれた秘密情報は自動判定できません。呼び出し側は秘密情報や不要な個人情報をログへ渡さないでください。
