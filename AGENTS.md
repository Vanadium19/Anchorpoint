# AGENTS.md

Guidance for coding agents in this repo. These instructions take precedence over agent defaults.

## Project

Unity FPS survival/base-building game. Unity **6000.3.11f1**. No CLI build/lint/test in repo — compile & play-test via the Unity Editor (open project, or `Unity.exe -projectPath . -batchmode -quiet -logFile - -executeMethod <Method>` headless). No test suite exists (`com.unity.test-framework` is a dep but no `*Tests` assemblies). Verify changes by opening the relevant scene in the Editor.

`Anchorpoint.slnx` = one Unity-generated `.csproj` per `.asmdef`. Open in Rider/VS for IntelliSense; never hand-edit `.csproj`/`.slnx` (regenerated).

Scenes: `Assets/_Main/Scenes/` — `Menu.unity`, `Camp.unity` (base-building hub), `Game.unity` (extraction level).

## Token discipline (Unity YAML)

`.unity` / `.asset` / `.prefab` / `.mat` files are huge (Camp.unity ~300KB). **Never Read one whole.** Use Grep with `-n` + context to find the block, then Read a narrow `offset`/`limit` range. Prefer editing scene/asset wiring in the Editor over text-patching YAML.

## Architecture

Read `Assets/_Main/Documentation/Architecture.md` and `CodeStyle_v_1.md` in full before writing code — binding, enforced at review. Load-bearing rules:

- **Module = functional area + public API.** Each module `Assets/_Main/Scripts/<Name>Module/` = own `.asmdef`. External code depends only on a module's public interfaces, never internals. Every module has `Docs/README.md`; index = `Assets/_Main/Documentation/Modules.md`. **Read a module's `Docs/README.md` before modifying it** — extension points & constraints vary per module.
- **DI via Zenject.** Bindings live *only* in each module's `Installers/` folder — no ad-hoc `Container.Bind` elsewhere.
- **Clean Architecture / Use Cases for cross-module orchestration only.** One module reacting to another without depending on it directly → Use Case in a root/orchestration layer, depends only on module interfaces.
- **Game entities: MVC.** Model = plain logic (no Unity where possible). View = `MonoBehaviour` (render/anim/VFX). Controller = DI-created, owns Model, wires input/events to it. View is instantiated by Unity (scene/prefab), never creates Model/Controller itself.
- **UI: MVP.** View = UI events + `Render` methods, no logic. Presenter wires input/events to UseCases/Model, calls `View.Render(...)` — never touches Unity UI components directly (no `button.interactable = ...` in a Presenter).
- **Async: UniTask only**, no coroutines. `async void` banned — use `UniTaskVoid` + `.Forget()`. Long-running ops take a `CancellationToken`.
- Third-party/plugin code (Zenject, DOTween, Odin, reusable `AudioModule` core) → `Assets/Plugins/`. `Assets/_Main/` = team's own content only. Keep the boundary.

### Module map

Game (`Assets/_Main/Scripts/`): `PlayerModule`, `EnemyModule` (FSM AI, states in `EnemyModule/AIModule`), `WeaponModule`, `InventoryModule`, `BuildingModule`, `BaseModule` (base leveling/rewards), `CampSaveModule`, `GameCycleModule` (win/lose), `EvacuationModule`, `SpawnModule`, `MenuModule`, `NpcBaseModule`, `UIModule` (shared HUD), `VFXModule`.

Infra: `ComponentsModule` (reusable entity components — health, movement, ranged attack, AI perception; narrow interfaces, no domain logic), `FSMModule` (engine-agnostic state machine, no Zenject/domain; owners adapt it e.g. `EnemyModule/AIModule`), `SaveModule` (`ISaveable` + JSON snapshots; keep `SaveKey` stable across versions), `InputModule` (wraps generated Input System actions behind `IInputMap`/`IInputService` — depend on these, never read Input System directly), `CommandsModule` (`IAsyncCommand` → `UniTask<TaskResult>`), `SharedData` (cross-cutting constants only, e.g. Animator hashes — no behavior), `UtilsModule` (generic domain-free helpers only).

`AudioModule` under `_Main` = game-side integration (mixer volume, settings persistence) over reusable core in `Assets/Plugins/AudioModule`.

## Code style (`CodeStyle_v_1.md`)

- Classes/fields/vars = nouns, no abbreviations except `ID`, `UI`, `URL`. Methods = verbs. Booleans start `is/has/can/should`. Events = past-tense facts (`HealthChanged`; `On` prefix reserved for the *handler* method).
- Constants `PascalCase` (not `SCREAMING_CASE`). Private fields `_camelCase`. `[SerializeField] private` inspector fields: plain `camelCase`, never `public`. Public members `PascalCase`.
- Explicit access modifiers everywhere (except interface members). Prefer expression-bodied read-only properties & one-line methods (`=>`).
- One class per file, filename = class name. Member order: fields → events → constructors → properties → methods; within each `public` → `protected` → `private`. Unity lifecycle methods (`Awake`/`Start`/`Update`…) before hand-written methods.
- Namespace = module's root folder name.
- **No comments** except `// TODO:`, `// FIXME:`, `///` XML doc on public APIs. Code self-explanatory instead.
- No `Debug.Log` in product code. No `FindObjectOfType`/`GameObject.Find` — use DI/inspector refs/factories. No `GetComponent<T>()` in `Update`; no `[RequireComponent]` + `GetComponent`-in-`Awake` combo — wire refs via `[SerializeField]`. Event subscribe/unsubscribe in `OnEnable`/`OnDisable`. Use `Action`, never `UnityAction`.
- Async methods: `Async` suffix + `CancellationToken` param.
