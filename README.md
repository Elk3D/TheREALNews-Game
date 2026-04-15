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

## Phase 1 — Office (3D) · Full Setup Guide

Goal: player spawns at the office door, walks across a small open-plan room,
crosses a trigger in front of the desk, the camera locks, and the game hands
off to Phase 2. Budget: ~60 min from a fresh URP project.

If anything below takes more than its time-box, fall back to the **Shortcut**
note inside that step.

---

### Step 0 — One-time project prep (5 min)

1. Create a Unity 6 project from the **Universal 3D (URP)** template.
2. Copy this repo's `Assets/` over the project's `Assets/` and let Unity
   recompile.
3. **DOTween:** Asset Store → *DOTween (HOTween v2)* → Import → run
   `Tools → Demigiant → DOTween Utility Panel → Setup DOTween…` →
   *Apply*. Without this `Door.cs` and `CameraEffects.cs` won't compile.
4. **Tags & Layers** (`Edit → Project Settings → Tags and Layers`):
   - Tag: add **`Player`** (already exists in fresh projects).
   - Layer 8: add **`Player`** (used by `PlayerMovement`'s ceiling
     SphereCast — without it the player can't crouch under low geometry).
5. **Input axes:** confirm the default Input Manager has `Horizontal`,
   `Vertical`, `Mouse X`, `Mouse Y`, `Jump`. (Default URP project has all
   five.) The scripts use the legacy `Input` class — do **not** install the
   new Input System package.

---

### Step 1 — Create the scene (2 min)

`File → New Scene → Basic (URP)` → save as `Assets/Scenes/Office.unity`.

Delete the default *Cube* if present. Leave the *Directional Light* and
*Main Camera* for now — both are replaced below.

---

### Step 2 — Build the office shell (10 min)

Greybox geometry only — the aesthetic comes from materials + lighting.

```
Office (empty)
├── Floor          Plane, scale (2, 1, 2)            → ~20 × 20 m
├── Walls (empty)
│   ├── Wall_N     Cube, scale (20, 3, 0.2), z = +10
│   ├── Wall_S     Cube, scale (20, 3, 0.2), z = -10
│   ├── Wall_E     Cube, scale (0.2, 3, 20), x = +10
│   └── Wall_W     Cube, scale (0.2, 3, 20), x = -10  (gap for door)
├── Ceiling        Plane, rotated (180,0,0), y = 3
├── Desk           Cube, scale (1.6, 0.8, 0.8), pos (0, 0.4, 6)
├── Chair          Cube, scale (0.5, 0.5, 0.5),  pos (0, 0.25, 5.2)
└── Monitor        Cube, scale (0.6, 0.4, 0.05), pos (0, 1.1, 6.2)
```

Make three URP/Lit materials and assign:
- `M_Floor` — base color **#E8E6E1** (off-white), smoothness 0.15
- `M_Wall`  — base color **#F2F0EC**, smoothness 0.05
- `M_Desk`  — base color **#3A3A3A**, smoothness 0.4

> **Shortcut:** if greyboxing eats the budget, just place a Floor + 4 walls +
> a single Cube labeled "Desk". Aesthetic can come later — what we need
> tonight is the trigger flow.

---

### Step 3 — Fluorescent ceiling lights (5 min)

Disable the default *Directional Light* (don't delete — keep for reference).

For each fluorescent fixture (place 3–4 evenly along the ceiling):

1. `GameObject → Light → Point Light`, set:
   - Position y = 2.9, intensity **8**, range **6**
   - Color **#F4F8FF** (a touch cool)
2. Add **`LightFlicker`** component (`Assets/Scripts/World/LightFlicker.cs`)
   with: WaveType = **NOISE**, Base = **1**, Amplitude = **0.06**,
   Frequency = **30**, Phase = a random number per fixture so they don't
   flicker in sync.

Result: a subtle, uneasy hum without anything looking obviously broken.

> Project Settings → Quality → make sure **Pixel Light Count ≥ 4** or only
> the closest fixtures will flicker.

---

### Step 4 — Build the Player prefab (15 min)

This hierarchy must match exactly — the serialized fields on `Player.cs`
expect these transforms by name/role.

```
Player                 (CharacterController, Player.cs, PlayerLock.cs, AudioSource)
└── HeadContainer      (empty, local pos 0, 1.6, 0)
    └── CameraHolder   (empty, local pos 0, 0, 0)
        └── Camera     (Camera, AudioListener, tag MainCamera)
```

**Player root:**
- Tag: **Player**, Layer: **Player**.
- `CharacterController`: Center **(0, 0.9, 0)**, Radius **0.3**, Height **1.8**,
  Slope Limit 45, Step Offset 0.3.
- `AudioSource`: Spatial Blend 1, Play On Awake **off** (this is the footsteps
  source).

**Add `Player.cs` and fill the inspector:**
- Containers → Head Container = `HeadContainer`, Camera Holder = `CameraHolder`
- Camera section → Camera = the `Camera` child (auto-found if left empty)
- `PlayerMovement` block — leave defaults (MoveSpeed 2, RunSpeed 2.5,
  Crouch on, Jump on).
- `PlayerLook` block — Sensitivity **0.5**, Vertical clamp ±82.
- `CameraHeadBob` block — defaults are fine; check **Enable**.
- `PlayerFootsteps` block — drag your `AudioSource` into `m_AudioSource`,
  drop 3–6 footstep clips into `m_FootstepClips`. Asset Store has free packs
  named "footsteps wood/concrete".

**Add `PlayerLock.cs`** (no fields — used by EventTrigger UnityEvent in Step 6).

Drag `Player` into `Assets/Prefabs/Player.prefab`.

> **Shortcut:** if you're skipping audio for the prototype, leave
> `m_FootstepClips` empty and `PlayerFootsteps.Tick` no-ops cleanly.

---

### Step 5 — Place the GameManager (2 min)

Empty `GameObject` named **`_GameManager`**, add `GameManager.cs`. Drag the
Player prefab instance from the scene into its `m_Player` slot.

Defaults: Starting Approval **100**, Game Over Threshold **40**, Daily Quota
**8**. Tweak later from this single inspector.

Place `Player` prefab instance at the door position — somewhere like
`(-9, 0, -8)` looking toward the desk (rotation Y ≈ 0).

---

### Step 6 — The desk trigger (5 min)

This is the Phase 1 → Phase 2 hand-off.

1. Empty `GameObject` named **`Desk_Trigger`**, child of `Office`.
2. Position it ~1 m in front of the chair: `(0, 0.9, 4.5)`.
3. Add a **Box Collider**, size `(2, 1.8, 1)`, **Is Trigger ON**.
4. Add **`EventTrigger`** (`Assets/Scripts/World/EventTrigger.cs`):
   - Trigger Tag: **`Player`**
   - Single Use: **on**
   - Active: **on**
5. In the `OnEnter ()` UnityEvent box click **+** twice and wire:
   - **Slot 1:** drag the `Player` prefab instance →
     `PlayerLock.Lock` (function dropdown)
   - **Slot 2:** drag `_GameManager` →
     `GameManager.EnterPhase` → set the dropdown enum to **Documents**

When the player walks into the box, `Player.Lock()` freezes movement +
releases the cursor, and the GameManager fires `OnPhaseChanged(Documents)`,
which Phase 2's UI controller will subscribe to.

> **Optional — sit-down feel:** add a third UnityEvent slot that calls
> `Camera.transform.DOMove` (via a tiny helper script) to the chair seat
> position. Skip if it eats time.

---

### Step 7 — The office door (10 min, optional but nice)

Use the existing `Door.cs` for entry/exit polish.

1. Build the door: `Cube` scaled `(0.05, 2, 0.9)` parented to an empty
   **`Hinge`** at the wall edge.
2. Two empty markers as siblings of the door:
   - **`OpenLocation`** — Hinge rotated Y +90° at the same position
   - **`CloseLocation`** — Hinge at Y 0°
3. Add an `EventTrigger` on a small Box Collider in front of the door
   (`Single Use` **off**), tag-filtered to Player.
4. On the door root add `Door.cs`:
   - Trigger = the door's EventTrigger
   - Hinge / OpenLocation / CloseLocation = wired as above
   - SpeedOpen 1, SpeedClose 0.5, EaseOpen InOutSine
   - AudioSource = an AudioSource on the door, Open/Close clips optional

> **Shortcut:** skip the door entirely. Phase 1 reads fine as
> *spawn → walk → desk*.

---

### Step 8 — Lighting + post (5 min)

`Window → Rendering → Lighting`:
- Environment Lighting Source → **Color**, Ambient = **#D8D8D2**
- Realtime Global Illumination **off** (we're using point lights only)
- Click **Generate Lighting** once after placing fixtures.

Add a **Global Volume** (`GameObject → Volume → Global Volume`) → New profile.
Override:
- **Tonemapping** — Mode: Neutral
- **Color Adjustments** — Saturation **−15**, Contrast **+5**
- **Vignette** — Intensity **0.18**, Smoothness 0.4

That's the corporate-sterile look on a budget.

---

### Step 9 — Smoke test the loop (3 min)

Press **Play**:

- [ ] Cursor locks, mouse-look works, vertical clamp engages near ±82°.
- [ ] WASD walks. Shift+W runs. Camera bobs while moving and stops on idle.
- [ ] Footsteps trigger if clips are wired.
- [ ] Lights flicker subtly.
- [ ] Walk into `Desk_Trigger` → cursor releases, movement freezes.
- [ ] In the *_GameManager* inspector, **Current Phase** flips from
  `Office` to `Documents`. (Run-time only — won't persist.)

If any checkbox fails, see the *Common gotchas* section below before
sinking time into a rewrite.

---

### Common gotchas

| Symptom | Fix |
|---|---|
| Player falls through floor | Floor needs a `Mesh Collider` or you used a Plane without one — replace with a thin Cube. |
| Mouse-look feels glued / framerate-dependent | You're on the new Input System. Project Settings → Player → **Active Input Handling = Input Manager (Old)** or **Both**, then restart Unity. |
| Crouch never re-stands under low ceilings | Player layer wasn't created (Step 0.4). Without it the SphereCast mask is `~0` and self-hits the controller. |
| `LightFlicker` doesn't visibly flicker | Quality settings → Pixel Light Count too low, or Amplitude too small relative to Base. |
| Trigger never fires | Box Collider's **Is Trigger** is off, or the Player's collider is on a layer the trigger ignores. Make sure both are on Default or Player and Physics matrix has them colliding. |
| `GameManager.Instance` null on Awake from another script | `GameManager` has `[DefaultExecutionOrder(-1000)]` so it wakes first — but if you call it from another `Awake()`, switch to `Start()`. |
| DOTween "TweenCallback was null" warnings | Re-run the DOTween Setup utility (Step 0.3). |

---

### What "Phase 1 done" means

When you can press Play, walk to the desk, see Cursor unlock + the
GameManager's `CurrentPhase` switch to `Documents` in the inspector — Phase 1
is done. Everything visual beyond that (door swing, sit-down lerp, ambient
hum loop) is polish that can wait until Phase 2 is playable.

---

## Phase 2 — Documents (2D UI)

To be built. `GameManager` already exposes:

- `Approval` (starts 100, game over below 40)
- `DocumentsProcessed`, `WordsRedacted`, `DailyQuota`
- `RecordSubmission(words, spinScore01)` — applies the spin-score curve
- `OnMetricsChanged`, `OnGameOver` events for UI binding
