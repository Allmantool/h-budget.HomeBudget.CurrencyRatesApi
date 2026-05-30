# Code Review Checklist

Use this checklist for pull requests and larger Codex-generated changes.

## Correctness

- The change solves the stated problem without unrelated behavior changes.
- Edge cases, null/default values, empty collections, and error paths are handled.
- Public contracts are unchanged or intentionally documented.

## Design and Maintainability

- Responsibilities follow SOLID and GRASP.
- The implementation stays simple and avoids speculative abstraction.
- Duplication is reduced where harmful, without premature generalization.
- Dependencies are explicit and testable.
- Law of Demeter is respected; deep object chains are avoided.

## Organization and Readability

- Files and types are cohesive.
- One top-level type per file is followed where practical.
- File names match the main type.
- Methods are small enough to understand easily.
- Nesting and cognitive complexity are reasonable.
- Method and constructor parameter counts stay within the repository guidance or have a clear reason.
- Names describe domain intent.

## Runtime Behavior

- Async I/O accepts and propagates `CancellationToken`.
- No `.Result`, `.Wait()`, or sync-over-async.
- Timeout, retry, idempotency, disposal, telemetry, and correlation behavior are preserved.
- Structured logs are useful and do not expose secrets or sensitive data.
- Validation, authorization, and security checks are preserved.

## Tests and Quality Gates

- Tests are added or updated for changed behavior where practical.
- Tests are deterministic and focused on observable behavior.
- `dotnet build` passes.
- `dotnet test` passes or failures are explained.
- `dotnet format --verify-no-changes` passes or formatting/analyzer findings are explained.
- Analyzer/linter warnings introduced by the change are fixed or explicitly justified.
