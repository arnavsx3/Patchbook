# Patchbook

A personal notebook for programming errors, fixes, and lessons worth keeping.

Save the error, what caused it, and the solution so you can find your own answers next time.

## Planned features

- Add, edit, and delete entries with explanations and code snippets.
- Organize entries with tags and reference links.
- Search your notebook and filter by tag.
- Copy saved code snippets.

## Planned stack

- C# and ASP.NET Core on .NET 10
- Blazor for the interactive UI
- Tailwind CSS for styling
- Entity Framework Core and SQLite for storage
- xUnit for tests

## Run locally

Requires the .NET 10 SDK, Node.js 22 or newer, and npm.

```bash
make setup
make dev
```

Run these commands from the repository root, then open http://localhost:5080.
`make build` builds the application and its Tailwind styles.

## Status

The Blazor application and Tailwind build pipeline are in place. Notebook features are being built next, using server-side in-memory storage. Database integration is deferred.
