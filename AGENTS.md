# AGENTS.md

# Repository Agent Instructions

This file is the primary entry point for AI coding agents working in this repository.

This repository is the HomeBudget Rates API. The main areas are:

* `HomeBudget.Rates.Api` for the ASP.NET Core host and API configuration;
* `HomeBudget.Components.CurrencyRates` for NBRB rates integration and currency-rate workflows;
* `HomeBudget.Components.Exchange` for exchange calculations;
* `HomeBudget.DataAccess*` for persistence;
* `HomeBudget.Components.*.Tests` and `HomeBudget.Rates.Api.Tests` for unit and integration coverage;
* `docs` for repository standards, PR review guidance, and design notes.

Detailed engineering standards live in:

```text
docs/coding-standards.md
```

Read this file first, then read the nearest nested `AGENTS.md` for the area being changed, then read the detailed standards and local implementation patterns.

---

## 1. Instruction Order

When instructions conflict, follow this order:

1. Platform/system instructions.
2. User task instructions.
3. Nearest `AGENTS.md`.
4. Root `AGENTS.md`.
5. `docs/coding-standards.md`.
6. Existing local code patterns.
7. General framework or language conventions.

Do not use a local pattern if it is clearly unsafe, incorrect, obsolete, or in conflict with higher-priority instructions.

---

## 2. Operating Principle

Work evidence-first.

Before changing code, inspect the repository evidence for the touched area:

* current implementation;
* tests;
* dependency injection setup;
* public contracts;
* configuration/options;
* validators;
* mappers;
* repositories;
* domain models;
* helper APIs;
* test builders and fixtures;
* nearby naming, folder, and error-handling patterns.

Prefer reuse over invention.

Do not add abstractions, dependencies, configuration, or new patterns until existing project patterns are proven insufficient.

---

## 3. Change Discipline

Keep changes small, focused, reversible, and directly tied to the task.

Do not bundle unrelated work, such as:

* feature implementation plus broad refactoring;
* contract changes plus formatting cleanup;
* dependency upgrades plus behavior changes;
* schema changes plus unrelated code cleanup;
* test framework changes plus application logic changes;
* logging/observability redesign plus bug fix.

When broader problems are found, fix only the safe touched-scope issue and report the remaining risk as follow-up.

Do not start broad legacy cleanup unless explicitly requested.

---

## 4. Ambiguity and Stop Conditions

Do not guess when the decision affects correctness, public behavior, persistence, security, financial calculations, external integrations, or operational safety.

If repository evidence does not resolve the ambiguity, stop before changing production code and report:

* what is uncertain;
* why it matters;
* what behavior or contract is affected;
* the smallest safe next step.

This is mandatory for changes involving:

* public API contracts;
* database schema or migrations;
* external provider behavior;
* authentication or authorization;
* security-sensitive data;
* financial or decimal calculations;
* date/time boundaries;
* caching;
* retries, timeouts, or rate limiting;
* background workflows;
* error contracts;
* data import/export;
* production configuration.

---

## 5. Architecture Expectations

Follow `docs/coding-standards.md`.

Default architecture rules:

* keep domain/application behavior separate from infrastructure details;
* keep provider/client DTOs at infrastructure boundaries;
* preserve public contracts unless a contract change is explicitly requested;
* prefer explicit dependencies and composition;
* avoid hidden global state;
* avoid broad rewrites;
* avoid speculative abstractions;
* make invalid state fail fast at runtime boundaries;
* keep code cohesive, readable, and testable.

Use SOLID, GRASP, KISS, YAGNI, DRY, and Law of Demeter pragmatically. Do not use these principles to justify unnecessary indirection.

---

## 6. Code Quality Gates

Do not introduce:

* duplicated logic;
* long methods or large classes;
* god objects;
* deep nesting;
* long parameter lists;
* boolean flag arguments that create multiple workflows;
* primitive obsession where a domain type would clarify intent;
* magic strings or magic numbers;
* service locator patterns;
* broad catch blocks that hide failures;
* swallowed exceptions;
* sync-over-async;
* weakly typed bags where strong models are reasonable;
* unrelated dependencies;
* framework misuse;
* tests that do not assert meaningful behavior.

Fix touched-scope smells when the fix is local, safe, and relevant to the task. Otherwise report the issue as follow-up.

---

## 7. C#/.NET Defaults

Unless local standards say otherwise:

* respect nullable reference types;
* use `sealed` classes when inheritance is not intended;
* prefer immutable records for simple data carriers;
* pass `CancellationToken` through async I/O paths;
* never use `.Result`, `.Wait()`, or sync-over-async;
* use structured logging templates;
* avoid logging secrets or sensitive data;
* keep LINQ readable;
* prefer clear names over comments explaining confusing code;
* use domain-specific names instead of vague `Manager`, `Helper`, `Processor`, or `Util`.

Financial, rate, or money calculations must use `decimal`, not `double` or `float`.

---

## 8. API and Contract Safety

Preserve existing public contracts unless the task explicitly requires a contract change.

Do not accidentally change:

* routes;
* HTTP methods;
* query parameters;
* status codes;
* JSON property names;
* response envelopes;
* error response shapes;
* date formats;
* sorting or pagination behavior.

When a contract changes intentionally, update relevant tests, examples, OpenAPI/Swagger metadata, and documentation.

---

## 9. Configuration and Secrets

Do not commit:

* passwords;
* tokens;
* API keys;
* private keys;
* production connection strings;
* developer-only service URLs;
* machine-specific values;
* real secrets in examples.

Use safe placeholders in shared configuration.

Use user secrets, environment variables, ignored local config, or secure CI/CD secrets for sensitive values.

---

## 10. Testing Expectations

Add or update tests for changed behavior where practical.

Prefer deterministic unit tests for domain/application behavior.

Do not call real external providers in unit tests.

External integration tests must be clearly marked, configurable, and safe to skip when network, credentials, or infrastructure are unavailable.

Do not delete, weaken, or skip tests merely to make a change pass.

---

## 11. Validation

Use the smallest relevant validation first, then broaden when risk warrants it.

For .NET work, typical validation order is:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet format --verify-no-changes
```

Adapt commands to the actual solution, project, and repository scripts.

Do not run production, destructive, external-provider, or infrastructure-mutating commands unless they are documented as safe for local validation.

Do not claim validation passed unless the command was actually run in this workspace and exited successfully.

If validation was not run, failed, or was blocked, report that honestly with the reason.

---

## 12. Final Response Requirements

Every coding task response must include:

1. Summary of what changed.
2. Public behavior changes, if any.
3. Files changed, grouped by purpose.
4. Tests added or updated.
5. Validation commands and results.
6. Known risks or limitations.
7. Follow-up work, if any.
8. Any intentional deviation from these instructions or `docs/CODING_STANDARDS.md`.

If complexity was added, explain why it is needed now.

Do not claim success without evidence.

---

## 13. Severity Language for Reviews

Use this severity model for audit and review findings:

* **P0 Blocker** — correctness, data loss, security, production outage, broken public contract, or incorrect financial behavior.
* **P1 Critical** — high-risk maintainability, missing tests for changed behavior, unsafe integration behavior, or ambiguous public behavior.
* **P2 Major** — localized design debt, readability issue, incomplete diagnostics, or meaningful test coverage gap.
* **P3 Minor** — naming, formatting, comments, or small consistency issue.

Fix P0/P1 issues in the touched scope when safe.

Report P2/P3 issues as follow-up when fixing them would broaden the task.
