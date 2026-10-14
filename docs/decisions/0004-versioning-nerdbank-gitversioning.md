# 0004. Versioning with Nerdbank.GitVersioning

- Status: Accepted
- Date: 2026-10-14

## Context

Assembly and package versions must come from git history, not manual edits.

## Decision

- Use Nerdbank.GitVersioning (`version.json`, package version in `Directory.Packages.props`, local tool `nbgv` in `.config/dotnet-tools.json` with the same version).
- Public releases come from `main` and `vX.Y.Z` tags. Other branches produce prerelease versions.
- Pre-1.0, a minor version can contain breaking changes.

## Consequences

- Builds need full git history. CI uses `fetch-depth: 0` and the verify scripts fail on a shallow clone.
- See [docs/standards/versioning.md](../standards/versioning.md).
