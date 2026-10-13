# Agent skill ownership

GitHub Issues, AGENTS.md, and eng/verify are authoritative. External skills supply workflows, not exemptions. Check exact installed names and dependencies at runtime.

## Upstream sources
- Matt Pocock: https://github.com/mattpocock/skills (engineering skills under skills/engineering/)
- Poteto pstack: https://github.com/backnotprop/pstack (skills under skills/)
- ASD-STE100: https://www.asd-ste100.org/ (controlled English reference; verify license before distributing its rules or wordlists)

## Ownership
| Task | Primary | Complementary |
| --- | --- | --- |
| Requirements/spec | Matt: grill-with-docs, to-spec | pstack: architect for hard design tradeoffs |
| Issue decomposition | Matt: to-tickets | GitHub Issues remain source of truth |
| Coding | Matt: implement / implement-spec | pstack: why only for focused analysis |
| TDD | Matt: tdd | Do not invoke pstack tdd simultaneously |
| Bug diagnosis | Matt: diagnosing-bugs | pstack: correct if justified |
| PR review | Matt: code-review | pstack: interrogate for additional investigation, not a duplicate mandatory review |
| Benchmarks | Repository benchmarks | pstack: benchmark-checklist as an optional checklist |
| Milestone knowledge transfer | Repository milestone docs | pstack: how, why, teach |

Select one primary workflow per task. Use supporting skills only for a specific gap. Do not assume any skill exists on the execution host. Never silently install, vendor, modify, or execute third-party skills. Use repository-native workflow if absent. Confirm upstream names at invocation time; skill names and contents may change.

## Milestone knowledge transfer
At milestone completion, write a short explanation of the **whole library** as it currently exists: architecture, API contracts, compatibility, testing, benchmark evidence, decisions, limits, and what changed in the milestone. Use pstack how/why/teach if available. Link the document from the milestone closure summary.

## Conflict precedence
1. Security and maintainer requirements
2. Repository AGENTS.md, coding standards, GitHub issue scope, and verification
3. Primary external skill
4. Optional supporting external skills

External advice never overrides tests, acceptance criteria, explicit C# conventions, or the requirement for a PR. Independent Spec and Standards reviews remain required regardless of skill availability.
