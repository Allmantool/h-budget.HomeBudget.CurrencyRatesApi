## Summary

Describe the user-visible or operational change and why it is needed.

## Rates API Checklist

- [ ] Provider-independent domain boundaries are preserved.
- [ ] NBRB metadata and scale semantics are handled correctly.
- [ ] The change does not hardcode a limited supported-currency catalog.
- [ ] Invalid currency, no-rate-for-date, and provider failure behavior are explicit.
- [ ] Cancellation tokens are propagated through async I/O.
- [ ] Rates and amounts use `decimal`.
- [ ] Tests cover changed behavior and important failure paths.
- [ ] API docs/OpenAPI are updated when public behavior changes.
- [ ] Configuration contains no secrets or local-machine values.
- [ ] Codex/agent guidance remains specific to `HomeBudget-Rates-Api`.

## Validation

List commands run and results, for example:

- `dotnet restore`
- `dotnet build --no-restore`
- `dotnet test --no-build`
- `dotnet format --verify-no-changes`

## Risk And Rollback

Call out API contract changes, provider behavior changes, migration/config changes, and rollback considerations.
