# Agent skill ownership

[AGENTS.md](../../AGENTS.md) points to dkstack on the `v1` ref. Start with the dkstack triage skill.

GitHub Issues, the Kexiva rules, and `eng/verify` stay authoritative for this repository. A skill supplies a workflow. It does not grant an exemption from an issue, a test, or a check. Check exact installed names and dependencies at runtime.

## Upstream sources
- dkstack: https://github.com/daniel-kindl/dkstack/tree/v1
- Matt Pocock: https://github.com/mattpocock/skills (engineering skills under skills/engineering/)
- Poteto pstack: https://github.com/backnotprop/pstack (skills under skills/)
- ASD-STE100: https://www.asd-ste100.org/ (controlled English reference; verify license before distributing its rules or wordlists)

## Ownership
| Task | Primary | Complementary |
| --- | --- | --- |
| Task selection | dkstack: triage | None |
| Requirements/spec | dkstack: clarify-intent | Matt: grill-with-docs, to-spec |
| Issue decomposition | GitHub Issues | Matt: to-tickets |
| Design | dkstack: apply-architecture | pstack: architect for a hard tradeoff |
| Coding and TDD | dkstack: tdd-loop | Do not also run Matt tdd or pstack tdd |
| Bug diagnosis | dkstack: coding/debugging.md | Matt: diagnosing-bugs |
| PR review | dkstack: code-review | pstack: interrogate for one extra question, not a second review |
| Durable decision | dkstack: write-adr | Repository ADR template |
| Text | dkstack: writing-check | [technical-writing.md](technical-writing.md) |
| Benchmarks | Repository benchmarks | pstack: benchmark-checklist as an optional checklist |
| Milestone knowledge transfer | Repository milestone docs | pstack: how, why, teach |

Select one primary workflow per task. Use a complementary skill only for a specific gap. Do not assume any skill exists on the execution host. Never silently install, vendor, modify, or execute third-party skills. If the dkstack skill is absent, use the workflow in [CONTRIBUTING.md](../../CONTRIBUTING.md). Confirm upstream names at invocation time. Skill names and contents may change.

## Milestone knowledge transfer
At milestone completion, write a short explanation of the **whole library** as it currently exists: architecture, API contracts, compatibility, testing, benchmark evidence, decisions, limits, and what changed in the milestone. Use pstack how/why/teach if available. Link the document from the milestone closure summary.

## Conflict precedence
1. Security and maintainer requirements
2. Kexiva issue scope, decisions, coding standards, and verification
3. dkstack guidelines and skills
4. A complementary skill from the table above

A complementary skill never overrides a test, an acceptance criterion, an explicit C# convention, or the requirement for a pull request. The specification review and the standards review stay required even when a review skill is absent.
