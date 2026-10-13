# AGENTS.md

Authoritative entry point for AI agents contributing to Kexiva.

## Principles
- DRY, SOLID, KISS, YAGNI
- TDD: tests before implementation
- Correctness first; performance by evidence
- No silent scope expansion

## Mandatory Workflow
1. Select an eligible GitHub Issue (`status:ready`).
2. Confirm dependencies and acceptance criteria.
3. Assign yourself.
4. Mark `status:in-progress`.
5. Create short-lived branch (`feat/123-...`).
6. Write tests first where applicable.
7. Implement the smallest correct solution.
8. Run `./eng/verify.sh` (or `eng/verify.ps1`).
9. Request independent specification and standards reviews.
10. Address findings.
11. Open PR with `Closes #123`.
12. Merge only after required CI and approvals.
13. Close issue after merge.

## Prohibitions
- Bypass CI or quality gates.
- Weaken tests or analyzers.
- Invent benchmark results.
- Modify unrelated code.
- Close issues prematurely.
- Implement production parsing/encoding/serialization during foundation work.

## Links
- [Testing standards](docs/standards/testing.md)
- [Benchmark methodology](docs/benchmarks/methodology.md)
- [Engineering scripts](eng/README.md)

Agents must follow this document.

## Engineering skills and controlled English
- Follow [agent workflow ownership](docs/standards/agent-skills.md). Repository checks and GitHub Issues remain authoritative over external skill guidance.
- Use Matt Pocock's engineering workflow for requirements, tickets, implementation, TDD, and code review when installed.
- Use Poteto pstack for optional architecture analysis, benchmark investigations, and milestone knowledge transfer. Do not run competing orchestration skills for the same task.
- Follow [technical writing guidance](docs/standards/technical-writing.md) for documentation, XML docs, issue text, and PRs. ASD-STE100 is an inspiration; do not claim certified or complete compliance.
- External skills are optional local tooling. Check their availability and exact documented names before use; do not block normal issue delivery when unavailable.
