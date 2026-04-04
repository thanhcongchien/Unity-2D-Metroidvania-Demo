# Repository Guidelines

## Project Structure & Module Organization
This project targets Unity `2022.3.62f3c1`. Runtime code lives in `Assets/Script`, organized by domain: `Player`, `Enemy`, `Entity`, `SkillSystem`, `Data`, `Enum`, and `Interface`. Scenes are in `Assets/Scenes`; reusable content is in `Assets/Prefab`; art content is split across `Assets/Animation`, `Assets/Graphics`, `Assets/Materials`, and `Assets/URP`. Package dependencies are defined in `Packages/manifest.json`; engine settings live in `ProjectSettings`. Treat `Library`, `Temp`, `Logs`, and `obj` as generated folders, not source.

## Build, Test, and Development Commands
Open the project root `F:\unity\RPG` from Unity Hub. For batch validation, run:

```powershell
& "F:\unity\2022.3.62f3c1\Editor\Unity.exe" -projectPath "F:\unity\RPG" -quit -batchmode -logFile Logs/compile.log
& "F:\unity\2022.3.62f3c1\Editor\Unity.exe" -projectPath "F:\unity\RPG" -runTests -testPlatform EditMode -testResults Logs/EditMode.xml -quit -batchmode
& "F:\unity\2022.3.62f3c1\Editor\Unity.exe" -projectPath "F:\unity\RPG" -runTests -testPlatform PlayMode -testResults Logs/PlayMode.xml -quit -batchmode
```

No custom build pipeline was found. Use `File > Build Settings` for local builds until an `Assets/Editor/Build` script is added.

## Coding Style & Naming Conventions
Use 4-space indentation, braces on their own lines, and one class per file. Keep class, method, enum, and state type names in `PascalCase` (for example, `PlayerDashState`, `AttackData`); use `camelCase` for local variables and non-constant fields. Match the surrounding style when editing older files to avoid noisy diffs. Keep folder names feature-based, and always commit Unity `.meta` files with added, moved, or renamed assets.

## Session Context Workflow
When starting work in this project, first check recent local Codex history in `%USERPROFILE%\.codex\history.jsonl` and the matching files under `%USERPROFILE%\.codex\sessions`. Prioritize sessions related to this Unity RPG project, especially recent bug-fixing, UI, quest, dialogue, inventory, shop, save-system, and skill-system conversations. Before making changes, summarize the relevant prior context briefly and use it to avoid repeating earlier dead ends or reverting intentional design decisions.

## Testing Guidelines
`com.unity.test-framework` is installed, but no dedicated test tree exists yet. Add new tests under `Assets/Tests/EditMode` or `Assets/Tests/PlayMode`, and name files `FeatureNameTests.cs`. Prefer EditMode tests for pure logic and PlayMode tests for scene, animation, physics, or prefab behavior. Validate touched scenes and prefabs in the editor before opening a PR.

## Commit & Pull Request Guidelines
Git history is not available in this snapshot, so use short imperative commit messages with a scope when possible, such as `feat: add shard cooldown` or `fix: prevent enemy stun loop`. Keep commits focused on one gameplay or content change. PRs should include a summary, affected scenes/prefabs, test evidence, and screenshots or short clips for UI, animation, VFX, or layout changes.
