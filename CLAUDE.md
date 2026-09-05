# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity FPS survival/base-building game. Unity **6000.3.11f1**. No CLI build/lint/test scripts exist in the repo — compilation and play-testing happen through the Unity Editor (open the project, or `Unity.exe -projectPath . -batchmode -quiet -logFile - -executeMethod <Method>` for a headless run). There is no automated test suite (`com.unity.test-framework` is a package dependency but no `*Tests` assemblies exist yet) — verify changes by opening the relevant scene in the Editor.

Solution file `Anchorpoint.slnx` lists one `.csproj` per module (Unity-generated, one per `.asmdef`) — open it in Rider/VS for IntelliSense, but don't hand-edit `.csproj`/`.slnx`, Unity regenerates them.

Three scenes: `Assets/_Main/Scenes/Menu.unity`, `Camp.unity` (base-building/hub), `Game.unity` (extraction level).

## Architecture

Read `Assets/_Main/Documentation/Architecture.md` and `CodeStyle_v_1.md` in full before writing code — they are the team's binding rulebook, enforced at review. Summary of the load-bearing rules:

- **Module = functional area + public API.** Each module under `Assets/_Main/Scripts/<Name>Module/` is its own `.asmdef`. External code must depend only on a module's public interfaces, never its internals. Every module has a `Docs/README.md`; the index is `Assets/_Main/Documentation/Modules.md`. **Read a module's `Docs/README.md` before modifying it** — it names the module's own extension points and constraints, which vary per module.
- **DI via Zenject.** Bindings live *only* in each module's `Installers/` folder — no ad-hoc `Container.Bind` elsewhere.
- **Clean Architecture / Use Cases for cross-module orchestration only.** When one module needs to react to another without depending on it directly (e.g. camera reacting to player crouch), the Use Case goes in a root/orchestration layer and depends only on module interfaces, not internals.
- **Game entities: MVC.** Model = plain logic (no Unity where possible). View = `MonoBehaviour` (render/anim/VFX). Controller = created via DI, owns the Model, wires input/events to it. View is instantiated by Unity (scene/prefab) and never creates Model/Controller itself.
- **UI: MVP.** View has no logic, only UI events + `Render` methods. Presenter wires input/events to UseCases/Model and calls `View.Render(...)` — never touches Unity UI components directly (e.g. no `button.interactable = ...` in a Presenter).
- **Async: UniTask only**, no coroutines. `async void` is banned — use `UniTaskVoid` + `.Forget()` for fire-and-forget. Long-running operations must accept a `CancellationToken`.
- Third-party/plugin code (Zenject, DOTween, Odin Inspector, the reusable `AudioModule` core) lives in `Assets/Plugins/`. `Assets/_Main/` is exclusively the team's own content — keep that boundary when adding new assets.

### Module map

Game modules (`Assets/_Main/Scripts/`): `PlayerModule`, `EnemyModule` (FSM-driven AI, states in `EnemyModule/AIModule`), `WeaponModule`, `InventoryModule`, `BuildingModule`, `BaseModule` (base leveling/rewards), `CampSaveModule`, `GameCycleModule` (win/lose), `EvacuationModule`, `SpawnModule`, `MenuModule`, `NpcBaseModule`, `UIModule` (shared HUD), `VFXModule`.

Infrastructure modules: `ComponentsModule` (reusable entity components shared by player/enemies: health, movement, ranged attack, AI perception — implement narrow interfaces, don't add domain logic here), `FSMModule` (engine-agnostic state machine, no Zenject/game-domain knowledge — module owners adapt it, e.g. `EnemyModule/AIModule`), `SaveModule` (`ISaveable` + JSON snapshot persistence; owners register their `ISaveable` and must keep `SaveKey` stable across versions), `InputModule` (wraps the generated Unity Input System actions behind `IInputMap`/`IInputService` — other modules must depend on these, never read Input System directly), `CommandsModule` (`IAsyncCommand` contract for UI actions, returns `UniTask<TaskResult>`), `SharedData` (small cross-cutting constants only, e.g. Animator hashes — no behavior), `UtilsModule` (generic, domain-free helpers only).

`AudioModule` under `_Main` is the game-side integration layer (mixer volume, settings persistence) over the reusable core in `Assets/Plugins/AudioModule`.

## Code style (from `CodeStyle_v_1.md`)

- Entities (classes/fields/vars) are nouns, no abbreviations except conventional ones (`ID`, `UI`, `URL`). Methods are verbs. Booleans start with `is/has/can/should`. Events are past-tense facts (`HealthChanged`, not `OnHealthChanged` — `On` prefix is reserved for the *handler* method).
- Constants: `PascalCase` (not `SCREAMING_CASE`). Private fields: `_camelCase`. `[SerializeField] private` inspector fields: plain `camelCase`, never `public`. Public members: `PascalCase`.
- Explicit access modifiers everywhere (except interface members). Prefer expression-bodied read-only properties and one-line methods (`=>`).
- One class per file, filename matches class name. Member order: fields → events → constructors → properties → methods; within each, `public` → `protected` → `private`, except Unity lifecycle methods (`Awake`/`Start`/`Update`...) always come before hand-written methods.
- Namespace = module's root folder name.
- **No comments in code** except `// TODO:`, `// FIXME:`, and `///` XML doc on public APIs — code must be self-explanatory instead.
- No `Debug.Log` in product code. No `FindObjectOfType`/`GameObject.Find` — use DI/inspector refs/factories. No `GetComponent<T>()` in `Update`, and no `[RequireComponent]` + `GetComponent` in `Awake` combo — wire refs via `[SerializeField]` instead. Event subscribe/unsubscribe in `OnEnable`/`OnDisable`. Use `Action`, never `UnityAction`.
- Async methods get an `Async` suffix and a `CancellationToken` parameter.

## Git workflow (from `GitFlow.md`)

`main` = stable/release only. `develop` = active development trunk. Feature branches: `feature/*`, named `nickname/feature-name`, branched from `develop`. Finish a feature → open a PR against `develop` (approver: `@Vanadium19`) → **squash merge** → delete the feature branch. Merge conflicts get resolved on the feature branch, never on `develop`.
