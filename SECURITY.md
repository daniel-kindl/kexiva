# Security policy

## Supported versions

The project is in pre-release (M1). Only the latest commit on `main` receives security fixes.

## Report a vulnerability

Do not open a public issue for a vulnerability.

Use GitHub private vulnerability reporting: **Security** tab > **Report a vulnerability** on https://github.com/daniel-kindl/kexiva. If that is not available, contact the maintainer through the GitHub profile https://github.com/daniel-kindl.

Include:

- the affected version or commit,
- steps to reproduce, with the input,
- the impact that you expect.

You will get a first answer in 7 days, if possible.

## Scope notes

Kexiva parses untrusted input. A crash, hang, excessive allocation, or out-of-bounds access on any input is a security issue.

## Automation

- Dependabot updates NuGet packages, GitHub Actions, and the .NET SDK.
- NuGet audit runs on every restore (direct and transitive packages). Audit warnings are errors, so `eng/verify.*` fails on known-vulnerable packages.
- Pull requests run GitHub dependency review.
- CodeQL scans C# and workflows.
- All workflow actions are pinned to commit SHAs. Workflows use `contents: read` by default.
