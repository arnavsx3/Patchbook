# Architecture

Patchbook is a single ASP.NET Core application with Blazor Interactive Server components. C# handles both application logic and UI events; the browser receives rendered updates over Blazor's built-in connection.

Routes use Interactive Server without prerendering: the interactive notebook appears once the connection is ready. No separate HTTP API is needed for entry operations; the components call the store service directly.

## Entry flow

1. A Razor page collects an `EntryDraft`.
2. Data annotations validate the form. The store validates again at its boundary.
3. The page calls `IEntryStore` through dependency injection.
4. `InMemoryEntryStore` saves an immutable `Entry` in a dictionary.
5. The page renders the saved entry or a filtered list.

The store is registered as a singleton, so browser tabs share the same local notebook. A lock protects the dictionary. Reads return immutable entry snapshots rather than editable references into storage.

The store interface returns tasks to allow a future database implementation to perform asynchronous I/O. The current implementation completes those tasks immediately because dictionary operations do not require I/O.

## Storage boundary

There is no database, browser localStorage, or automatic file persistence. Closing the browser does not clear the running server's entries; stopping or restarting the application does. Development reloads that restart the server also clear entries.

EF Core and SQLite are deliberately deferred. A later implementation can replace `InMemoryEntryStore` behind `IEntryStore`. Queries and validation tests can then be reused to check equivalent behaviour.

## Backups and pins

`NotebookBackupService` serializes a versioned document containing entry IDs, content, creation and edit timestamps, and pin state. Imports are limited to 2 MB and 1,000 entries. The store validates and normalizes the whole batch before acquiring its write lock; valid missing IDs are then added together. Existing IDs are skipped so importing a stale backup cannot overwrite a newer local fix.

Pinning is a separate store operation. It preserves edit timestamps and content, and normal edits preserve pin state. All list sort modes put pins first while keeping the requested order within each group.

## Search and presentation

`EntrySearch` shares the fields used by filtering and contextual match previews. Language, tag, and literal case-insensitive search filters combine. `TextMatches` splits previews into plain text segments; `HighlightText` renders matching segments as encoded Razor content inside `mark` elements.

Larger components use paired `.razor` and `.razor.cs` files. Markup describes presentation; code-behind handles events and page state. Storage, validation, search rules, and backup rules stay in services and model helpers.

Snippet colouring uses locally bundled highlight.js grammars. C# asks the browser helper for escaped highlighting HTML after rendering. Plain text is HTML-encoded before display, and browser tests also check that HTML snippets stay inert. Line numbers are a separate non-selectable gutter; copying always uses the original saved code.

## Local use

The development profile listens on localhost. There are no accounts or separate user notebooks. This is a personal local application; all connections to the same running server share its entries.

## Build

The web project's build target generates Tailwind CSS through PostCSS and bundles browser helpers with esbuild before static assets are collected. The published server runs without Node.js; highlight.js is included in the browser bundle. Generated CSS and JavaScript, build outputs, and dependencies are ignored by Git; source files and lockfiles are committed.

Browser tests run against the published application in Production mode, on their own port and with a fresh in-memory store. They check the real UI and packaged assets together.

## Suggested reading order

Start with `Program.cs` for dependency injection and app startup. Read `Entry.cs` and `EntryDraft.cs` for records and validation, then `IEntryStore.cs` and `InMemoryEntryStore.cs` for the storage contract and LINQ queries. Read the store tests alongside that implementation. Then read `NotebookBackupService.cs` and `EntrySearch.cs` alongside their tests. Finish with the paired Razor and C# files in `Components/Pages`, followed by the shared components they use.
