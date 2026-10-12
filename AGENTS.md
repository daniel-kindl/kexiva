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
