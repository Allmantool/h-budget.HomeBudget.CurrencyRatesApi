# Coding Standards

These standards complement `AGENTS.md` and apply to C#/.NET work in this repository.

## Priorities

Favor correctness first, then simplicity, readability, maintainability, testability, and measured performance.

## Design

- Keep domain/business logic separate from infrastructure, transport, persistence, and framework code where the current architecture supports it.
- Apply SOLID and GRASP pragmatically.
- Prefer explicit dependencies and composition over hidden global state or inheritance.
- Avoid speculative abstractions, broad rewrites, and unrelated cleanup.
- Keep public contracts stable unless a task intentionally changes them.

## Organization

- Prefer one top-level type per file with the file named after the main type.
- Keep files under 150 lines when practical; treat 250 lines as a soft maximum.
- Keep methods under 30 lines when practical; treat 50 lines as a soft maximum.
- Keep nesting depth at 3 or less when practical.
- Split responsibilities by feature/domain folder conventions already used in the solution.

## Signatures

- Prefer 4 or fewer method parameters; treat 5 as a soft maximum.
- Prefer 5 or fewer constructor parameters; treat 7 as a soft maximum.
- When a signature grows, consider a domain-specific request, command, options, context, value, or parameter object.
- Do not create empty parameter bags only to satisfy a count.

## C#/.NET

- Respect nullable reference types.
- Pass `CancellationToken` through async I/O paths.
- Avoid `.Result`, `.Wait()`, and sync-over-async.
- Use structured logging templates and avoid logging secrets or sensitive data.
- Prefer `sealed` classes when inheritance is not intended.
- Prefer records for immutable data carriers.
- Avoid vague names such as `Manager`, `Helper`, `Processor`, or `Util` when a domain name is available.
- Keep LINQ readable; use named intermediate values or methods when expressions become dense.

## Refactoring

- Preserve behavior unless a behavior change is requested.
- Make incremental, reviewable changes.
- Add or update tests for changed behavior where practical.
- Document intentional deviations from these standards in PRs or final implementation notes.
