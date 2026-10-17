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

There is no database, browser localStorage, or file persistence. Closing the browser does not clear the running server's entries; stopping or restarting the application does. Development reloads that restart the server also clear entries.

EF Core and SQLite are deliberately deferred. A later implementation can replace `InMemoryEntryStore` behind `IEntryStore`. Queries and validation tests can then be reused to check equivalent behaviour.

## Local use

The development profile listens on localhost. There are no accounts or separate user notebooks. This is a personal local application; all connections to the same running server share its entries.

## Build

The web project's build target generates Tailwind CSS through PostCSS before static assets are collected. Node packages are development tools, not part of the running .NET application. Generated CSS, build outputs, and dependencies are ignored by Git; lockfiles are committed.

Browser tests run against the published application in Production mode, on their own port and with a fresh in-memory store. They check the real UI and packaged assets together.

## Suggested reading order

Start with `Program.cs` for dependency injection and app startup. Read `Entry.cs` and `EntryDraft.cs` for records and validation, then `IEntryStore.cs` and `InMemoryEntryStore.cs` for the storage contract and LINQ queries. Read the store tests alongside that implementation. Finish with the three pages in `Components/Pages` and the shared components they use.
