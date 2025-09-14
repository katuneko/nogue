# Repository Guidelines

## Project Structure & Module Organization
- `game/core`, `game/gameplay`: C# gameplay logic (Unity‑ready).
- `game/tests`: NUnit tests for core/gameplay.
- `content/`: Authoritative YAML sources; generated JSON lives alongside.
- `tools/`: Python utilities — validator and content exporters.
- `docs/`: Design and architecture notes.
- `.github/workflows/`: CI for content validation and (optionally) Unity tests.

## Build, Test, and Development Commands
- Python setup: `python -m venv .venv && source .venv/bin/activate && pip install -r tools/DataValidator/requirements.txt`
- Merge event fragments → `content/events.yaml`/`.json`: `python tools/ContentTools/merge_events.py`
- Export tiers/devices/crops/contracts JSON: `python tools/ContentTools/export_tiers_json.py` and `python tools/ContentTools/export_content_json.py`
- Validate content/schema/cross‑refs: `python tools/DataValidator/validate.py`
- Sync content into Unity project: `CONTENT_ROOT=content python tools/ContentTools/sync_content.py`
- Unity tests (when `game/ProjectSettings` exists): run via Unity Test Runner; CI job `unity-tests.yml` executes on PRs.

## Coding Style & Naming Conventions
- General: LF line endings, final newline, trim trailing whitespace (`.editorconfig`).
- Indentation: C# 4 spaces; YAML/JSON 2 spaces.
- C#: place `System` usings first; prefer braces for multiline blocks. Types/methods `PascalCase`, locals/fields `camelCase`. One public type per file; match filename.
- Content IDs: snake_case ASCII and immutable. Register in `content/ids.yaml`. Tiers in `content/tiers/T#.yaml`. Event fragments in `content/events/{common,T1..T6}.yaml`; `content/events.yaml` is generated — do not edit.

## Testing Guidelines
- Framework: NUnit under `game/tests` (suffix `*Tests.cs`). Keep tests deterministic (seed RNG). Cover boundary cases in `core` math and director/budget logic. For content changes, run validator and ensure new IDs are registered.

## Commit & Pull Request Guidelines
- Commits: concise, imperative (e.g., `content: add T3 wind tweaks`).
- PRs: include summary, linked issues, and rationale; ensure `Validate Content` CI passes; attach screenshots/logs for gameplay/UX changes. Follow `.github/PULL_REQUEST_TEMPLATE.md`. Reviews are requested via `CODEOWNERS`.

## Security & Configuration Tips
- Never commit secrets. Avoid editing generated JSON; regenerate via tools. Use `CONTENT_ROOT` to point Unity to alternate content when needed.

