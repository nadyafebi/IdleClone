# IdleClone

A 2D idle/platformer game built in Unity inspired by IdleOn. The goal is to create a 30 minutes demo with core features of IdleOn.

## Project Overview
- **Engine**: Unity 6000.3, 2D, URP
- **Target Platform**: WebGL
- **UI**: UI Toolkit

## Scene Structure

### Global Game Manager (persistent, `DontDestroyOnLoad`)
Lives in the first scene and survives all scene loads. Never put per-scene objects here.
```
Game Manager       — GameManager, ClickRouter
├── Dialog         — DialogController
└── Screen Fader   — ScreenFader
```

### Per-Scene hierarchy
Each scene has a local Map Manager root. Player, camera, and input indicator all live here and are recreated on every scene load.
```
Map Manager        — MapManager
├── Main Camera
├── Global Light 2D
├── Cursor         — ClickIndicator
└── Player         — PlayerMovement
    └── Player Sprite  — PlayerRenderer
```

## C# Conventions

**Naming**
- Private fields: `_camelCase` with underscore prefix
- Public properties and methods: `PascalCase`
- `[SerializeField]` on its own line above each field it applies to
- `[Header("...")]` / `[Tooltip("...")]` for Inspector organization

**Structure**
- Organize each MonoBehaviour with `#region` blocks in this order:
  `Serialized Fields` → `Public Properties` → `Private Fields` → `Unity Lifecycle` → `Public Methods` → (feature regions) → `Editor Visualisation`
- Static utility classes get a `Private Helpers` region at the bottom

**Comments**
- Only comment the non-obvious WHY: invariants, workarounds, subtle timing constraints
- No docstrings, no "what this does" comments

**Patterns**
- Guard clauses / early returns preferred over deep nesting
- `Debug.Log` / `LogWarning` / `LogError` messages prefixed with `[ClassName]`
- Plain C# classes (not MonoBehaviours) for pure data (e.g. `PlatformNode`)
- Static classes for pure logic with no scene lifetime (e.g. `PlatformPathfinder`)
- Coroutines for frame-spread movement; store the handle to allow cancellation

**Unity**
- New Input System (`UnityEngine.InputSystem`) — not the legacy `Input` class
- `FindFirstObjectByType<T>()` — not the deprecated `FindObjectOfType<T>()`
- Always disable the component (`enabled = false`) and log an error when a required dependency is missing in `Start()`
- Gizmo debug drawing behind a `[SerializeField] bool _drawGizmos` toggle
