# Contributing

Keep changes focused and reviewable. Use a short conventional commit message that explains the change, for example `feat: add tag filtering`, `fix: preserve snippet whitespace`, or `docs: explain the storage boundary`.

Run `make check` before committing. Run `make e2e` when a change affects page behaviour, forms, navigation, or assets. Fix failing checks before pushing each completed chunk.

Commit dependency lockfiles. Do not commit generated CSS, build outputs, browser traces, local database files, or credentials.

Database integration is deliberately deferred. Keep pages dependent on `IEntryStore`, and preserve validation at the store boundary when adding persistent storage later.
