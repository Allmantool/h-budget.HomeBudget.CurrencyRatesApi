# Repository Engineering Instructions

This repository is a C#/.NET rates API solution. Follow these instructions for Codex/code-generation work and human review in this repo.

## Engineering Priorities

Optimize in this order:

1. Correctness.
2. Simplicity.
3. Readability.
4. Maintainability.
5. Testability.
6. Performance only where needed and measured.

## Core Principles

- Apply SOLID and GRASP pragmatically.
- Prefer KISS and YAGNI over speculative layers, extension points, or abstractions.
- Use DRY to remove harmful duplication, but do not create premature abstractions.
- Follow the Law of Demeter: avoid exposing or navigating deep object graphs.
- Prefer composition over inheritance.
- Keep dependencies explicit through constructors or method parameters.
- Keep units small, cohesive, and named for the domain.
- Preserve public behavior and public contracts unless the task explicitly requires a change.
- Do not perform broad rewrites, mass formatting, or unrelated cleanup unless explicitly requested.

## File and Type Organization

- Prefer one top-level class, record, interface, enum, or exception per file.
- File names should match the main type name.
- Keep related but separate responsibilities in separate files.
- Avoid large "god files" and mixed infrastructure/domain/transport responsibilities.
- Use feature/domain-based folders where the current project structure supports it.
- Target file size: under 150 lines.
- Soft maximum file size: 250 lines.
- Target method size: under 30 lines.
- Soft maximum method size: 50 lines.
- Maximum nesting depth: 3.

These are guidance limits, not blind mechanical rules. If exceeding them is the clearest option, explain why in the final response.

## Method and Constructor Parameters

- Preferred maximum method parameters: 4.
- Soft maximum method parameters: 5.
- Preferred maximum constructor parameters: 5.
- Soft maximum constructor parameters: 7.

When a signature exceeds these limits, consider a request, command, options, context, value, or other domain-specific parameter object. Do not create meaningless parameter bags only to satisfy a number.

## Static Classes and Extension Methods

Use static classes for:

- Pure helper methods.
- Extension methods.
- Mapping helpers.
- Constants grouped by a domain concept.

Do not use static classes for:

- Business workflows with dependencies.
- I/O.
- Logging.
- Configuration-dependent behavior.
- Mutable shared state.

Extension methods must improve readability and must not hide expensive side effects.

## C# and .NET Practices

- Respect nullable reference types and avoid suppressing nullable warnings without a clear reason.
- Async methods that perform I/O should accept and pass `CancellationToken`.
- Do not use `.Result`, `.Wait()`, or other sync-over-async patterns.
- Use structured logging message templates.
- Do not log secrets, credentials, tokens, personal data, or sensitive business data.
- Prefer `sealed` classes when inheritance is not intended.
- Prefer records for immutable data carriers.
- Prefer clear domain names over vague names such as `Manager`, `Helper`, `Processor`, or `Util`.
- Avoid large LINQ expressions when they harm readability or debugging.
- Keep EF Core queries translatable when EF Core is used; avoid accidental client evaluation.
- Preserve existing telemetry, correlation, authorization, validation, cancellation, timeout, retry, and resource-disposal behavior.

## Refactoring Rules

- Preserve behavior unless explicitly asked to change it.
- Keep changes incremental and reviewable.
- Avoid unrelated changes and large rewrites.
- Keep public contracts stable unless the task requires changing them.
- Prefer extracting private methods before introducing new services.
- Extract classes/services only when they reduce real complexity, coupling, or duplication.
- Add or update tests for changed behavior where practical.
- Explain trade-offs and intentional deviations in the final response.

## Definition of Done

Final responses for non-trivial work must include:

- What changed.
- Why the design or configuration was chosen.
- What checks were run.
- Any checks that could not be run.
- Any intentional deviations from these standards.
