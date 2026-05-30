# Clean Code Review

Use this skill when reviewing or refactoring C#/.NET code for maintainability, readability, SOLID, GRASP, KISS, YAGNI, Law of Demeter, complexity, file size, method size, parameter count, dependency design, testability, and code-review readiness.

## Review Workflow

1. Read the changed code and nearby conventions before proposing changes.
2. Separate correctness issues from maintainability concerns.
3. Prefer the smallest behavior-preserving fix that improves clarity or safety.
4. Check whether tests cover the behavior being changed.
5. Avoid introducing new abstractions unless they remove real complexity, coupling, or duplication.

## Finding Severity

Report findings in this order:

- Blocking: likely bug, build/test failure, security issue, broken public contract, data loss risk, or production reliability issue.
- Should Fix: maintainability, readability, testability, architecture, async/cancellation, logging, or analyzer issue that creates meaningful future risk.
- Nice to Have: polish, naming, minor duplication, or local simplification that is useful but not required.

## Finding Format

Each finding should include:

- File/symbol.
- Problem.
- Why it matters.
- Suggested fix.
- Whether the fix is safe or behavior-changing.

## Review Checklist

- Correctness and edge cases.
- Simplicity and KISS/YAGNI.
- SOLID and GRASP responsibilities.
- Law of Demeter and dependency boundaries.
- File/type organization: one top-level type per file where practical.
- Method size, nesting depth, and cognitive complexity.
- Method and constructor parameter count.
- Async, cancellation, timeouts, retries, and resource disposal.
- Structured logging with no sensitive data.
- Validation, authorization, and security behavior.
- Test coverage and deterministic test design.
- Analyzer, formatter, and build results.
