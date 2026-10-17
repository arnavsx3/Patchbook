# Patchbook

A personal notebook for programming errors, fixes, and lessons worth keeping.

Save the error, what caused it, and the solution so you can find your own answers next time.

## Features

- Create, view, edit, and delete entries.
- Keep error messages, explanations, code snippets, and reference links together.
- Search across saved content with highlighted matches and contextual previews.
- Combine tag and language filters; sort entries and pin useful fixes to the top.
- Read syntax-highlighted snippets with line numbers and clear language labels.
- Copy snippets while preserving code whitespace.
- Export JSON backups and restore missing entries without overwriting existing fixes.
- Use the notebook on desktop or mobile.
- Optionally load three example entries to explore the app.

## Stack

| Technology | Purpose |
| --- | --- |
| C# / .NET 10 | Application language and runtime |
| ASP.NET Core | Application hosting and dependency injection |
| Blazor Interactive Server | Interactive C# UI components |
| Tailwind CSS / PostCSS | Compiled styling |
| highlight.js / esbuild | Locally bundled snippet highlighting |
| In-memory entry store | Temporary server-side storage |
| xUnit | Store and validation tests |
| Playwright | Real browser and mobile layout checks |
| GitHub Actions | Automated verification |

**Database integration is deferred.** EF Core and SQLite will be added later.

## Run locally

Requires the .NET 10 SDK, Node.js 22 or newer, npm, and Make.
The SDK is pinned to `10.0.112` in `global.json` so local builds and CI use the same Blazor asset dependencies. Update the SDK pin and NuGet lockfiles together.

```bash
cd ~/Projects/Patchbook
make setup
make dev
```

Open [localhost:5080](http://localhost:5080). Run commands from the repository root.
`make dev` watches C# components, Tailwind styles, and browser helpers. Press Ctrl+C to stop the app and all watchers.
A C# change that restarts the server will also clear the in-memory notebook.
For a different development port, use `ASPNETCORE_URLS=http://localhost:5083 make dev`.

## Storage for this phase

Entries live in a C# collection on the server, shared by browser tabs connected to the same running app. Refreshing the browser keeps them; **restarting the application clears them**.

There is no database, browser localStorage, or automatic persistence yet. Export a backup before stopping the app to keep a copy of your fixes. The development server listens on localhost. This is a single-user local notebook with no accounts or separate user data.

## Backups

Use **Export backup** on the notebook page to download a versioned JSON file. **Import backup** restores entries whose IDs are missing and skips entries already in the notebook. Local edits are kept; importing the same backup again does not create duplicates. Pins, timestamps, and code whitespace are included.

Backups support up to 1,000 entries and 2 MB. The entire file is validated before any entries are added.

## Commands

| Command | What it does |
| --- | --- |
| `make setup` | Install dependencies and generate browser assets |
| `make dev` | Start the app and asset watchers |
| `make build` | Compile a Release build, including browser assets |
| `make test` | Run xUnit tests |
| `make check` | Verify formatting, build, run unit tests, and audit npm packages |
| `make e2e` | Publish and test the app in a browser on port 5081 |
| `make publish` | Create a runnable app in `artifacts/publish` |

Before your first browser test run:

```bash
npx playwright install chromium
make e2e
```

To run the published app without the development watchers:

```bash
make publish
cd artifacts/publish
dotnet Patchbook.dll --urls http://localhost:5080
```

## Repository

```text
src/Patchbook/
  Program.cs           Application startup and service registration
  Entries/             Models, validation, search, backups, and in-memory storage
  Components/Pages/    Razor pages paired with C# .razor.cs files
  Components/Shared/   Reusable UI components
  Client/              Browser helpers and local highlighting imports
  Styles/              Tailwind entry point and theme
  wwwroot/             Static assets
tests/
  Patchbook.Tests/     xUnit tests
  browser/             Playwright tests
scripts/              Development and browser asset build helpers
docs/                 Architecture and learning notes
```

See [the architecture notes](docs/architecture.md) for the entry flow, storage boundary, and a suggested reading order.
