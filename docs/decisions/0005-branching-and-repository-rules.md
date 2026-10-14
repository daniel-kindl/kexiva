# 0005. Branching, squash merges, and rulesets

- Status: Accepted
- Date: 2026-10-14

## Context

The project has one maintainer and several agents. History must stay linear and every change must pass CI.

## Decision

- `main` is stable. `dev` is the integration branch. Issue branches (`feat/`, `fix/`, `perf/`, `docs/`, `chore/<issue>-<desc>`) target `dev`.
- Squash merge only. The pull request title (a Conventional Commit) and body become the commit message.
- Rulesets `protect-main` and `protect-dev` require a pull request, resolved conversations, linear history, and the strict CI checks. They block force pushes and deletion. Required approvals are 0 because GitHub does not let an author approve their own pull request; independent agent reviews and green CI are the gate.
- The repository admin role can bypass the rulesets. Agents do not bypass them.
- Dependabot pull requests target `dev`.

## Consequences

- Settings are recorded in [docs/development/repository-settings.md](../development/repository-settings.md).
- Renaming a CI job breaks the required checks. Update the rulesets in the same change.
