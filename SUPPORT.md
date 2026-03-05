# Support

## Use The Right Channel

For normal repository work:

- open a bug report for defects
- open a feature request for planned improvements

For contribution expectations:

- see `CONTRIBUTING.md`

For workspace/build rules:

- see `OAN Mortalis V1.0/docs/WORKSPACE_RULES.md`
- see `OAN Mortalis V1.0/docs/BUILD_READINESS.md`

## Before Opening A Support Request

Verify locally:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Configuration Release
powershell -ExecutionPolicy Bypass -File .\test.ps1 -Configuration Release
powershell -ExecutionPolicy Bypass -File .\OAN Mortalis V1.0\tools\verify-private-corpus.ps1
```

## Security Issues

Do not use public support or issue channels for security-sensitive disclosures.

Follow `SECURITY.md`.
