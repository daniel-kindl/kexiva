# Technical writing standard

Use clear, consistent English based on ASD-STE100 principles. This is **not** a claim of complete ASD-STE100 compliance or certification.

## Scope
Use this guidance for READMEs, design documents, public XML documentation, source comments, GitHub Issues, PRs, and agent reports. Do not rename public APIs, C# identifiers, CLI flags, product names, or established technical terms merely to fit controlled vocabulary.

## Writing rules
- Prefer short, direct sentences with one main instruction or fact.
- Use active voice and consistent terms for the same concept.
- Use imperative verbs for instructions and observable outcomes for acceptance criteria.
- Define ambiguous acronyms on first use; keep identifiers exact.
- Use a numbered sequence for steps that must execute in order.
- Avoid filler, unexplained pronouns, ambiguous negatives, and unsupported claims.
- State conditions, units, defaults, limits, and failure behavior explicitly.
- Distinguish measured performance from hypotheses. Include dataset, hardware, OS, runtime, and method for performance claims.
- For public APIs, document observable behavior, exceptions, edge cases, and allocation guarantees only when verified.

## Kexiva glossary
| Term | Meaning |
| --- | --- |
| API compatibility | Supported public signatures and documented calling patterns match the intended .NET API |
| Behavioral compatibility | Results, errors, edge cases, and relevant defaults match the reference API |
| Allocation | Managed heap memory allocated during an operation; specify measurement scope |
| Throughput | Completed operations or processed bytes per unit time |
| Latency | Time for one operation under a defined benchmark workload |
| SIMD | Single instruction, multiple data; CPU vector operations |
| Span<T> | Contiguous, bounded memory view used by .NET |
| Differential test | Test comparing Kexiva and a .NET reference for the same inputs |

## Review
Review documents and public XML comments for clarity and terminology. The repository's existing analyzer and formatting gates remain the only automated enforced checks until a writing linter is separately approved, implemented, and validated. Do not represent a simple spelling checker as full STE verification.

ASD-STE100 texts and controlled vocabularies may be copyrighted. Link to the official source; do not copy the standard into this repository.
