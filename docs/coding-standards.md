# Coding Standards

These standards apply to production code, tests, scripts, and documentation in the HomeBudget Rates API repository unless a more specific nested `AGENTS.md` or domain-specific standard overrides them.

Rates API review and documentation guidance also lives in:

```text
docs/code-review-checklist.md
docs/pr-review-check.md
docs/pull_request_template.md
docs/rates-api-currency-catalog/
```

---

## 1. Purpose

The goal of these standards is to keep the codebase:

* correct;
* secure;
* maintainable;
* readable;
* testable;
* observable;
* easy to review;
* safe to change incrementally.

These standards are written for both humans and AI coding agents. They should guide implementation, refactoring, reviews, and production-readiness checks.

---

## 2. Engineering Priorities

When tradeoffs are required, use this priority order:

1. Correctness.
2. Data safety.
3. Security.
4. Public contract stability.
5. Simplicity.
6. Readability.
7. Maintainability.
8. Testability.
9. Observability.
10. Measured performance.

Do not optimize performance by weakening correctness, safety, validation, diagnostics, or contract clarity.

Do not add complexity unless it solves a current, proven problem.

---

## 3. Evidence-First Workflow

Before changing code, inspect the relevant repository evidence.

Required inspection scope depends on the task, but usually includes:

* nearest `AGENTS.md`;
* existing implementation in the touched area;
* existing tests;
* dependency injection registration;
* public contracts and DTOs;
* configuration/options;
* validators;
* repositories;
* mappers;
* factories;
* constants;
* helper APIs;
* test builders and fixtures;
* CI scripts and validation commands;
* existing documentation.

Reuse existing local patterns before introducing new ones.

If local patterns conflict, choose the safer, clearer, more recent, and more heavily tested pattern.

---

## 4. Change Scope

Keep changes small, focused, and reversible.

A single task should not mix unrelated categories, such as:

* behavior change and broad formatting cleanup;
* feature implementation and dependency upgrade;
* schema migration and unrelated refactoring;
* public contract change and internal cleanup;
* test framework replacement and business logic change;
* provider integration change and UI redesign;
* security change and style cleanup.

When you discover unrelated debt, do not expand the task automatically. Report it as follow-up unless fixing it is required for the current change to be correct.

---

## 5. Ambiguity Rules

Do not guess when ambiguity can affect correctness, safety, persistence, contracts, or production behavior.

First inspect repository evidence.

If evidence does not resolve the ambiguity, stop before changing production code and report:

* the uncertainty;
* why it matters;
* affected files, contracts, or behavior;
* options if clear;
* the smallest safe next step.

This rule is mandatory for:

* public API changes;
* persistence and migrations;
* data import/export;
* financial calculations;
* authentication and authorization;
* secrets and sensitive data;
* external provider behavior;
* caching;
* retries and timeouts;
* background jobs;
* distributed workflows;
* date/time boundaries;
* error contracts;
* destructive actions.

---

## 6. Architecture Principles

Use these principles pragmatically:

* SOLID;
* GRASP;
* KISS;
* YAGNI;
* DRY;
* Law of Demeter;
* high cohesion;
* low coupling;
* explicit dependencies;
* composition over inheritance.

Good architecture should make the current code easier to understand, test, and safely change.

Do not create abstractions only because they might be useful later.

Add abstraction only when it:

* removes current duplication;
* isolates volatile infrastructure;
* clarifies domain behavior;
* reduces method/class complexity;
* preserves a strong existing pattern;
* improves testability without hiding behavior.

---

## 7. Layering and Ownership

Keep behavior in the layer that owns the knowledge.

Typical boundaries:

* **Domain/application**: business rules, validation decisions, workflows, domain calculations.
* **Infrastructure**: database access, external clients, file system, message brokers, provider DTOs, framework-specific integration.
* **API/UI boundary**: request/response models, route semantics, transport validation, presentation concerns.
* **Tests**: fixtures, builders, fakes, mocks, behavior assertions.

Do not let infrastructure DTOs leak into domain or public contracts unless the architecture intentionally allows it.

Do not put business rules in controllers, HTTP clients, EF/Dapper mapping code, UI templates, or configuration binding unless the local architecture explicitly uses that pattern.

---

## 8. Public Contract Stability

Preserve public contracts unless the task explicitly requires a contract change.

Public contracts include:

* HTTP routes;
* HTTP methods;
* query parameters;
* request bodies;
* response bodies;
* status codes;
* error shapes;
* JSON property names;
* date/time formats;
* enum/string values;
* pagination and sorting behavior;
* database migration contracts;
* message/event schemas;
* CLI arguments;
* configuration keys;
* exported library APIs.

When changing a public contract intentionally:

* update tests;
* update examples;
* update OpenAPI/Swagger or equivalent metadata;
* update documentation;
* document migration impact;
* explain the behavior change in PR/final notes.

Do not expose raw infrastructure/provider errors through public contracts unless intentionally designed.

---

## 9. Domain Modeling

Prefer meaningful domain types over primitive values when they clarify business rules.

Use domain-specific types for concepts such as:

* IDs;
* codes;
* date ranges;
* money;
* rates;
* percentages;
* statuses;
* provider names;
* operation types;
* validation results;
* workflow decisions.

Avoid primitive obsession, such as passing loosely related `string`, `int`, `decimal`, and `bool` values through many layers.

Use enums when the value set is closed and stable.

Use value objects when validation and invariants matter.

Use records for immutable data carriers.

Do not create empty parameter bags only to reduce parameter count.

---

## 10. Method and Constructor Signatures

Prefer four or fewer method parameters.

Treat five method parameters as a soft maximum.

Prefer five or fewer constructor parameters.

Treat seven constructor parameters as a soft maximum.

When a signature grows, consider:

* request object;
* command object;
* query object;
* options object;
* context object;
* value object;
* domain-specific parameter object;
* splitting responsibilities;
* a cohesive facade service.

Do not hide unrelated dependencies behind generic service aggregators.

Avoid boolean flag arguments that make one method perform multiple workflows.

Prefer:

* separate methods;
* explicit enum;
* strategy object;
* options object;
* command/query type.

Bad:

```csharp
ProcessPayment(payment, true, false, true);
```

Better:

```csharp
ProcessIncomingPayment(command);
ProcessOutgoingPayment(command);
```

---

## 11. Code Organization

Prefer one top-level type per file.

Name the file after the main type.

Keep files under 150 lines when practical.

Treat 250 lines as a soft maximum.

Keep methods under 30 lines when practical.

Treat 50 lines as a soft maximum.

Keep nesting depth at 3 or less when practical.

Prefer guard clauses over deep nesting.

Keep folder structure aligned with existing feature/domain conventions.

Do not create new folder patterns unless the existing structure cannot reasonably support the change.

Do not move files only for style preferences.

---

## 12. Naming

Use names that describe domain meaning and responsibility.

Avoid vague names when a domain name is available:

* `Manager`;
* `Helper`;
* `Processor`;
* `Util`;
* `Handler` without a specific use case;
* `Service` without domain meaning.

Good names make invalid usage harder.

Prefer:

```text
CurrencyRateResolver
PaymentAccountImporter
ContractorMappingResult
RateHistoryRequest
OperationDateRange
ProviderResponseClassifier
```

Over:

```text
RateHelper
DataProcessor
CommonManager
Utils
Service2
```

Use consistent terminology from the domain and public contracts.

---

## 13. Code Smell and Anti-Pattern Gate

Every change must avoid introducing or worsening:

* duplication;
* copy-paste programming;
* long methods;
* large classes;
* god objects;
* deep nesting;
* long conditional chains;
* long switch chains;
* primitive obsession;
* magic strings/numbers;
* data clumps;
* long parameter lists;
* boolean flag arguments;
* generic dictionaries used as domain models;
* service locator patterns;
* global mutable state;
* hidden side effects;
* temporal coupling;
* tight coupling;
* low cohesion;
* inappropriate intimacy;
* feature envy;
* shotgun surgery;
* lazy classes;
* middle-man abstractions;
* leaky abstractions;
* speculative generality;
* premature optimization;
* broad catch blocks;
* swallowed errors;
* inconsistent logging;
* framework/vendor misuse.

Fix touched-scope smells when the fix is local, safe, and relevant.

Do not perform broad cleanup without request.

Report residual risks as follow-up.

---

## 14. C#/.NET Standards

Respect nullable reference types.

Use `sealed` classes when inheritance is not intended.

Prefer immutable records for simple data carriers.

Use `readonly record struct` only for small value-like types where value semantics are clear.

Pass `CancellationToken` through async I/O paths.

Do not use:

```csharp
.Result
.Wait()
GetAwaiter().GetResult()
```

unless the code is at a documented safe synchronous boundary.

Avoid `dynamic`, `object`, reflection, or weakly typed bags when a strong DTO/model is reasonable.

Keep LINQ readable. Use named intermediate variables or extracted methods when expressions become dense.

Use pattern matching when it improves clarity.

Do not use pattern matching when simple `if` statements are clearer.

Do not suppress nullable, analyzer, or compiler warnings without a clear explanation.

---

## 15. Async and Cancellation

Async I/O must remain async all the way.

Pass cancellation tokens through:

* HTTP calls;
* database calls;
* file I/O;
* message broker calls;
* background job operations;
* long-running computations where practical.

Do not convert cancellation into generic failure.

Do not catch `OperationCanceledException` and log it as an error unless cancellation is unexpected and actionable.

Timeout and cancellation are different conditions. Preserve that distinction where the architecture allows it.

---

## 16. Errors and Results

Follow existing repository error/result patterns.

Errors must be actionable.

Prefer machine-readable errors where clients or workflows need to react programmatically.

Do not use only human-readable messages for domain or contract-significant failures.

Do not swallow exceptions.

Avoid broad catch blocks.

If a broad catch is necessary at a boundary:

* classify the error;
* preserve useful context;
* avoid leaking sensitive data;
* convert to the repository’s standard error/result shape;
* log once at the appropriate boundary.

Do not log and rethrow repeatedly.

---

## 17. Logging and Observability

Use structured logging templates.

Good:

```csharp
_logger.LogWarning(
    "Provider request failed for {ProviderName} with status {StatusCode}.",
    providerName,
    statusCode);
```

Bad:

```csharp
_logger.LogWarning($"Provider request failed for {providerName}: {statusCode}");
```

Include context useful for diagnosis:

* operation name;
* domain identifier where safe;
* provider or subsystem;
* status code;
* retry attempt;
* elapsed time;
* correlation/request ID when available.

Do not log:

* passwords;
* tokens;
* API keys;
* private keys;
* authorization headers;
* connection strings;
* raw sensitive payloads;
* personal or financial data unless explicitly safe and required.

Metrics and tracing must follow existing repository instrumentation patterns.

---

## 18. Configuration

Configuration should describe environment-specific behavior, not replace domain rules.

Good configuration examples:

* base URLs;
* timeouts;
* retry counts;
* cache durations;
* feature toggles;
* safe default options;
* provider names;
* batch sizes;
* limits.

Bad configuration examples:

* secrets in shared files;
* hard-coded complete external catalogs;
* production connection strings;
* machine-specific paths;
* business rules duplicated from code;
* stale copies of provider metadata without versioning or validation.

Validate options at startup where possible.

Fail fast for missing required configuration.

Use typed options instead of stringly typed configuration access spread across the codebase.

---

## 19. Secrets and Security

Do not commit:

* passwords;
* tokens;
* API keys;
* private keys;
* certificates;
* production connection strings;
* machine-specific hosts;
* real secrets in examples;
* sensitive data exports.

Use:

* user secrets;
* environment variables;
* ignored local config;
* secure CI/CD secret storage.

Do not expose internal infrastructure diagnostics through public APIs.

Do not include secrets in logs, exceptions, test snapshots, screenshots, docs, or generated artifacts.

---

## 20. External Integrations

Keep external system details isolated.

External integration code should own:

* provider routes;
* provider DTOs;
* request query parameters;
* response parsing;
* status-code interpretation;
* retry/timeout behavior;
* provider-specific error classification.

Application/domain code should work with stable internal concepts.

Do not leak external DTOs into public API contracts unless that is already the documented contract.

Retry only safe transient failures.

Do not retry:

* validation errors;
* invalid client requests;
* unauthorized requests unless token refresh is implemented;
* known non-transient provider errors;
* domain “not found” results;
* no-data results that are valid provider responses.

Keep base URLs, timeouts, retries, and rate-limiter settings configurable.

---

## 21. Caching

Do not add caching unless it solves a current problem and has a clear expiration or invalidation policy.

Cache keys must include every input that affects the result.

Cache values must not hide error semantics.

Do not return stale data unless the contract explicitly allows stale-on-error behavior.

When changing cache behavior, test:

* key shape;
* hit;
* miss;
* expiration;
* invalidation;
* error behavior;
* cancellation;
* concurrency where relevant.

---

## 22. Date and Time

Use the correct temporal type:

* `DateOnly` for business dates without time;
* `TimeOnly` for time-of-day without date;
* `DateTimeOffset` for absolute moments in time;
* `DateTime` only when local repository conventions require it and ambiguity is controlled.

Use an injected clock/time provider when available.

Avoid direct calls in business logic to:

```csharp
DateTime.Now
DateTime.UtcNow
DateOnly.FromDateTime(DateTime.Now)
DateOnly.FromDateTime(DateTime.UtcNow)
```

Do not mix date-only provider semantics with timezone conversion unless the public contract explicitly requires it.

Document and test date boundaries:

* inclusive start;
* inclusive or exclusive end;
* same-day range;
* empty range;
* invalid range;
* leap day where relevant.

---

## 23. Money, Decimal, and Precision

Use `decimal` for:

* money;
* rates;
* percentages that affect money;
* financial calculations;
* persisted financial values;
* sums and comparisons of financial values.

Do not use `double` or `float` for financial values.

Define rounding only at explicit boundaries:

* display;
* public contract;
* persistence;
* provider conversion;
* reporting;
* external system compatibility.

Do not round internal calculations unless the domain rule requires it.

Preserve scale and source values when auditability matters.

Tests for financial logic should include:

* zero;
* negative values;
* high precision;
* boundary values;
* scale conversion;
* rounding behavior;
* culture-invariant serialization.

---

## 24. Persistence and Migrations

Do not change schema, indexes, seed data, migrations, or stored configuration together with unrelated refactoring.

Migrations must preserve existing data unless the task explicitly requires data migration or cleanup.

Use idempotent migration scripts where the repository migration tool supports them.

When changing persistence behavior, include:

* migration impact;
* rollback or remediation notes;
* existing data compatibility;
* tests where practical;
* documentation update.

Do not silently reinterpret historical records.

Do not change primary keys, natural keys, uniqueness rules, or identity semantics without explicit task approval and tests.

---

## 25. DTOs and Mapping

DTOs should model their boundary accurately.

Use separate models when semantics differ between:

* public API contract;
* domain/application model;
* persistence model;
* external provider payload;
* message/event schema.

Mapping must be explicit when:

* names differ;
* semantics differ;
* scale conversion occurs;
* date conversion occurs;
* error classification occurs;
* identity is transformed;
* optional source fields become required target fields.

Avoid hidden mapping behavior.

Tests should cover:

* normal mapping;
* missing optional fields;
* missing required fields;
* invalid values;
* boundary date/time values;
* decimal precision;
* enum/status conversion;
* malformed external payloads where relevant.

---

## 26. Validation

Validate at runtime boundaries:

* controllers/endpoints;
* request DTOs;
* command/query handlers;
* options binding;
* provider responses;
* persistence inputs;
* background job inputs;
* cache inputs;
* message consumers;
* public service methods.

Make invalid state fail fast where possible.

Use domain types, enums, constants, value objects, and validators to prevent invalid states.

Do not duplicate validation rules across layers unless each layer needs boundary-specific validation.

Prefer existing validators and validation patterns.

---

## 27. Dependency Management

Do not add dependencies unless clearly needed.

A new dependency must be:

* relevant to the current task;
* maintained;
* production-safe;
* license-compatible;
* not excessive for the problem;
* consistent with repository architecture;
* better than existing local utilities.

Do not upgrade unrelated dependencies as part of a feature or bug fix.

Do not add libraries to hide design problems.

Do not use package upgrades to bypass tests, compiler errors, or peer-dependency conflicts.

---

## 28. Testing Standards

Add or update tests for changed behavior where practical.

Prefer behavior-focused tests over implementation-detail tests.

Tests should be deterministic.

Avoid real time, real network, real external providers, and shared mutable state in unit tests.

Use fakes/mocks/test doubles according to local patterns.

Good tests usually cover:

* success path;
* validation failure;
* boundary values;
* error classification;
* cancellation where relevant;
* persistence edge cases;
* serialization/deserialization;
* mapping edge cases;
* idempotency where relevant.

Do not delete, weaken, or skip tests merely to make a change pass.

When a test is intentionally skipped, document why and what is required to enable it.

---

## 29. Validation Commands

Use the smallest relevant validation first, then broaden when risk warrants it.

For .NET work, typical commands are:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet format --verify-no-changes
```

For targeted validation, prefer project-level commands first:

```bash
dotnet build <project>.csproj
dotnet test <test-project>.csproj
```

For shared or contract-impacting changes, use solution-level validation:

```bash
dotnet build <solution>.sln
dotnet test <solution>.sln
```

Adapt commands to repository scripts.

Do not run:

* production deployments;
* destructive scripts;
* real external-provider tests;
* infrastructure-mutating tests;
* data cleanup jobs;
* source-map uploads;
* release/publish scripts;

unless they are explicitly documented as safe and required.

Report validation honestly as:

* passed;
* failed;
* not run;
* skipped.

Include the reason for failed, skipped, or not-run commands.

---

## 30. Refactoring

Refactoring must preserve behavior unless a behavior change is explicitly requested.

Prefer incremental improvements:

* extract focused methods;
* improve naming;
* reduce duplication;
* simplify conditionals;
* introduce cohesive value objects;
* isolate infrastructure details;
* remove clearly dead touched-scope code;
* improve tests around changed behavior.

Avoid:

* broad rewrites;
* speculative extension points;
* architecture redesign;
* dependency replacement;
* unrelated formatting churn;
* moving files without need;
* changing public contracts as cleanup;
* weakening tests or compiler rules.

Document intentional deviations.

---

## 31. Documentation

Document non-obvious decisions.

Prefer clearer code over comments explaining confusing code.

Update documentation when changing:

* public API behavior;
* configuration;
* error semantics;
* persistence schema;
* external integration behavior;
* cache behavior;
* date/time behavior;
* financial calculation behavior;
* operational validation steps;
* deployment/runtime assumptions.

Do not add placeholder documentation that does not help future maintainers.

---

## 32. Markdown and Repository Docs

Documentation should be accurate, concise, and operationally useful.

Use clear headings.

Prefer examples that can be copied safely.

Do not include secrets, real personal data, real financial data, or machine-specific paths unless clearly marked as examples and safe.

When documenting commands, say where to run them.

When documenting validation, include expected success criteria.

When documenting known limitations, be explicit about what is not proven.

---

## 33. Review Standards

For each review, check:

* correctness;
* public contract impact;
* security;
* data safety;
* test coverage;
* maintainability;
* observability;
* error handling;
* configuration safety;
* dependency impact;
* backward compatibility;
* validation evidence.

Use severity levels:

* **P0 Blocker** — correctness, data loss, security, production outage, broken public contract, or incorrect financial behavior.
* **P1 Critical** — high-risk maintainability, unsafe integration behavior, missing tests for changed behavior, or ambiguous public behavior.
* **P2 Major** — localized design debt, readability issue, incomplete diagnostics, or meaningful test coverage gap.
* **P3 Minor** — naming, formatting, comments, or small consistency issue.

Fix P0/P1 issues in the touched scope when safe.

Report P2/P3 as follow-up when fixing them would broaden the task.

---

## 34. AI Agent Final Notes

Every implementation response must include:

1. Summary.
2. Public behavior changes, if any.
3. Files changed, grouped by purpose.
4. Tests added or updated.
5. Validation commands and results.
6. Known risks or limitations.
7. Follow-up work, if any.
8. Intentional deviations from these standards, if any.

If complexity was added, explain why it is needed now.

Examples of complexity requiring explanation:

* new abstraction;
* new dependency;
* new branch;
* new contract;
* new migration;
* new cache;
* new retry policy;
* new background/asynchronous step;
* new provider integration.

Do not claim success without evidence.

---

## 35. Project-Specific Addenda

Keep this document focused on shared HomeBudget Rates API engineering rules.

When a project or feature needs stricter rules, create a focused addendum and link it from `AGENTS.md` or the nearest nested `AGENTS.md`.

Useful addenda for this repository include:

```text
docs/code-review-checklist.md
docs/pr-review-check.md
docs/rates-api-currency-catalog/
```

Addenda should contain only domain-specific rules, not duplicate this whole document.

For example, a Rates API addendum may define rules for:

* provider catalog metadata;
* currency identity;
* rate scale;
* date-range limits;
* provider-specific error classification;
* historical-rate reconciliation.

The root standards remain the shared baseline.
