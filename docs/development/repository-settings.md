# Repository settings

This file records the GitHub settings for `daniel-kindl/kexiva`. See [ADR 0005](../decisions/0005-branching-and-repository-rules.md).

## General

- Default branch: `main`. Integration branch: `dev`.
- Pull requests: **squash merge only**. Default commit message: pull request title and description.
- Automatically delete head branches: on.
- Private vulnerability reporting: on.
- Dependabot alerts and security updates: on.
- Secret scanning and push protection: on.
- Code scanning: CodeQL default setup (C#, GitHub Actions).
- Actions > General > Workflow permissions: **Read repository contents**.

## Branch rulesets

`protect-main` applies to `main` and `protect-dev` applies to `dev`. Both have the same rules:

| Setting | Value |
| --- | --- |
| Require pull request | yes |
| Required approvals | 0 |
| Dismiss stale approvals on push | yes |
| Require conversation resolution | yes |
| Required status checks (strict, up to date) | see list |
| Block force pushes | yes |
| Block deletion | yes |
| Require linear history | yes |
| Allowed merge methods | squash |
| Bypass list | repository admin role (always) |

Required status checks (GitHub Actions, integration id 15368):

- `verify (linux-x64)`
- `verify (windows-x64)`
- `verify (macos-arm64)`
- `verify (macos-x64)`
- `compile-benchmarks`
- `pr-title (Conventional Commits)`
- `dependency-review`

If you rename a CI job, update both rulesets in the same pull request.

## Labels

See [.github/labels.md](../../.github/labels.md).
