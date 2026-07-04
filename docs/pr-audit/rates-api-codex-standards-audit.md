# Rates API PR Diff and Codex Standards Audit

## Summary

PARTIAL.

The literal `git diff master...HEAD` is empty because `HEAD` equals `master` at audit time. The actual PR payload was present as staged working-tree changes. I audited the staged diff, removed the staged-only copied Codex automation bundle from the worktree, replaced copied Titan/VC/Camunda/Jira standards with HomeBudget Rates API standards, and cleared machine-specific/default secret values from `HomeBudget.Rates.Api/appsettings.json`.

The Rates API refactor moves toward provider catalog-based currency resolution and is directionally correct for multi-currency support. Build and tests pass on the cleaned staged payload, but merge readiness remains partial because `dotnet format --verify-no-changes` still fails on broad repository formatting/analyzer issues and one provider current-date policy risk remains.

## Diff scope

Baseline commands:

- `git status --short`: staged working-tree payload present.
- `git branch --show-current`: `tech/enhance-list-of-supported-currencies`.
- `git rev-parse --show-toplevel`: repository root confirmed.
- `git diff --name-status master...HEAD`: no output.
- `git diff --stat master...HEAD`: no output.
- `dotnet --info`: SDK `10.0.301`, runtimes for .NET 5-10 installed.
- `dotnet --list-sdks`: `9.0.118`, `10.0.301`.

Changed files reviewed by category:

| Path | Category | Purpose | Belongs | Risk | Action |
|---|---|---|---|---|---|
| `AGENTS.md` | D. Codex/agent standards | Main repository Codex standards | Yes | Low | Edited to Rates API-specific standards |
| `docs/coding-standards.md` | D. Codex/agent standards | Coding standards | Yes | Low | Edited to Rates API-specific standards |
| `docs/code-review-checklist.md` | D. Codex/agent standards | Review checklist | Yes | Low | Edited to Rates API-specific checklist |
| `docs/pr-review-check.md` | D. Codex/agent standards | PR review rubric | Yes | Low | Replaced copied rubric with Rates API rubric |
| `docs/pull_request_template.md` | F. Documentation | PR checklist | Yes | Low | Replaced copied template with Rates API checklist |
| `.codex/skills/**` added files | H. Generated / accidental / should-not-be-in-PR | Copied Titan skill bundle | No | High | Removed from staged payload |
| `scripts/codex-*`, `scripts/lib/codex/**`, `scripts/tests/**` added files | H. Generated / accidental / should-not-be-in-PR | Copied Titan/Jira/BPMN automation | No | High | Removed from staged payload |
| `HomeBudget.Components.CurrencyRates/Clients/INationalBankApiClient.cs` | A. Rates API production code | Official NBRB `/exrates` routes and typed methods | Yes | Medium | Keep, review cancellation |
| `HomeBudget.Components.CurrencyRates/Configuration/DependencyRegistrations.cs` | A. Rates API production code | Registers currency resolver | Yes | Low | Keep |
| `HomeBudget.Components.CurrencyRates/Exceptions/NationalBankCurrencyResolutionException.cs` | A. Rates API production code | Resolver-specific exception | Yes | Low | Keep |
| `HomeBudget.Components.CurrencyRates/Extensions/CurrencyRateExtensions.cs` | A. Rates API production code | Enrich rates from catalog definition | Yes | Low | Keep |
| `HomeBudget.Components.CurrencyRates/Extensions/CurrencyRateGroupedExtensions.cs` | A. Rates API production code | Resolve exchange rates by abbreviation | Yes | Medium | Keep, verify BYN/name behavior |
| `HomeBudget.Components.CurrencyRates/Models/Api/NationalBankCurrency.cs` | A. Rates API production code | Adds NBRB metadata fields | Yes | Low | Keep |
| `HomeBudget.Components.CurrencyRates/Models/ConfigCurrency.cs` | A. Rates API production code | Adds optional periodicity | Yes | Low | Keep |
| `HomeBudget.Components.CurrencyRates/Models/Currency.cs` | A. Rates API production code | Adds abbreviation and periodicity | Yes | Low | Keep |
| `HomeBudget.Components.CurrencyRates/Models/NationalBankCurrencyDefinition.cs` | A. Rates API production code | Provider-normalized currency metadata | Yes | Low | Keep |
| `HomeBudget.Components.CurrencyRates/Providers/Interfaces/INationalBankRatesProvider.cs` | A. Rates API production code | Exposes active catalog definitions | Yes | Low | Keep |
| `HomeBudget.Components.CurrencyRates/Providers/NationalBankRatesProvider.cs` | A. Rates API production code | Catalog-based current rates and 365-day chunking | Yes | Medium | Keep, fix current-date/cancellation risk |
| `HomeBudget.Components.CurrencyRates/Services/CurrencyRatesService.cs` | A. Rates API production code | Uses active catalog for history enrichment | Yes | Low | Keep |
| `HomeBudget.Components.CurrencyRates/Services/Interfaces/INationalBankCurrencyResolver.cs` | A. Rates API production code | Resolver abstraction | Yes | Low | Keep |
| `HomeBudget.Components.CurrencyRates/Services/NationalBankCurrencyResolver.cs` | A. Rates API production code | Resolves configured currencies from NBRB catalog | Yes | Medium | Keep, fix catalog cancellation risk |
| `HomeBudget.Core/Extensions/DateTimeExtensions.cs` | A. Rates API production code | Adds max-day range splitting | Yes | Low | Keep |
| `HomeBudget.Rates.Api/Configuration/RefitHttpClientConfiguration.cs` | C. Rates API configuration | Normalizes legacy NBRB host and configures client | Yes | Medium | Keep, confirm deployment config |
| `HomeBudget.Rates.Api/Constants/LoggerTags.cs` | C. Rates API configuration | Logging tag constants | Yes | Low | Keep |
| `HomeBudget.Rates.Api/Extensions/Logs/CustomLoggerExtensions.cs` | C. Rates API configuration | OTLP resource attributes | Yes | Low | Keep |
| `HomeBudget.Rates.Api/Program.cs` | C. Rates API configuration | Passes service name to logger config | Yes | Low | Keep |
| `HomeBudget.Rates.Api/appsettings.json` | C. Rates API configuration | Default config | Yes | Blocker fixed | Cleared password and machine-specific URLs |
| `HomeBudget.Components.CurrencyRates.Tests/Extensions/CurrencyRateGroupedExtensionsTests.cs` | B. Rates API tests | Dynamic abbreviation exchange test | Yes | Low | Keep |
| `HomeBudget.Components.CurrencyRates.Tests/Providers/NationalBankRatesProviderTests.cs` | B. Rates API tests | 365-day chunking and dynamic current rate tests | Yes | Low | Keep |
| `HomeBudget.Components.CurrencyRates.Tests/Services/CurrencyRatesServiceTests.cs` | B. Rates API tests | Service test updates for resolver | Yes | Low | Keep |
| `HomeBudget.Components.CurrencyRates.Tests/Services/NationalBankCurrencyResolverTests.cs` | B. Rates API tests | Catalog resolver tests | Yes | Low | Keep |
| `HomeBudget.Components.IntegrationTests/MockServices/MockNationalBankRatesProvider.cs` | B. Rates API tests | Integration mock update | Yes | Low | Keep |
| `docs/rates-api-currency-catalog/00-current-state-audit.md` | F. Documentation | Current state evidence | Yes | Low | Keep |
| `docs/rates-api-currency-catalog/01-target-design.md` | F. Documentation | Target design | Yes | Low | Keep |
| `docs/rates-api-currency-catalog/02-persistence-decision.md` | F. Documentation | Persistence decision | Yes | Low | Keep |
| `docs/rates-api-currency-catalog/03-implementation-log.md` | F. Documentation | Implementation notes | Yes | Low | Keep |
| `docs/rates-api-currency-catalog/04-validation-report.md` | F. Documentation | Prior validation notes | Yes | Medium | Keep but supersede with current validation |

## Rates API refactoring review

### Current rates

Before: `NationalBankRatesProvider.GetTodayActiveRatesAsync` fetched the NBRB daily list with `periodicity=0` and filtered by configured abbreviations. This treated the daily list as the effective supported-currency universe.

After: configured currencies resolve against `/exrates/currencies`, then current rates are fetched by `Cur_ID` for the requested current date.

Risk: Medium. This supports non-daily catalog entries and preserves scale, but it uses `DateTime.Today` directly inside provider logic, making time-zone/current-date behavior implicit and hard to test.

Tests: Unit tests cover THB resolution and current-rate fetch by catalog ID.

Recommended action: Keep the design, but inject a clock or move the current-date decision to an application boundary before merge if feasible.

### Historical rates

Before: historical dynamics requests split by calendar year; leap-year chunks could exceed NBRB's 365-day maximum.

After: history requests split by `SplitByMaxDays(365)`.

Risk: Low. This aligns with the provider limit.

Tests: Unit test verifies chunks stay at or under 365 days.

Recommended action: Keep.

### Currency metadata

Before: currency metadata was limited by config and static `NationalBankCurrencies` usage in important flows.

After: `NationalBankCurrencyDefinition` normalizes catalog metadata including ID, parent ID, numeric code, abbreviation, scale, periodicity, and active dates.

Risk: Low. The model is appropriate, and catalog loading now propagates cancellation to the Refit method. Failed or canceled catalog loads are not permanently cached.

Tests: Resolver tests cover ID lookup, abbreviation lookup, duplicates, inactive currency, ambiguity, and ID/abbreviation mismatch.

Recommended action: Keep.

### Exchange lookup

Before: exchange conversion relied on static currency IDs from `NationalBankCurrencies`.

After: exchange lookup uses grouped rate abbreviation, enabling dynamically configured currencies when rates are present.

Risk: Medium. This is needed for dynamic currencies, but BYN handling now compares against `NationalBankCurrencies.Blr.Name`; verify all callers pass the expected BYN value.

Tests: Added dynamic THB abbreviation test.

Recommended action: Add or confirm BYN regression coverage with the actual caller-provided value.

## Codex configuration review

Files reviewed:

- `AGENTS.md`
- `docs/coding-standards.md`
- `docs/code-review-checklist.md`
- `docs/pr-review-check.md`
- `docs/pull_request_template.md`
- `.codex/skills/**`
- `scripts/**`

Copied-project leakage found:

- Titan / VC.Platform branding.
- Camunda, Zeebe, BPMN workflow rules.
- Jira overview automation.
- Service Bus, tenant, process-worker, and unrelated workflow guidance.
- Local Titan path under `/Users/.../Dev/Titan/...`.
- Script references to unrelated repository paths such as `VC.Platform/...`.

Actions taken:

- Rewrote `AGENTS.md` as a HomeBudget Rates API standards package.
- Rewrote the review and coding docs to focus on Rates API, NBRB behavior, currency metadata, precision, HTTP, caching, API contracts, tests, and observability.
- Replaced the copied PR template with a Rates API checklist.
- Removed staged-only copied `.codex/skills/**` and `scripts/**` files from the worktree.

## h-budget Rates API standards

Final standards now enforce:

- Mission and architecture boundaries for the Rates API.
- NBRB provider rules for `/exrates/currencies`, `/exrates/rates`, `parammode`, metadata fields, scale, periodicity, 404, empty responses, and dynamics range limits.
- Provider-independent currency model requirements.
- `DateOnly`, explicit date policy, and injectable clock guidance.
- `decimal` precision and scale-sensitive tests.
- Typed HTTP clients, options, cancellation, failure classification, and structured provider logs.
- Cache-key, TTL, invalidation, and failure-cache behavior.
- API contract behavior for unsupported currency, provider unavailable, no-rate-for-date, and range limits.
- Rates-specific PR review checklist.

## NBRB provider compatibility review

- Currencies: improved. Resolver uses `/exrates/currencies` and metadata fields instead of treating a limited daily rate list as complete.
- Rates: improved. Current rates fetch by `Cur_ID`; history uses dynamics by `Cur_ID`.
- `parammode`: documented in standards; current code does not yet use alphabetic `parammode=2` because it resolves abbreviations through catalog then queries by ID.
- Scale: improved. Catalog `Cur_Scale` is preserved and used for enrichment/rate-per-unit.
- Date ranges: improved for dynamics chunking. Active currency windows use `DateStart <= requestedDate < DateEnd`; confirm this matches NBRB semantics for end date.
- Errors: improved for current-rate 404 and null rates. Catalog request failures become `NationalBankCurrencyResolutionException`; timeout/malformed payload behavior depends on Refit/global middleware and should be validated.
- Empty responses: current individual rate null is skipped; list endpoint empty behavior is not central to the new current-rate flow.
- Dynamics limit: improved with 365-day chunks.

## Validation

Discovery completed:

- `dotnet sln list`: solution contains 9 projects.
- `Get-ChildItem -Recurse -Include *.sln,*.csproj`: confirmed `HomeBudgetRatesApi.sln` and all project files.
- `README.md`: not present.

Current validation results:

| Command | Result | Notes |
|---|---|---|
| `dotnet sln list` | Pass | 9 projects listed. |
| `Get-ChildItem -Recurse -Include *.sln,*.csproj` | Pass | Solution and project files discovered. |
| `dotnet test --list-tests` | Pass | Test discovery succeeded; restore/build warnings only. |
| `dotnet restore HomeBudgetRatesApi.sln` | Pass with warnings | Existing duplicate `PackageVersion`, package pruning, and vulnerable transitive package warnings. |
| `dotnet build HomeBudgetRatesApi.sln --no-restore` | Pass with warnings | 142 warnings after targeted formatting; warnings include existing analyzer/style issues plus changed-scope warnings for direct `LogWarning`, test helper parameter count, and existing broad catches. |
| `dotnet test HomeBudget.Components.CurrencyRates.Tests/HomeBudget.Components.CurrencyRates.Tests.csproj --no-build --verbosity minimal` | Pass | 23/23 tests passed. |
| `dotnet test HomeBudget.Rates.Api.Tests/HomeBudget.Rates.Api.Tests.csproj --no-build --verbosity minimal` | Pass | 1/1 test passed. |
| `dotnet test HomeBudgetRatesApi.sln --no-build --verbosity minimal` | Pass | 48/48 tests passed, including integration tests. |
| `dotnet format HomeBudgetRatesApi.sln --include <changed .cs files>` | Pass with warnings | Applied touched-file formatting where automatic fixes existed; no code-fix available for several analyzer warnings. |
| `dotnet format HomeBudgetRatesApi.sln --verify-no-changes` | Fail | Broad pre-existing import-order/whitespace/analyzer findings across unrelated files; stopped instead of applying repo-wide churn. |

## Security review

Secret/path findings:

- `HomeBudget.Rates.Api/appsettings.json`: staged local database connection string with password and local machine names. Fixed by restoring safe empty defaults.
- `HomeBudget.Rates.Api/appsettings.Integration.json`: contains a local integration-test SQL password. Existing test fixture pattern; not introduced by this cleanup.
- `startsonar.sh`: references `${SONAR_TOKEN}` only; no token value found.
- `HomeBudget.DataAccess.Dapper/SqlClients/SqlConnectionFactory.cs`: logs a connection string template without passing the value; confirm logging remains safe.
- Copied Codex scripts contained secret-scanner test fixtures and local path leakage; removed from the worktree.

## Remaining risks

- `master...HEAD` is empty while the reviewed changes are staged working-tree changes. Unknown: whether the PR branch was intended to be committed before audit.
- `dotnet format --verify-no-changes` fails repo-wide on existing formatting/analyzer issues; targeted formatting was applied only to changed C# files.
- Current-rate provider logic uses `DateTime.Today` directly; time-zone and testability policy should be made explicit.
- BYN exchange lookup should be regression-tested with the exact caller-provided value.
- Existing `.codex/skills/clean-code-review/SKILL.md` remains from `master` and is generic C#/.NET guidance; no copied-project leakage found in that file.

## Recommended next action

Fix or explicitly accept the current-date clock policy in `GetTodayActiveRatesAsync`.
