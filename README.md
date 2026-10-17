# Patchbook

A personal notebook for programming errors, fixes, and lessons worth keeping.

Save the error, what caused it, and the solution so you can find your own answers next time.

## Features

- Create, view, edit, and delete entries.
- Keep error messages, explanations, code snippets, and reference links together.
- Search across saved content, filter by tag, and sort entries.
- Copy snippets while preserving code whitespace.
- Use the notebook on desktop or mobile.
- Optionally load three example entries to explore the app.

## Stack

| Technology | Purpose |
| --- | --- |
| C# / .NET 10 | Application language and runtime |
| ASP.NET Core | Application hosting and dependency injection |
| Blazor Interactive Server | Interactive C# UI components |
| Tailwind CSS / PostCSS | Compiled styling |
| In-memory entry store | Temporary server-side storage |
| xUnit | Store and validation tests |
| Playwright | Real browser and mobile layout checks |
| GitHub Actions | Automated verification |

**Database integration is deferred.** EF Core and SQLite will be added later.

## Run locally

Requires the .NET 10 SDK, Node.js 22 or newer, npm, and Make.

```bash
cd ~/Projects/Patchbook
make setup
make dev
```

Open [localhost:5080](http://localhost:5080). Run commands from the repository root.
`make dev` watches C# components and Tailwind styles. Press Ctrl+C to stop both processes.
A C# change that restarts the server will also clear the in-memory notebook.

## Storage for this phase

Entries live in a C# collection on the server, shared by browser tabs connected to the same running app. Refreshing the browser keeps them; **restarting the application clears them**.

There is no database, browser localStorage, or file persistence yet. The development server listens on localhost. This is a single-user local notebook with no accounts or separate user data.

## Commands

| Command | What it does |
| --- | --- |
| `make setup` | Install project dependencies and generate styles |
| `make dev` | Start the app and style watcher |
| `make build` | Compile a Release build, including styles |
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
  Entries/             Models, validation, storage interface and implementation
  Components/Pages/    Notebook, editor, and entry details
  Components/Shared/   Reusable UI components
  Styles/              Tailwind entry point and theme
  wwwroot/             Static assets
tests/
  Patchbook.Tests/     xUnit tests
  browser/             Playwright tests
scripts/              Development and CSS build helpers
docs/                 Architecture and learning notes
```

See [the architecture notes](docs/architecture.md) for the entry flow, storage boundary, and a suggested reading order.
