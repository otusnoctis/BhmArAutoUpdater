# NexusLocal Template Base

`NexusLocal` is a Windows desktop utility base built with `.NET MAUI` + `BlazorWebView` and distributed through `Velopack` + `GitHub Releases`.

This repository is intended to evolve into a reusable template for small local tools:
- stable Windows desktop app
- sidebar dashboard frame
- self-update through GitHub Releases
- simple local data persistence
- minimal infrastructure with no backend dependency

## Purpose

This project is not meant to be a complex business application yet. It is a starting architecture for building other utility apps on top of the same foundation.

The important idea is:
- the app is a Windows-only desktop app
- the UI is MAUI Blazor Hybrid
- updates are handled by Velopack
- distribution is automated from GitHub tags
- local app data is stored in a `data` folder near the repo/app environment logic

## Current Stack

- `.NET 10`
- `.NET MAUI`
- `BlazorWebView`
- `Bootstrap`
- `Velopack`
- `GitHub Actions`
- `GitHub Releases`

## High-Level Architecture

The app has 4 main layers:

1. `App Frame`
   - Main layout
   - Sidebar navigation
   - Shared visual structure

2. `Pages`
   - `Home`: simple dashboard landing page
   - `Quote`: example writable feature with local persistence
   - `Settings`: version/update/status page

3. `Services`
   - environment detection
   - Velopack update orchestration
   - local JSON persistence
   - startup/update state handling

4. `Release Infrastructure`
   - local packaging script
   - GitHub Actions workflow
   - Velopack-generated release artifacts

## Repository Structure

```text
.
|-- .github/workflows/release.yml
|-- Directory.Build.props
|-- scripts/pack-nexuslocal.ps1
|-- NexusLocal/
|   |-- NexusLocal.csproj
|   |-- Components/
|   |   |-- Layout/
|   |   |-- Pages/
|   |-- Services/
|   |-- Resources/
|   |-- Platforms/
|   |-- wwwroot/
|-- data/
|   |-- quote.json
```

## Runtime Model

At runtime the app behaves like this:

1. The user installs the app through Velopack.
2. The app runs as a normal Windows desktop application.
3. On startup, the app checks GitHub Releases for updates.
4. If a newer version exists, the app exposes that state in the UI.
5. The user can trigger download/update from `Settings`.
6. Velopack downloads the new package, applies it, and restarts the app.

`dev mode` is intentionally treated differently:
- no real update flow is executed
- version is shown as `x.x.x-dev`
- update controls remain effectively disabled

## Update Architecture

The update feed is GitHub Releases.

The app uses:
- `VelopackUpdateService` for update checks and apply/restart flow
- `GithubSource(...)` as the update source
- assembly metadata to read the repository URL

Important design decisions:
- repository URL is centralized in `Directory.Build.props`
- Velopack version is centralized in `Directory.Build.props`
- portable package generation is disabled
- update checks happen automatically at startup
- the `Settings` menu item shows a `?` badge if an update is pending

## Release Architecture

Releases are tag-driven.

Flow:

1. Push a tag like `v0.3.1`
2. GitHub Actions runs on `windows-latest`
3. `.NET 10` is installed
4. `MAUI` workload is installed
5. `scripts/pack-nexuslocal.ps1` publishes and packs the app
6. Velopack artifacts are uploaded to the GitHub Release

Main files:
- [release.yml](./.github/workflows/release.yml)
- [pack-nexuslocal.ps1](./scripts/pack-nexuslocal.ps1)
- [Directory.Build.props](./Directory.Build.props)

## Local Data

The app currently persists feature data as JSON.

Example:
- `data/quote.json`

This is intentional. The template assumes:
- local-first tools
- simple file persistence
- no database unless a future derived app needs one

## UI Principles

The UI is intentionally simple and template-friendly:
- left sidebar navigation
- Bootstrap-based content pages
- minimal visual complexity
- no dependency on a remote API
- diagnostic/update concerns moved out of `Home` and into `Settings`

This makes it easier to repurpose the project into other utility apps without rewriting the main app frame.

## Rebuild Spec

If this app had to be recreated from this document alone, the important constraints are:

1. Build a Windows-only `.NET MAUI` app using `BlazorWebView`.
2. Use a sidebar layout with at least `Home`, `Quote`, and `Settings`.
3. Keep `Home` minimal.
4. Use `Quote` as the example of local JSON persistence.
5. Put version/update logic only in `Settings`.
6. Integrate `Velopack` for install/update/restart flow.
7. Use GitHub Releases as the update source.
8. Trigger releases from semantic version tags `v*.*.*`.
9. Centralize shared build settings in `Directory.Build.props`.
10. Keep the project suitable as a base template for future local utilities.

## What To Replace In A Derived App

When using this repo as a template for another tool, the main things to replace are:
- app name
- application id
- icon/splash assets
- GitHub repository URL
- sidebar sections
- feature pages
- local JSON schema/files

The parts that should usually stay are:
- MAUI + Blazor app frame
- Velopack integration
- GitHub Actions release flow
- central shared properties in `Directory.Build.props`
- service-oriented page/runtime structure

## Status

This repository is a template base in progress, not a finished product.

What already matters is stable:
- runtime stack
- update model
- release model
- app frame direction

What is expected to change:
- naming
- pages/features
- branding
- final product-specific UX
