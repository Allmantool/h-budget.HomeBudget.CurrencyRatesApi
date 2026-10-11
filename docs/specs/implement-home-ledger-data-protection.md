# Rates Data Protection Commit Watermark

Status: Implemented and locally verified (OpenSpec task 1.6), with task 1.7
schema/deployment documentation present. Production data-protection activation
remains blocked until the additive migration and later capability prerequisites
are verified. Exact historical decimal recovery remains blocked while the
authoritative columns use SQL `FLOAT`; acknowledging that limitation does not
turn it into a passing fidelity guarantee.

## Scope and ownership

This repository owns the authoritative `HomeBudget.CurrencyRates` SQL rate and
business-settings mutations. This slice adds a durable ordering receipt to each
runtime mutation and an internal fenced capture-frontier contract that a later
Accounting-owned recovery coordinator can call. It does not expose a public HTTP
route, implement backup/PITR, change authentication, or edit the workspace root
SQL migration sequence.

Authoritative change: `implement-home-ledger-data-protection`, task 1.6. The
cross-repository design decision is D3, "Use an explicit maintenance epoch and
component receipts for a common cutoff."

## Current evidence

- `CurrencyRatesWriteProvider.UpsertRatesWithSaveAsync` owns runtime rate
  upserts. Before task 1.6 it used a SQL `MERGE` inside `TransactionScope`; it
  now commits the `MERGE` and its receipt in one `XACT_ABORT` transaction.
- `ConfigSettingsProvider.SaveDefaultSettingsAsync` owns the `General` settings
  update. It now commits the `UPDATE` and its receipt in that same transactional
  pattern.
- Both providers target `HomeBudget.CurrencyRates` through
  `IBaseWriteRepository`.
- Integration migrations are repository-owned under
  `HomeBudget.Components.IntegrationTests/db/migrations`. Production migrations
  are workspace-owned under `Scripts/ms-sql` and are outside this task's owned
  paths.
- The deployed/test schema defines `Scale`, `OfficialRate`, and `RatePerUnit` as
  SQL `FLOAT`. The application models use `decimal`; therefore the current SQL
  schema cannot guarantee byte-for-byte or scale-preserving decimal round trips.

## Requirements

### RATES-DP-01 Atomic mutation receipt

Every successful runtime rate or settings mutation SHALL insert exactly one
durable receipt in the same local SQL transaction as the authoritative change.
The receipt SHALL contain a monotonic database-generated sequence, unique receipt
ID, mutation kind, UTC commit timestamp, and affected-row count. A rolled-back or
cancelled-before-commit mutation SHALL leave neither the business change nor a
receipt.

### RATES-DP-02 Ordered capture frontier

The component SHALL expose an internal cancellation-aware capture-frontier
contract. Writers and frontier acquisition SHALL serialize on one SQL
transaction-owned application lock. A returned fence SHALL identify the durable
fence receipt and watermark sequence. Mutations committed before the fence SHALL
have lower sequences; mutations committed after it SHALL have higher sequences.

The returned frontier is an application commit-order marker for coordinated
capture. Its transaction-owned lock is released when the frontier transaction
commits; it is not a held maintenance lease. Later cross-service barrier work
must close mutation admission while capture consumes this frontier. The frontier
is not a SQL Server backup, log-chain marker, or claim of PITR coverage.

### RATES-DP-03 Restart durability

Receipts and frontiers SHALL be persisted in `HomeBudget.CurrencyRates`, not process
memory. A new service scope/process using the same database SHALL observe the
same or a later watermark.

### RATES-DP-04 Compatibility

Existing public HTTP routes, request/response JSON, affected-row results, rate
calculation, and settings semantics SHALL remain unchanged. No external provider
call is required to acquire a fence.

### RATES-DP-05 Decimal truthfulness

New snapshot/watermark JSON values that contain decimals SHALL use invariant
decimal strings. This slice SHALL explicitly report the current SQL `FLOAT`
storage limitation and SHALL NOT claim that values already persisted through
that schema retain the original decimal scale or exact base-10 value. Converting
the authoritative columns to `DECIMAL(p,s)` requires a separate data profiling,
precision, compatibility, and migration decision.

## Design

Add `dbo.RatesCommitReceipts` with an identity `CommitSequence` primary key and
unique `ReceiptId`. Allowed kinds are `rates`, `settings`, and `fence`. Each
provider executes one `XACT_ABORT` SQL batch that:

1. begins a local transaction;
2. acquires an exclusive transaction-owned application lock on the stable Rates
   commit-fence resource;
3. performs the authoritative mutation;
4. inserts and captures the receipt;
5. commits; and
6. returns the original affected-row result plus receipt metadata.

The internal frontier provider uses the same lock and transaction but inserts
only a `fence` receipt. The database identity is the ordering source; wall-clock
time is descriptive only. The name deliberately describes a commit frontier,
not a lock held across a later physical capture.

The shared Dapper write adapter gains bounded query-single overloads that accept
a cancellation token and, for rates, a table-valued parameter plus scalar
parameters. No new package is required.

## Acceptance cases

1. A rate POST changes the rate and creates a `rates` receipt atomically while
   preserving the existing response payload.
2. A settings POST changes `General` settings and creates a `settings` receipt
   atomically while preserving the existing response payload.
3. Concurrent writes around a frontier are strictly ordered by `CommitSequence`.
4. A new service scope observes a previously committed frontier/watermark.
5. Pre-cancelled frontier acquisition throws cancellation and creates no receipt.
6. Receipt/fence JSON uses documented invariant strings, and tests record that
   the legacy `FLOAT` columns cannot guarantee exact decimal/scale round trips.
7. Migration reapplication is safe and preserves existing rate/settings rows.

## Definition of ready

- [x] Authoritative requirements and owning task identified.
- [x] Existing write paths, data-access boundary, migrations, and tests inspected.
- [x] Public compatibility and cancellation requirements stated.
- [x] SQL ordering/atomicity mechanism selected without new dependencies.
- [x] Decimal persistence limitation stated without weakening the requirement.
- [x] Root production migration ownership called out as an explicit handoff.
- [x] RED integration/unit cases identified before production changes.

## Verification evidence

- RED: before adding the integration migration,
  `dotnet test HomeBudget.Components.IntegrationTests/HomeBudget.Components.IntegrationTests.csproj --filter FullyQualifiedName~RatesCommitReceiptIntegrationTests --no-restore`
  failed both receipt tests with SQL error 208 because
  `dbo.RatesCommitReceipts` did not exist.
- GREEN: the same focused command passes all eight tests. Coverage includes
  rate and settings receipts, concurrent write/frontier ordering, restart
  durability, pre-cancellation, explicit decimal serialization/storage
  metadata, and atomic rollback of both authoritative mutation types when a
  receipt insert fails.
- `dotnet test HomeBudget.Components.CurrencyRates.Tests/HomeBudget.Components.CurrencyRates.Tests.csproj --no-restore`
  passes all 31 component tests, including cancellation-token forwarding.
- `dotnet build HomeBudgetRatesApi.sln --no-restore --verbosity:minimal`
  succeeds with no errors.
- `dotnet test HomeBudgetRatesApi.sln --no-build --no-restore` passes all 31
  component tests and all 34 integration tests. One of four Rates API tests
  remains blocked by the repository's existing Swashbuckle/OpenAPI binary
  mismatch (`MissingMethodException` for
  `IOpenApiRequestBody.get_Content()`); this change does not modify packages.
- Scoped `dotnet format whitespace ... --verify-no-changes --no-restore
  --include <changed C# files>` succeeds. A whole-solution format/analyzer
  verification still reports pre-existing findings outside this task's touched
  files.
- `eng/verify-fast.ps1 -Area Rates` passes its architecture and migration
  sequence/parity checks, then stops before Rates checks because the protected
  harness-evidence case reports a hash mismatch for `hidden-tests.ps1`. This
  repository task did not modify that protected file or its manifest.
- The integration and root production migrations are byte-identical. Both
  `V16__Add_rates_commit_receipts.sql` and
  `Scripts/ms-sql/V23__Add_rates_commit_receipts.sql` have SHA-256
  `67C41F7EDAACA63116EA992BCAE36EC467EA62A2033B6EEFD799C697D2E9AC91`.

## Schema compatibility and deployment order

The workspace migrations are the production sequence. Repository-local
integration migrations are byte-identical test twins and must not be applied as
an alternative production chain.

| Production migration | Integration twin | Dependency and compatibility purpose |
|---|---|---|
| Root `V21__Add_data_protection_control_plane.sql` | Accounting `V9__Add_data_protection_control_plane.sql` | Creates the protected operation/control schema used by the coordinator. Rates does not query it directly. |
| Root `V22__Add_ledger_generation_scope.sql` | Accounting `V10__Add_ledger_generation_scope.sql` | Establishes generation-zero compatibility and Accounting's ledger-generation fence. Rates remain shared authoritative dependencies rather than ledger-scoped rows. |
| Root `V23__Add_rates_commit_receipts.sql` | Rates `V16__Add_rates_commit_receipts.sql` | Adds `dbo.RatesCommitReceipts` in `HomeBudget.CurrencyRates`; this is the direct prerequisite for every receipt-bearing Rates mutation and capture frontier. |

Deploy this foundation through the normal guarded process in this order:

1. Apply root V21, V22, and V23 in sequence and verify their schema objects and repository-local migration parity. Do not replace the Rates process before V23 is present.
2. Deploy the matching generation-aware Accounting OperationsConsumer worker before the Accounting API while the `default`/`legacy` generation is active, then deploy the Accounting API from the same compatible candidate.
3. Deploy the Rates API after V23. Its public routes and response shapes remain compatible, but its mutation implementation now requires the receipt table.
4. Deploy later Gateway and SPA administration support only after their authorization and capability gates pass. Do not enable destructive or recovery workflows from this foundation alone.

If V23 is absent, the receipt insert fails inside the same transaction and the
rate/settings mutation rolls back; the service must not silently write an
unreceipted change. A generic process health check does not currently prove the
receipt schema, exact-decimal recovery, or cross-service data-protection
prerequisites. Until a guarded migration probe and the later authorization,
storage, key, provider, reconciliation, and restore-drill checks pass, readiness
for coordinated protection is BLOCKED even if ordinary read health is green.

Rollback reverts compatible binaries/configuration without dropping the
additive receipt table or the Accounting control/generation schema. Rolling back
after accepting unreceipted writes or activating a later ledger generation is
not a safe binary-only rollback and requires the later recovery/reconciliation
procedure.

The workspace-root SQL owner has added
`Scripts/ms-sql/V23__Add_rates_commit_receipts.sql`; its checked content is the
exact production twin of
`HomeBudget.Components.IntegrationTests/db/migrations/V16__Add_rates_commit_receipts.sql`.
This repository does not apply either migration to any environment.

## Task 5.2 cross-service maintenance admission

The coordinated-capture handoff adds one production constraint to the existing commit-frontier design: every authoritative rate or settings mutation must be wholly before or wholly after an Accounting-owned maintenance epoch. `BeginMutationTransaction` therefore acquires the existing exclusive Rates commit lock, then a transaction-owned shared `HomeBudget.DataProtection.Admission.Global.v1` lock, and checks the durable `[HomeBudget.DataProtection].dbo.MaintenanceEpochs` state before changing either table. The authoritative mutation and its receipt remain in that same transaction. A lock failure, missing control-plane schema, or closed epoch rolls back and fails closed; SQL errors 51030/51031 map to HTTP 503 without changing the public success contract.

Accounting closes admission by taking the same global lock exclusively before it records the epoch. This serializes concurrent Rates writes without a process-local flag and lets all replicas observe the durable barrier. `IRatesCaptureFrontierProvider` deliberately continues to use only the Rates commit lock: after the epoch closes, it can create the ordered `fence` receipt needed by coordinated capture while user/scheduled rate and settings mutations remain rejected.

Final local evidence:

- `dotnet build HomeBudget.Rates.Api/HomeBudget.Rates.Api.csproj --configuration Release --no-restore --no-incremental`: PASS, zero errors.
- `dotnet test HomeBudget.Components.CurrencyRates.Tests/HomeBudget.Components.CurrencyRates.Tests.csproj --configuration Release --no-restore`: PASS, 31/31.
- `dotnet test HomeBudget.Components.IntegrationTests/HomeBudget.Components.IntegrationTests.csproj --configuration Release --no-restore --filter FullyQualifiedName~RatesCommitReceiptIntegrationTests`: PASS, 10/10 against the disposable SQL Testcontainer. The new cases prove a closed epoch rejects both mutation types without new data or receipts, frontier acquisition remains available, and exact release resumes mutation.

Production activation is still BLOCKED until the Accounting control-plane migrations are applied through the guarded deployment process and all later provider/storage/key/restore-drill prerequisites pass. No environment was accessed or changed by this work.
