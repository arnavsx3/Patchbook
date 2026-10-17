# Patchbook

A personal notebook for programming errors, fixes, and lessons worth keeping.

Save the error, what caused it, and the solution so you can find your own answers next time.

## Features

- Add, edit, and delete entries with explanations and code snippets.
- Organize entries with tags and reference links.
- Search your notebook and filter by tag.
- Copy saved code snippets.

## Stack

- C# and ASP.NET Core on .NET 10
- Blazor for the interactive UI
- Tailwind CSS for styling
- Server-side in-memory storage (EF Core and SQLite are deferred)
- xUnit for tests
- Playwright for browser checks

## Run locally

Requires the .NET 10 SDK, Node.js 22 or newer, and npm.

```bash
make setup
make dev
```

Run these commands from the repository root, then open http://localhost:5080.
`make build` builds the application and its Tailwind styles.

## Status

The local notebook supports creating, reading, editing, and deleting entries, search, tag filters, sorting, and copying snippets. You can optionally load three example entries from the empty notebook.

Entries are shared by browser tabs connected to the same app and clear when the application restarts. There is no browser localStorage or database yet.
