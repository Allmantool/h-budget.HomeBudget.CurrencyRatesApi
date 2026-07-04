# HomeBudget Rates API PR Review Check

Use this rubric for Codex-assisted PR reviews in `HomeBudget-Rates-Api`.

## Required Review Flow

1. Inspect `git status --short`, branch, changed files, and the intended diff range.
2. Read `AGENTS.md`, `docs/coding-standards.md`, and this file.
3. Classify changed files as production code, tests, configuration, Codex standards, documentation, CI/CD, generated/accidental, or unknown.
4. Review Rates API behavior, NBRB provider compatibility, API contracts, test coverage, configuration safety, and copied-project leakage.
5. Run or record applicable validation commands.
6. Return a GO, PARTIAL, or NO-GO recommendation.

## Severity

- `blocker`: known correctness, security, secret leakage, provider-contract, data-loss, or validation failure that must be fixed before merge.
- `major`: meaningful production, maintainability, test, or contract risk that should be fixed before merge.
- `minor`: limited-risk issue that can be fixed before or shortly after merge.
- `informational`: context, uncertainty, or follow-up note.

## Rates API Review Checklist

- Does this change preserve provider-independent domain boundaries?
- Does it avoid hardcoded limited supported currencies?
- Does it preserve NBRB scale semantics?
- Does it model currency metadata needed for ID, parent ID, numeric code, abbreviation, scale, periodicity, and active dates?
- Does it handle invalid currency, empty rate, and provider failure distinctly?
- Does it propagate cancellation tokens through provider calls?
- Does it use `decimal` for rates?
- Does it split NBRB dynamics ranges beyond 365 days?
- Does it include tests for changed behavior?
- Does it update docs/OpenAPI where API behavior changed?
- Does it avoid copied-project instructions and machine-specific paths?

## Required Output

```md
# PR Review

## Summary
GO / PARTIAL / NO-GO with rationale.

## Diff Scope
Changed files grouped by category.

## Findings
| Severity | Area | Evidence | Impact | Required Fix |
|---|---|---|---|---|

## Rates API Refactoring Review
Before/after behavior, risks, tests, and recommended actions.

## Codex Configuration Review
Copied-project leakage, actions taken, and remaining risks.

## Validation
Commands and results.

## Security Review
Secret/path findings without printing secret values.

## Recommended Next Action
One concrete action.
```
