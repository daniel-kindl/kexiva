# 0001. Record architecture decisions

- Status: Accepted
- Date: 2026-10-14

## Context

Kexiva is built by a single maintainer and by agents. Decisions must be visible and stable between sessions.

## Decision

Record significant decisions as numbered Markdown files in `docs/decisions`, using the template in `0000-template.md`.

## Consequences

- Agents read the ADRs before they change architecture, tooling, or public API policy.
- A changed decision gets a new ADR that supersedes the old one.
