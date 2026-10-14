## Summary

<!-- What changed and why. -->

Closes #

## Type

- [ ] feat
- [ ] fix
- [ ] perf
- [ ] docs
- [ ] test
- [ ] refactor
- [ ] build / ci / chore

## Verification

<!-- Paste the end of eng/verify.sh or eng/verify.ps1 output. Mark platforms you did not run as NOT RUN. -->

| Platform | Result |
| --- | --- |
| Linux x64 | |
| Windows x64 | |
| macOS ARM64 | |
| macOS x64 | |

## Checklist

- [ ] The change stays in the issue scope. No unrelated changes.
- [ ] Tests were written first and cover the change (differential tests against .NET for API work).
- [ ] `eng/verify.*` passes. No new warnings or suppressions.
- [ ] Public API changes are recorded in `PublicAPI.Unshipped.txt`.
- [ ] Lock files changed only because packages changed.
- [ ] Docs and ADRs are updated.
- [ ] Conventional Commits title.

## Reviews

<!-- Link evidence for each independent review. Do not tick without a link. -->

- [ ] Specification:
- [ ] Standards:
- [ ] Correctness / tests:
