# THE REAL NEWS

Dark-comedy satire prototype. You play a document cleaner at a media firm —
swap flagged words from a curated list before the publication preview goes out.

## Project setup

This repo currently ships only the **scripts**. To run, drop them into a fresh
Unity 6 project:

1. Create a new project: **Unity 6 / Universal 3D (URP)** template.
2. Close Unity. From this repo, copy the `Assets/` folder over the new
   project's `Assets/` (merge — don't replace).
3. Install **DOTween (HOTween v2)** from the Asset Store, then run
   `Tools → Demigiant → DOTween Utility Panel → Setup DOTween`.
4. Re-open the project. Scripts compile under the `TheRealNews.*` namespaces.

> Project-level Unity files (`ProjectSettings/`, `Packages/`, `.unity` scenes)
> are not committed — Unity regenerates them per machine and they bloat diffs
> for a 2-day prototype.

## Code layout

```
Assets/Scripts/
  Core/        GameManager (singleton, run-state, phase, metrics)
  Player/      Player, PlayerMovement, PlayerLook, CameraHeadBob,
               PlayerFootsteps, PlayerLock
  World/       EventTrigger, Door, LightFlicker, SoundTrigger
  Utility/     CameraEffects (DOTween shake)
  Documents/   (Phase 2 — to be added)
```

All scripts are plain `MonoBehaviour`s — the original decompiled framework
types (`JMonoBehaviour`, `JDisposable`, `ActionEvent`, `PlayerInput` static,
`GameCamera`, `CameraMovements`, `CameraTilt`) are gone. Sub-systems are
`[Serializable]` classes driven by `Player.Update()`.

## Phase 1 — Office (3D)

1. Scene with a `Player` prefab (CharacterController + Camera child).
2. An `EventTrigger` collider in front of the desk.
3. Wire `EventTrigger.OnEnter → GameManager.EnterPhase(Documents)` and
   `→ Player.Lock`.

## Phase 2 — Documents (2D UI)

To be built. `GameManager` already exposes:

- `Approval` (starts 100, game over below 40)
- `DocumentsProcessed`, `WordsRedacted`, `DailyQuota`
- `RecordSubmission(words, spinScore01)` — applies the spin-score curve
- `OnMetricsChanged`, `OnGameOver` events for UI binding
