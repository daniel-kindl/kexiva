# Contributing

Read [AGENTS.md](AGENTS.md) first. It applies to humans and agents.

## Development workflow

1. Select a GitHub issue with the label `status:ready`. Confirm its dependencies and acceptance criteria. Assign yourself. Replace `status:ready` with `status:in-progress`.
2. Create a branch from `dev`.
3. Write tests first, then the change.
4. Run `./eng/verify.sh` or `pwsh ./eng/verify.ps1`.
5. Get an independent specification review and an independent standards review. Address the findings.
6. Open a pull request to `dev`.

After the squash merge, close the linked issue. A merge into `dev` does not close it, because `main` is the default branch.

## Prerequisites

- .NET SDK from `global.json` (currently 10.0.402). Install it with `dotnet-install` or your package manager.
- git with full history (Nerdbank.GitVersioning needs it).
- PowerShell 7 for `eng/verify.ps1`.
- Optional: `dotnet tool restore` installs `nbgv`.

## Branching

| Branch | Purpose |
| --- | --- |
| `main` | Stable. Only release pull requests from `dev`. |
| `dev` | Integration. Issue pull requests target `dev`. |
| `feat/<issue>-<desc>` | New API. |
| `fix/<issue>-<desc>` | Bug fix. |
| `perf/<issue>-<desc>` | Performance work with benchmark evidence. |
| `chore/<issue>-<desc>`, `docs/<issue>-<desc>` | Tooling and documentation. |

Branches are short-lived. GitHub deletes them after merge.

## Commits

Use [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/):

```
<type>(<optional scope>): <summary>
```

Types: `feat`, `fix`, `perf`, `docs`, `test`, `refactor`, `build`, `ci`, `chore`, `revert`. Use `!` or a `BREAKING CHANGE:` footer for breaking changes.

## Pull requests

- Squash merge only. The pull request title becomes the commit message, so CI checks that it is a Conventional Commit.
- Fill in the pull request template. Include verification output.
- Link the issue (`Closes #N`).
- CI must be green on all four platforms.
- Record public API changes in `src/Kexiva/PublicAPI.Unshipped.txt`.

## Tests and benchmarks

- [Testing](docs/testing/README.md)
- [Benchmark methodology](docs/benchmarks/methodology.md). Do not invent benchmark results.

## Coding standards

The build enforces them. See `.editorconfig`, `.globalconfig`, `Directory.Build.props`, and [ADR 0002](docs/decisions/0002-build-standards-and-verification.md).
