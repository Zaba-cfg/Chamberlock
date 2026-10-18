# Chamberlock — Architecture & Roadmap

> **Document State**: Alive roadmap — updated during development.
> **Last Updated**: 2026-10-18

---

## Index

| Section | Description |
|---------|-------------|
| [1. Project Identity](#1-project-identity) | Engine, render specs, references, branches |
| [2. Game Vision](#2-game-vision) | Idea, concept, game loop, win/loss conditions |
| [3. Architecture Overview](#3-architecture-overview) | Patterns, directory structure |
| [4. Systems Documentation](#4-systems-documentation) | All implemented and planned systems |
| &emsp; 4.1 [Health](#41-health-system) | HP container, visual/audio feedback |
| &emsp; 4.2 [Movement](#42-movement-system) | Interface-driven, modifier-aware |
| &emsp; 4.3 [Look Direction](#43-look-direction-system) | 2D rotation |
| &emsp; 4.4 [Weapon System](#44-weapon-system) | Core weapon logic, strategies, data |
| &emsp; 4.5 [Projectile System](#45-projectile-system) | Bullet movement, damage, pooled lifetime |
| &emsp; 4.6 [Modifier System](#46-modifier-system) | Add-then-Multiply pipeline |
| &emsp; 4.7 [Interaction System](#47-interaction-system) | Trigger-based closest selection |
| &emsp; 4.8 [Weapon Pickup & Detection](#48-weapon-pickup--detection) | Pickup and area scanning |
| &emsp; 4.9 [Player Input](#49-player-input-system) | Mouse + Gamepad, IMoveInput |
| &emsp; 4.10 [Enemy AI](#410-enemy-ai-system) | FSM (3 baseline + per-type states) |
| &emsp; 4.11 [UI System](#411-ui-system) | HUD + menus |
| &emsp; 4.12 [Debug System](#412-debug-system) | Loggers and testers |
| &emsp; 4.13 [Object Pool](#413-object-pool-system) | Generic pooling |
| &emsp; 4.14 [Room System](#414-room-system) | Rooms, doors, map structure, transitions ([diagrams](docs/ROOMS.md)) |
| &emsp; 4.15 [Enemy Types](#415-enemy-types-planned) | Per-type behaviors (planned) |
| &emsp; 4.16 [Game Flow](#416-game-flow--scene-flow) | Game state, run tracking, scene loading |
| &emsp; 4.17 [Audio](#417-audio-system) | Music + SFX layers, mixer-driven volume |
| [5. Input System](#5-input-system) | Action bindings |
| [6. Scenes](#6-scenes) | Scene list and build status |
| [7. Prefabs](#7-prefabs) | Prefab inventory |
| [8. Packages](#8-packages) | Package dependencies |
| [9. Key Design Decisions](#9-key-design-decisions) | Architectural rationale |
| [10. What Has Been Done](#10-what-has-been-done--status-matrix) | Implemented systems |
| [11. What's Missing](#11-whats-missing--identified-gaps) | Open gaps by priority |
| [12. Roadmap](#12-roadmap--work-plan) | 7-phase work plan |
| [13. Asset Inventory](#13-asset-inventory) | SO assets, audio, sprites, prefabs |
| [14. Technical Notes](#14-technical-notes) | Pipeline diagrams, pseudocode |

---

## 1. Project Identity

| Field | Value |
|-------|-------|
| **Name** | Chamberlock |
| **Engine** | Unity 6.3 LTS (6000.3.11f1) |
| **Template** | Universal 2D |
| **Render Pipeline** | URP 17.3.0 — 2D Renderer |
| **Language** | C# (.NET Standard 2.1) |
| **Input** | Unity Input System 1.19.0 (New only) |
| **Color Space** | Linear |
| **Goal** | Learn software engineering through Unity |
| **References** | *The Binding of Isaac*, *Enter The Gungeon* |

### Visual Specs

| Property | Value |
|----------|-------|
| **Logic Resolution** | 320×180 |
| **Pixels Per Unit** | 16 PPU |
| **Filter Mode** | Point (no blur) |
| **Compression** | None |
| **Tile Size** | 16×16 |
| **Entity Footprint** | 16×24 px (1 tile wide × ~1.5 tall) |
| **Entity Sprite Canvas** | 32×32 px (body ~16×24; remainder is animation padding) |
| **Camera Orthographic Size** | 5.625 (180 ÷ 16 ÷ 2) |
| **Pixel Perfect Camera** | Assets PPU 16 · Ref Res 320×180 · Pixel Snapping On |

> **PPU math**: world size = sprite pixels ÷ 16. A 16 px body = 1.0 unit = 1 tile; 24 px = 1.5 units. Transparent canvas padding does **not** scale a sprite — only the drawn body's pixel size does. Draw every frame of a character on the **same canvas size** so frames stay aligned.
>
> **Camera**: to see a full 180 px tall at 16 PPU the orthographic size must be `180 ÷ 16 ÷ 2 = 5.625`. A size of `5` only shows 160 px.

#### Sprite Canvas Specs — Entities

| Entity | Body (visible) | Canvas | Footprint |
|--------|----------------|--------|-----------|
| **Player** | 18×26 px | 32×32 px | 1.0 × 1.5 tiles |
| **Small enemy** (baseline `Enemy`) | 16×24 px | 32×32 px | 1.0 × 1.5 tiles |
| **Medium enemy** (Melee, Shooter, Rusher) | 16×24 px | 32×32 px | 1.0 × 1.5 tiles |
| **Large enemy** (Tank) | 24×32 px | 48×48 px | 1.5 × 2.0 tiles |

> Enemy rows are target sizes for the planned roster; the current **Player** and generic **Enemy** sprites are both 18×26 px (see §13).

#### Sprite Canvas Specs — Weapons, Pickups & Projectiles

> Values are the current assets' actual sprite rects and canvases (see §13).

| Asset | Canvas | Sprite (visible) | Pivot |
|-------|--------|------------------|-------|
| Held gun — pistol (`Pistol.png`) | 16×16 px | 14×10 px | Center |
| Held gun — rifle (`Colt.png`, `Mosin.png`) | 24×16 px | 20×10 px | Center |
| Held melee (`Katana.png`) | 24×16 px | 20×8 px | Center |
| Weapon UI icon (`WeaponData._icon`) | 16×16 px | — | Center |
| Modifier / powerup pickup | 16×16 px | 16×16 px | Center |
| Projectile / bullet | 8×8 px | — | Center |
| Muzzle flash (`MuzzleFlash.png`) | 16×16 px | 14×16 px | Center |

> **Held weapon sprites are drawn landscape** (wider than tall) with a **Center** pivot; `WeaponSocket` rotates the sprite into the aim direction.
>
> **Draw at final size — never use `localScale` to resize pixel art.** The `Projectile` prefab still uses a `0.3` transform scale (placeholder); reset it to `1` once the bullet sprite is drawn at final size.
>
> **Sorting**: held weapon `Order in Layer +1` (in front of the player); floor pickups `−1` (behind the player).

#### Sprite Import Settings (apply to every sprite)

| Setting | Value |
|---------|-------|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Multiple (sprite sheets) / Single (one-offs) |
| Pixels Per Unit | 16 |
| Filter Mode | Point (no filter) |
| Compression | None |
| Generate Mip Maps | Off |
| Wrap Mode | Clamp |
| Mesh Type | Full Rect |
| Pivot | Center (default) — see table above |

> **Colliders are independent of the art.** Use a `CircleCollider2D` at the feet (~0.35 radius) — never a box that matches the full sprite.

### Branches

- `main` — stable, reviewed work
- `develop` — active integration branch

---

## 2. Game Vision

### 2.1 The Idea

A **2D top-down roguelite dungeon crawler** inspired by *The Binding of Isaac* and *Enter The Gungeon*. The player explores a dungeon composed of rooms, fighting enemies with a variety of weapons and collecting modifiers that alter their stats and abilities. Each run is unique due to random weapon pickups, modifier combinations, and enemy encounters.

### 2.2 The Concept

| Aspect | Description |
|--------|-------------|
| **Genre** | 2D top-down roguelite shooter |
| **Setting** | Dark dungeon with fantasy-themed monsters and environments |
| **Core Fantasy** | Survive increasingly difficult rooms by finding better weapons and stacking powerful modifiers |
| **Perspective** | Top-down pixel art aesthetic (320×180 logic resolution, 16 PPU) |
| **Controls** | Dual-stick style — WASD movement, mouse/gamepad aim |

**Key Pillars:**
- **Weapon Variety**: Melee, projectile, and future weapon types — each with unique fire strategies
- **Modifier Stacking**: Combine modifiers (Double Damage + Double Fire Rate + Double Shot) to create broken builds
- **Enemy Variety**: Different enemy types (baseline `Enemy`, Melee, Shooter, Robber, Rusher, Tank, Support) require different strategies
- **Permadeath**: Every run is a fresh start. No continues, no checkpoints.

### 2.3 The Game Loop

Each run follows this cycle:

```
Start Room → [Combat Room → Item Room] × N → Final Room → Victory / Game Over
```

| Phase | Description |
|-------|-------------|
| **1. Start Room** | Entry point. No enemies, no pickups. Prepare and move forward. |
| **2. Combat Room** | Doors lock on entry. Enemies spawn. Clear all enemies to unlock doors and proceed. |
| **3. Item Room** | Contains a weapon or modifier pickup. No enemies. Choose your reward. |
| **4. Repeat** | Cycle through combat and item rooms, growing stronger with each clear. |
| **5. Final Room** | Final encounter: a dense wave of normal enemies. Clear it to complete the run. |

**Between Rooms:** Player chooses which door to enter (branching paths planned for future).

```
                         ┌─ Combat ─ Item ─┐
Start ─ Combat ─ Item ──┤                  ├─ Final
                         └─ Combat ─ Item ─┘
```

### 2.4 Win & Loss Conditions

| Condition | Trigger | Result |
|-----------|---------|--------|
| **Win** | Clear the Final Room | Victory screen → Main Menu → Start a new run |
| **Lose** | Player health reaches 0 (permadeath) | Game Over screen → Run stats (time, kills, rooms cleared) → Main Menu → Start a new run |

**Permadeath Rule:** There are no continues or checkpoints. Death ends the run permanently. Progress is not saved between runs (meta-progression is a future consideration under Phase 7).

**No Other Loss Conditions:** Time limits, survival goals, or other failure states are not part of the core design. The only way to lose is to die.

---

## 3. Architecture Overview

The project follows a **composition-over-inheritance** architecture. Entities (`Player`, `Enemy`) are thin shells that assemble behavior through `[RequireComponent]` MonoBehaviours. Data lives in ScriptableObjects. Communication uses C# `Action` events.

### Core Patterns

| Pattern | Where | Purpose |
|---------|-------|---------|
| **Entity Shell** | `Player.cs`, `Enemy.cs` | Thin wrappers; all behavior in components |
| **Strategy** | `WeaponFireStrategy` (abstract SO) → `MeleeFireStrategy`, `ProjectileFireStrategy` | Swap fire behavior per weapon without code changes |
| **Observer** | `Action` events on Health, Weapon, WeaponHolder, ModifierInventory | Decoupled cross-component communication |
| **Pipeline** | `ModifierCalculator` — Add-then-Multiply | Deterministic stat modification across providers |
| **State Machine** | `EnemyBehavior` FSM (Chasing, SeekingWeapon, Attacking) | Enemy AI decision-making |
| **Interface Segregation** | `IMoveInput`, `IInteractable`, `IModifierProvider`, `IEnemy` | Marker or 1–2 member contracts; classes choose what they implement |
| **Object Pool** | `ObjectPool<T>` + `PoolManager` | Reuse projectile instances — avoid Instantiate/Destroy GC pressure |
| **Global State + Service Singletons** | `GameManager`, `RunManager`, `SceneLoader` (`Systems/Game`) | One owner per concern; game state and scene changes flow through a single place |
| **Room Controller** | `RoomController` (planned) | Locks exits, spawns enemies, tracks room clear state |
| **Template Method** | `EnemyBehavior` base + derived types | Shared FSM skeleton, overridden behavior per enemy type |

### Directory Structure

```
Scripts/
├── Interfaces/          # Contracts (IMoveInput, IInteractable, IModifierProvider, IEnemy)
├── Entities/            # Composition shells
│   ├── PlayerRelated/   # Player.cs, PlayerDeathHandler.cs
│   └── EnemyRelated/    # Enemy.cs, MeleeEnemy.cs, ShooterEnemy.cs, RobberEnemy.cs
├── Components/
│   ├── Base/            # Domain-agnostic reusable components
│   │   ├── HealthRelated/   # Health, HealthVisualFeedback, HealthAudioFeedback, DestroyOnDeath
│   │   ├── Weapon/          # WeaponPickup, WeaponDetection
│   │   ├── Interaction.cs
│   │   ├── LookDirection.cs
│   │   └── Movement.cs
│   ├── Weapons/         # Weapon system (Strategy pattern)
│   │   ├── Weapon.cs, WeaponHolder.cs, WeaponData.cs
│   │   ├── WeaponFireStrategy.cs (abstract), MeleeFireStrategy.cs
│   │   ├── WeaponFireModifierDefinition.cs
│   │   ├── WeaponFireContext.cs, WeaponFireResult.cs
│   │   ├── WeaponAmmoType.cs
│   │   ├── WeaponModifierLoadout.cs
│   │   └── WeaponAudioFeedback.cs, WeaponVisualFeedback.cs
│   ├── Modifiers/       # Stat modifier pipeline
│   │   ├── ModifierDefinition.cs (abstract), StatModifierDefinition.cs
│   │   ├── DoubleShotModifierDefinition.cs
│   │   ├── ModifierInstance.cs, ModifierInventory.cs, ModifierCalculator.cs
│   │   ├── ModifierStat.cs, ModifierOperation.cs
│   │   └── ModifierPickup.cs, TemporaryModifierPickup.cs
│   ├── EnemyComponents/ # Enemy AI (per-type subclasses)
│   │   ├── EnemyAIInput.cs, EnemyCombat.cs, EnemyWeaponDecision.cs
│   │   ├── EnemyKillCounter.cs, EnemyTargetAssigner.cs
│   │   ├── EnemyData.cs (ScriptableObject), EnemyDataApplier.cs
│   │   ├── EnemyStartingWeapon.cs
│   │   ├── ObstacleAvoidance.cs, ObstacleRaycast.cs
│   │   ├── EnemyBehaviorRelated/  # EnemyBehavior.cs (base), EnemyState.cs, MeleeBehavior.cs, ShooterBehavior.cs, RobberBehavior.cs
│   │   └── (planned) RusherBehavior, TankBehavior
│   ├── Player/          # Player-specific components
│   │   └── PlayerDeathHandler.cs
│   ├── PlayerInput/     # Unity Input System bridge
│   │   ├── PlayerInput.cs, AimDevice.cs
│   ├── Projectile/      # Projectile.cs, ProjectileFireStrategy.cs
│   └── Rooms/           # Room system (3A core, 3B map, 3C spawner)
│       ├── RoomType.cs, DoorDirection.cs, SpawnPointKind.cs (enums)
│       ├── RoomController.cs, RoomDoor.cs, RoomEnemy.cs, RoomDebugger.cs
│       ├── RoomEnemySpawner.cs, RoomItemSpawner.cs, RoomSpawnPoint.cs
│       ├── EnemySpawnEntry.cs
│       └── RoomData.cs (ScriptableObject)
├── Map/                  # (3B) MapData.cs, MapRoomEntry.cs, RoomDoorLink.cs,
│                         # DungeonManager.cs, RoomCamera.cs
├── Systems/             # Global managers and services
│   ├── Pool/              # ObjectPool.cs, PoolManager.cs, IPoolable.cs
│   ├── Game/              # GameState.cs, GameManager.cs, RunManager.cs, RunStats.cs,
│   │                      # SceneLoader.cs, SceneId.cs, PauseInput.cs, Bootstrap.cs
│   ├── Settings/          # GameSettings.cs (static, PlayerPrefs-backed)
│   └── Audio/             # AudioManager.cs (music + SFX layers, mixer-driven)
├── UI/
│   ├── QuitGame.cs      # Static helper — Editor-aware application quit
│   ├── HUD/             # PlayerHealthBarUI.cs, RunTimerUI.cs, WeaponHudUI.cs
│   ├── Menus/           # MainMenuController.cs, PauseMenuController.cs,
│   │                    # SettingsPanelController.cs, GameOverScreen.cs
│   └── WeaponAmmoUI/    # WeaponAmmoUI.cs (legacy minimal ammo text)
│   └── RoomTransition/  # RoomTransitionOverlay.cs (fade, 3B)
└── Debug/               # Debug loggers and testers
    ├── HealthDebugger.cs, HealthTester.cs
    ├── WeaponDebugger.cs, ModifierDebugger.cs
```

---

## 4. Systems Documentation

### 4.1 Health System

**Files**: `Health.cs`, `HealthVisualFeedback.cs`, `HealthAudioFeedback.cs`, `DestroyOnDeath.cs`

| Component | Role |
|-----------|------|
| `Health` | Core HP container. `TakeDamage()`, `Heal()`, `RestoreFullHealth()`. Events: `DamageTaken(float)`, `HealthChanged(float,float)`, `Died` |
| `HealthVisualFeedback` | Sprite flash on damage (coroutine-based color swap) |
| `HealthAudioFeedback` | Plays damage and death audio clips via `AudioSource.PlayOneShot` |
| `DestroyOnDeath` | Domain-agnostic: `Health.Died` → `Destroy(gameObject)`. Used by the Enemy; reusable for breakables |

**Death consumers** (subscribe to `Health.Died`, keep concerns separate):

| Component | Owner | Behaviour |
|-----------|-------|-----------|
| `DestroyOnDeath` | Enemy | Destroys the GameObject (no animation yet) |
| `EnemyKillCounter` (`Components.Enemy`) | Enemy | `RunManager.AddKill()` — feeds the run's kill stat |
| `PlayerDeathHandler` (`Components.Player`) | Player | `RunManager.EndRun()` → `GameManager.SetState(GameOver)` |

**Design Decisions**:
- Health is clamped `[0, MaxHealth]`; `IsDead` check guards all operations
- Visual feedback uses coroutine with `StopCoroutine` to prevent stacking
- Audio requires both clips at `Awake` (fail-fast with `MissingReferenceException`)
- Death behaviour is split into small single-purpose components rather than embedded in `Health`, so entities compose only what they need
- `PlayerDeathHandler` no-ops when there is no `GameManager` (keeps the `SampleScene` sandbox playable); input and movement need no explicit disabling — `GameOver` sets `Time.timeScale = 0` and `Player` gates itself on `GameManager.IsRunning`

**Status**: ✅ Complete

---

### 4.2 Movement System

**File**: `Movement.cs`

- Reads `IMoveInput.MoveInput` (works for Player and Enemy via interface)
- Applies speed through `ModifierInventory.CalculateValue` (modifiers affect movement)
- Uses `Rigidbody2D.linearVelocity` in `FixedUpdate`
- Clamps input magnitude to 1

**Status**: ✅ Complete

---

### 4.3 Look Direction System

**File**: `LookDirection.cs`

- Stores normalized `Forward` direction
- Sets `transform.rotation` via `Quaternion.Euler(0, 0, angle)` — 2D rotation around Z
- Ignores zero vectors

**Status**: ✅ Complete

---

### 4.4 Weapon System

**Core Files**: `Weapon.cs`, `WeaponHolder.cs`, `WeaponData.cs`, `WeaponFireStrategy.cs`, `WeaponFireModifierDefinition.cs`, `WeaponFireContext.cs`, `WeaponFireResult.cs`, `WeaponAmmoType.cs`, `WeaponModifierLoadout.cs`

#### Data Layer (`WeaponData` ScriptableObject)
| Property | Type | Description |
|----------|------|-------------|
| `_weaponName` | string | Display name |
| `_icon` | Sprite | UI icon |
| `_damage` | float | Base damage |
| `_fireRate` | float | Shots per second |
| `_magazineSize` | int | Magazine capacity |
| `_reloadDuration` | float | Reload time in seconds |
| `_maxReserveAmmo` | int | Max reserve ammo |
| `_weaponFireStrategy` | WeaponFireStrategy | Fire behavior SO |
| `_ammoType` | WeaponAmmoType | Magazine or Infinite |

#### Runtime (`Weapon`)
- `TryFire()` returns `WeaponFireResult` enum (Success, NotEquipped, Cooldown, NoAmmo, Reloading)
- Applies `WeaponFireModifierDefinition` modifiers to `WeaponFireContext` before strategy execution
- Calculates modified stats (Damage, FireRate, ReloadDuration) via `ModifierCalculator` with weapon + holder providers
- `AddModifier()` / `AddTemporaryModifier()` — writes into the weapon's own `ModifierInventory` (weapon-scoped, see §4.6)
- `WeaponModifierLoadout` — optional component: applies a predefined `ModifierDefinition[]` on spawn, so prefab variants can define pre-modded weapons (e.g. Double Shot Pistol)
- Tracks ammo, cooldowns via `Time.time`

#### Weapon Holder (`WeaponHolder`)
- Manages equip/drop lifecycle with socket parenting
- `Equip()` handles mutual exclusion (drops current, steals from other holders)
- Events: `WeaponChanged(Weapon)` for UI binding

#### Fire Strategies
| Strategy | Implementation |
|----------|---------------|
| `MeleeFireStrategy` | `Physics2D.OverlapCircleAll` with spread angle, deduplication via `HashSet<Health>`, minimum radius multiplier; enemy owners skip `IEnemy` targets (no friendly fire) |
| `ProjectileFireStrategy` | Fetches a `Projectile` from `PoolManager` per attack count with spread angle |

**Assets**:
| Weapon | Damage | FireRate | Mag | Reload | Reserve | Spread | Strategy | Ammo |
|--------|--------|----------|-----|--------|---------|--------|----------|------|
| Pistol | 10 | 3 | 7 | 2s | 21 | 10° | Projectile | Magazine |
| Colt | 5 | 10 | 30 | 5s | 90 | 70° | Projectile | Magazine |
| Mosin | 30 | 1 | 5 | 5s | 15 | 10° | Projectile | Magazine |
| Katana | 10 | 1 | 1 | 0s | 0 | 90° | Melee | Infinite |

**Status**: ✅ Complete (melee no-friendly-fire verified)

---

### 4.5 Projectile System

**Files**: `Projectile.cs`, `ProjectileFireStrategy.cs`

- `Projectile` moves via `Rigidbody2D.linearVelocity` in direction
- Deals damage on `OnTriggerEnter2D` via `Health.TakeDamage`
- Enemy-fired projectiles **pass through enemies** (owner side cached at `Initialize` via the `IEnemy` marker; ally hits skip both damage and pool return) — consistent with LOS gating (§4.10), which also ignores allies
- Implements `IPoolable` — zeroes its velocity on get/return
- Returns to the pool on hit or after its 5 s lifetime (never `Destroy`ed)
- `ProjectileFireStrategy` fetches instances from `PoolManager` (§4.13)

**Status**: ✅ Complete (no-friendly-fire verified)

---

### 4.6 Modifier System

**Core Files**: `ModifierDefinition.cs`, `WeaponFireModifierDefinition.cs`, `StatModifierDefinition.cs`, `DoubleShotModifierDefinition.cs`, `ModifierInstance.cs`, `ModifierInventory.cs`, `ModifierCalculator.cs`, `ModifierStat.cs`, `ModifierOperation.cs`

#### Calculation Pipeline
```
result = baseValue
→ Add all Add modifiers (across all providers)
→ Multiply all Multiply modifiers (across all providers)
```

#### Modifier Types
| Type | Base | Implementation |
|------|------|---------------|
| `StatModifierDefinition` | `ModifierDefinition` | Modifies a `ModifierStat` with `ModifierOperation` (Add/Multiply) |
| `DoubleShotModifierDefinition` | `WeaponFireModifierDefinition` | Mutates `WeaponFireContext`: `AttackCount++`, `SpreadAngle += 15` |

#### Stats Affected
| Stat | Enum | Example |
|------|------|---------|
| Damage | `ModifierStat.Damage` | DoubleDamage: ×2 |
| Fire Rate | `ModifierStat.FireRate` | DoubleFireRate: ×2 |
| Reload Duration | `ModifierStat.ReloadDuration` | DoubleReload: ×0.1 |
| Movement Speed | `ModifierStat.MovementSpeed` | DoubleSpeed: ×2 |

#### Modifier Inventory
- `AddModifier()` — permanent
- `AddTemporaryModifier()` — auto-expires via `Time.time` check in `Update()`
- `RemoveModifier()` — manual removal
- Events: `ModifierAdded`, `ModifierRemoved`

#### Modifier Pickups
| Prefab | Behavior |
|--------|----------|
| `ModifierPickup` | `IInteractable` → adds permanent modifier to the owner's inventory → destroys self |
| `TemporaryModifierPickup` | `IInteractable` → adds temporary modifier with duration → destroys self |

#### Weapon-Scoped Modifiers
- `Weapon.AddModifier()` / `Weapon.AddTemporaryModifier()` — write into the weapon's own `ModifierInventory` (the missing write path; the read path in `Weapon` already consumed it).
- `WeaponModifierLoadout` component (§4.4): applies a predefined `ModifierDefinition[]` on spawn — a pre-modded weapon is just a **prefab variant** with this component; any spawner that instantiates the prefab gets the mods for free.
- Two scopes are merged at read time: **weapon inventory + holder (owner) inventory** — owner mods (pickups) affect every weapon held, weapon mods (loadout) affect only that weapon and travel with the weapon object when dropped.

> Each modifier shows its matching **16×16 icon** from `Assets/Sprites/Modifiers` (see §1 → Sprite Canvas Specs — Weapons, Pickups & Projectiles). The pickup objects are authored directly in `SampleScene` (there are no pickup prefabs yet).

**Status**: ✅ Complete (weapon-scoped loadout `WeaponModifierLoadout` verified)

---

### 4.7 Interaction System

**File**: `Interaction.cs`

- `CircleCollider2D` (trigger) for overlap detection
- Maintains list of `IInteractable` objects in range
- `Interact()` finds closest interactable by squared distance
- Used for weapon pickup and modifier pickup

**Status**: ✅ Complete

---

### 4.8 Weapon Pickup & Detection

**Files**: `WeaponPickup.cs`, `WeaponDetection.cs`

| Component | Role |
|-----------|------|
| `WeaponPickup` | `IInteractable` → calls `WeaponHolder.Equip()` on interactor |
| `WeaponDetection` | `CircleCollider2D` trigger → tracks nearby weapons in a list (used by enemy AI) |

**Status**: ✅ Complete

---

### 4.9 Player Input System

**Files**: `PlayerInput.cs`, `AimDevice.cs`, `PlayerInputActions.cs` (auto-generated)

#### Input Actions
| Action | Mouse/Keyboard | Gamepad |
|--------|---------------|---------|
| Move | WASD | Left Stick + Dpad |
| Look Pointer | Mouse Position | — |
| Look Stick | — | Right Stick |
| Fire | Left Click | Right Shoulder |
| Interact | E | South Button (A) |
| Reload | R | East Button (B) |

- Implements `IMoveInput` for `Movement`
- Events: `FirePressed`, `ReloadPressed`, `InteractPressed`; `FireHeld` is a `bool` property (polled by `Weapon`)
- `AimDevice` enum tracks Mouse vs Gamepad for aim source switching

**Status**: ✅ Complete

---

### 4.10 Enemy AI System

**Files**: `EnemyAIInput.cs`, `EnemyCombat.cs`, `EnemyWeaponDecision.cs`, `EnemyBehavior.cs`, `EnemyState.cs`, `ObstacleAvoidance.cs`, `ObstacleRaycast.cs`

#### Baseline FSM (3 states)
```
Chasing ──────► SeekingWeapon (no weapon / weapon empty)
  ▲                    │
  │                    │ weapon found & in range
  │◄───────────────────┘
  │
  │ in attack range + has weapon
  ▼
Attacking ────► SeekingWeapon (weapon empty / dropped)
```

| State | Behavior |
|-------|----------|
| `Chasing` | Follows target, looks at target. Transitions to SeekingWeapon if no weapon or empty. Transitions to Attacking if in range. |
| `SeekingWeapon` | Finds closest weapon via `WeaponDetection`, moves toward it, picks up when in range. Falls back to Chasing if no weapons. |
| `Attacking` | Faces target, fires weapon. Reloads if ammo=0. Drops weapon if empty. Transitions to Chasing if out of range. |

| Component | Role |
|-----------|------|
| `EnemyAIInput` | Implements `IMoveInput` — direction toward `Target`, invertible via `SetRetreating(bool)` (scaled by `_retreatSpeedMultiplier`) and haltable via `SetMoveEnabled(bool)`; passes the final direction (after retreat inversion) through `ObstacleAvoidance` when present |
| `EnemyCombat` | `CanAttack()` = range check **and** line-of-sight (`ObstacleRaycast.IsPathClear`, toggleable via `_requireLineOfSight`); calls `WeaponHolder.TryFire` |
| `EnemyWeaponDecision` | `GetClosestAvailableWeapon(filter)` / `IsWeaponInPickupRange()` / `TryEquipNearbyWeapon(filter)` — optional `Func<Weapon, bool>` filter so types can restrict pickups |
| `ObstacleAvoidance` | Whisker steering: direct ray blocked → casts ±`_whiskerAngle` whiskers over `_checkDistance` → takes the freer side (remembered to prevent jitter); dead-end → 90° turn along the wall. Added on the base `Enemy` prefab |
| `ObstacleRaycast` | Static `IsPathClear(origin, direction, distance)` — `RaycastNonAlloc` with a shared buffer; skips trigger colliders and anything with `Health`, so only walls/props block (no Layer setup required) |

#### Navigation & line-of-sight
- **Wall avoidance**: `EnemyAIInput` post-processes its move direction with `ObstacleAvoidance`, so chasing, retreating, fleeing, and weapon-seeking all route around obstacles instead of welding to them.
- **LOS gating**: `EnemyCombat.CanAttack()` also requires a clear ray to the target — enemies never fire through walls (melee swings included). Breaking LOS drops the FSM back to `Chasing`, so avoidance takes over until the enemy re-acquires the player.
- Allies don't block LOS (they have `Health`), matching projectile behavior.

#### Dispatch & per-type states
- `Update()` → `protected virtual HandleState(EnemyState)` — a switch that dispatches the 3 baseline states to `protected virtual` handlers (`HandleChasing`, `HandleSeekingWeapon`, `HandleAttacking`); subclasses override `HandleState` to add states and override individual handlers to modify rules (Template Method).
- `CurrentState` has a `protected` setter; `protected virtual EnemyState InitialState` (default `Chasing`) selects the entry state per type.
- **Per-type states** (Phase 4): `EnemyState.Hiding`, `Stealing`, `Fleeing` — used only by `RobberBehavior`.
- The weapon-acceptance hook `protected virtual bool CanAcceptWeapon(Weapon)` defaults to `true` (baseline `Enemy`); `MeleeBehavior` / `ShooterBehavior` narrow it.
- Baseline `Enemy` behavior is unchanged by all of the above — it runs exactly the diagram in this section.

**Status**: ✅ Complete (baseline FSM + per-type extension points; per-type behaviors in §4.15; navigation add-on `ObstacleAvoidance` + LOS gating verified)

---

### 4.11 UI System

#### HUD (`Scripts/UI/HUD`) — lives under `GameplayCanvas/HUD`

| Component | Binds to | Role |
|-----------|----------|------|
| `PlayerHealthBarUI` | `Health.HealthChanged` | Drives a `Slider` (min 0 → max `MaxHealth`). Reads the initial value in `Start`, after `Health.Awake` |
| `RunTimerUI` | polls `RunManager.ElapsedTime` | Elapsed time as `mm:ss` |
| `WeaponHudUI` | `WeaponHolder.WeaponChanged`, `Weapon.AmmoChanged` / `ReloadStarted` / `ReloadCompleted` | Icon, name, ammo/reserve (or `∞`), reload indicator |

`WeaponAmmoUI` (`Scripts/UI/WeaponAmmoUI`) remains as the **legacy** minimal ammo text used in `SampleScene`; the full HUD uses `WeaponHudUI`.

**Note.** Health/weapon HUDs subscribe in `OnEnable` and unsubscribe in `OnDisable`. Any read of another component's state happens in `Start`, because `Awake`/`OnEnable` order across different GameObjects is not guaranteed.

#### Menus (`Scripts/UI/Menus`)

| Component | Role |
|-----------|------|
| `MainMenuController` | Main Menu: **New Game** (starts a run, switches to `Running`, loads `Gameplay`), **Settings**, **Quit**. |
| `PauseMenuController` | Gameplay pause: routes the `Pause` action — closes settings if open, otherwise toggles pause. **Resume**, **Settings**, **Quit to Menu**. |
| `SettingsPanelController` | Shared settings UI (one prefab, instanced by both menus). Music/SFX volume, fullscreen toggle, resolution dropdown. Raises `Opened` / `Closed`. |
| `GameOverScreen` | Overlay shown on `GameState.GameOver`. Fills time/kills/rooms from `RunManager`; **Restart** (StartRun → Running → load `Gameplay`) and **Main Menu** (EndRun → MainMenu → load `MainMenu`). The component sits on a parent while only its `Panel` child is toggled. |
| `QuitGame` | Static helper — stops Play mode in the Editor, `Application.Quit()` in a build. |

**Mutual exclusion.** The settings panel is a *sibling of the menu items* inside the same menu panel, so hiding the panel would also hide settings. Instead, when settings opens the owner controller hides the panel's other children and restores them when `SettingsPanelController` raises `Closed` — the two menus never overlap. Visibility is owned by the menu controllers; the settings panel stays agnostic of who hosts it.

**Persistence.** `GameSettings` (namespace `Systems.Settings`) is a static, `PlayerPrefs`-backed store; `SettingsPanelController` writes straight to it and `Bootstrap` applies the saved values on startup. `AudioManager` consumes the volume values (§4.17).

**Gameplay canvas layout.**

The whole canvas is the `GameplayCanvas` prefab (§7), instanced in the `Gameplay` scene with per-scene bindings for the player's `Health` and `WeaponHolder`.

```
GameplayCanvas
├── HUD            # health bar, run timer, weapon HUD
├── PauseMenu      # shown only while Paused
└── GameOverScreen # overlay shown only while GameOver
```

**Status**: ✅ Complete (HUD, Main Menu, Pause Menu, Settings and Game Over done; modifier HUD still pending)

---

### 4.12 Debug System

**Files**: `HealthDebugger.cs`, `HealthTester.cs`, `WeaponDebugger.cs`, `ModifierDebugger.cs`

| Component | Role |
|-----------|------|
| `HealthDebugger` | Logs health changes, damage taken, death |
| `HealthTester` | `ContextMenu` actions: TakeDamage, Heal, RestoreFullHealth |
| `WeaponDebugger` | Logs fire, reload start/complete, ammo changes |
| `ModifierDebugger` | Logs modifier added/removed with stat/operation/value/duration |

**Status**: ✅ Complete

---

### 4.13 Object Pool System

> Reference: *Enter The Gungeon* — heavy bullet usage demands pooling.

**Files**: `Scripts/Systems/Pool/ObjectPool.cs`, `PoolManager.cs`, `IPoolable.cs` (namespace `Systems.Pool`)

| Component | Role |
|-----------|------|
| `ObjectPool<T>` | Generic pool where `T : Component` — `Stack<T>` of available instances + `HashSet<T>` of in-use instances (O(1) double-return guard). `Get()`, `Release()`, `Preload()` |
| `PoolManager` | Scene singleton (`PoolManager.Instance`). Serialized entries (prefab + prewarm count), fail-fast validation at `Awake`, owns the `Pools` container GameObject |
| `IPoolable` | Interface — `OnGetFromPool()`, `OnReturnToPool()` callbacks |

**Design**:
- `PoolManager.Awake()` validates entries (throws on null prefab or duplicate) and eagerly instantiates each entry's prewarm count as inactive seed instances
- The strongly typed `ObjectPool<T>` is created on first `Get<T>()` and absorbs the seed instances (avoids reflection; expansion is lazy)
- Expansion beyond prewarm is unlimited — a `MaxSize` cap is deferred to the polish phase
- Pooled instances are parented under a `Pools` container child of the `PoolManager`; the manager nulls `Instance` in `OnDestroy` so scene unloads leave no stale statics
- `Projectile` implements `IPoolable` — zeroes rigidbody velocity on get/return; returns itself to the pool on hit **or** after its 5 s lifetime instead of `Destroy`
- `ProjectileFireStrategy` fetches via `PoolManager.Instance.Get(...)` — throws `MissingReferenceException` if the scene has no `PoolManager` or the prefab is not registered (fail-fast)
- Registered entries: `Projectile` prefab, prewarm 32 (pool grows on demand)

**Known limitations**: pool growth is unbounded; seeds for a registered-but-never-used prefab stay inactive in the scene.

**Status**: ✅ Complete (projectiles pooled; enemies/pickups can register entries later)

---

### 4.14 Room System

> Reference: *The Binding of Isaac* — rooms are the core spatial unit.
>
> Class & communication diagrams: **[`docs/ROOMS.md`](docs/ROOMS.md)** — every room class, its relations, the prefab composition, and a full sender/receiver matrix.

**Files**: `Components/Rooms/` — `RoomController.cs`, `RoomDoor.cs`, `RoomData.cs`, `RoomEnemy.cs`, `RoomEnemySpawner.cs`, `RoomItemSpawner.cs`, `RoomSpawnPoint.cs`, `EnemySpawnEntry.cs`, `RoomType.cs`, `DoorDirection.cs`, `SpawnPointKind.cs`, `RoomDebugger.cs`. Map: `Components/Map/` — `MapData.cs`, `MapRoomEntry.cs`, `RoomDoorLink.cs`, `DungeonManager.cs`, `RoomCamera.cs`; `UI/RoomTransition/RoomTransitionOverlay.cs`.

#### Room Types
| Type | Behavior |
|------|----------|
| **Combat** | Spawns enemies. Exits lock. Clear all enemies to unlock. |
| **Item** | Contains a weapon or modifier pickup. No enemies. |
| **Final** | Final room. Dense wave of normal enemies (boss enemy cut — out of scope). Clears = map complete (ends the run). |
| **Start** | Entry point. No enemies, no pickups. |

#### Room Controller
- `RoomData` (ScriptableObject): room type, tile dimensions, the enemy spawn list (`EnemySpawnEntry`: prefab + count), and an optional `ItemPickupPrefab` (a weapon prefab or a `ModifierPickup` prefab).
- `RoomController` (MonoBehaviour): state machine `Idle → Active → Cleared`. Activates when the player enters the room's trigger collider (or explicitly via the DungeonManager). Only `Combat`/`Final` rooms lock; `Start`/`Item` never lock and never count.
- `RoomDoor`: a `Collider2D` **trigger** detects the player; a child `Blocker` collider is enabled only while locked. Unlocked/cleared rooms stay unlocked.
- `RoomEnemy`: registrar on each enemy — finds its parent `RoomController`, registers on enable, forwards `Health.Died`. Keeps death accounting off `DestroyOnDeath`'s ordering.
- `RoomEnemySpawner` / `RoomItemSpawner`: on `RoomActivated`, spawn once from `RoomData` at the room's `RoomSpawnPoint`s filtered by `SpawnPointKind` (`Enemy` / `Item`). Enemies parent under the spawn root so `RoomEnemy` can find the room; the item pickup parents under the room so a taken pickup can never respawn (`Activate()` fires once per cached instance).
- Clearing the last tracked enemy → unlock doors, fire `RoomCleared`, call `RunManager.AddRoomCleared()`. The clear check runs whenever an enemy leaves the tracked set (death *or* removal), so it does not depend on `DestroyOnDeath` firing `Died` before it destroys the enemy.
- Clearing a `RoomType.Final` room also completes the run: `RoomController.CompleteRun()` calls `RunManager.EndRun()` then `GameManager.SetState(GameState.GameOver)`; the dedicated victory screen (5.5) now handles the run-end flow instead of the old Game Over placeholder.

#### Map & Transitions
- `MapData` (ScriptableObject): ordered `MapRoomEntry` list — each entry = `RoomData` + room prefab + `worldPosition` + explicit `RoomDoorLink`s (`from` direction → `toRoomIndex` → `to` direction).
- **Discrete rooms**: one active room prefab at a time. `DungeonManager` instantiates the entry room, subscribes to its doors, and on a linked door crossing: fade out → deactivate the current room → activate (or first-time instantiate) the next → teleport the player just inside the arrival door → `Activate()` → snap camera → fade in.
- **Room caching (backtracking)**: visited rooms are cached in a dictionary and only deactivated, never destroyed, so their state persists — dead enemies stay dead, a dropped weapon stays on the floor, a taken pickup does not come back. `Activate()` is idempotent, so a cleared room re-shown does not re-lock or re-spawn.
- `RoomCamera` locks the view to the active room root (no player-follow); `RoomTransitionOverlay` (CanvasGroup) provides the black fade and blocks input during a swap.
- `RoomDoorLink`s are authored **forward**; `DungeonManager` builds a runtime connection table and adds the reverse of every link automatically. `RoomDoorLink._oneWay` opts a link out of auto-reverse (e.g. a Final Room door that seals behind you). A room needs a door matching each link's `to` direction.

#### Room Generator (Future)
- If procedural generation is added later: `RoomGenerator` picks from a pool of room templates
- Templates are pre-built tilemap rooms with spawn points
- Generator connects them in a valid graph

**Status**: ✅ Complete — 3.1–3.5 (room core + map/transitions), 3.7 (enemy spawner), 3.8 (item room) and 3.9 (start room) and 3.10 (backtracking) are done. 3.6 (room tilemap) was **dropped** — rooms are hand-crafted prefabs with sprite art, and a spare 16×16 tileset lives in `Assets/Sprites/Room/TileSet`. The **Final Room** is authored and ends the run on clear (the multi-phase boss enemy was **cut — out of scope**; the room uses normal enemies). Run end shows the dedicated victory screen (5.5).

---

### 4.15 Enemy Types (Planned)

> Reference: *The Binding of Isaac* — varied enemy behaviors create emergent difficulty.

**Base**: `EnemyBehavior` (existing FSM) extended with per-type subclasses.

| Type | Behavior | Attack Pattern |
|------|----------|----------------|
| **Melee** | Always closes distance to the player and strikes at melee reach. Only ever equips melee weapons. | Melee (melee weapon) |
| **Shooter** | Shoots while slowly backing away (0.6× retreat speed). When its weapon empties, picks up a new projectile weapon from the floor; keeps its distance while unarmed. | Ranged (projectile) |
| **Rusher** | Charges in a straight line when in range. Brief telegraph. Melee only. | Dash (charge) |
| **Floater** | Moves erratically (sine wave). Fires sporadically. | Ranged (slow, erratic) |
| **Tank** | Slow, high HP. Uses a heavy slow weapon with great damage and slow projectile speed. | Ranged (heavy, slow) |
| **Support** | Stays far. Buffs nearby enemies (speed/damage aura). | Passive aura |
| **Robber** | If player has weapon: rushes in, steals it, runs away, then shoots player with it after a few seconds. If player has no weapon: hides and stays as far as possible until player picks one up. | Adaptive (steal → ranged) |
| ~~**Boss**~~ | **Cut** — out of scope. The Final Room now uses normal enemies; clearing it completes the run. | — |

#### Implementation Approach
```
EnemyBehavior (base)        ← existing FSM skeleton + CanAcceptWeapon() hook (Template Method)
  ├── MeleeBehavior         ← done: melee-only weapon filter (4.2)
  ├── ShooterBehavior       ← done: retreat while firing, projectile-only filter (4.3)
  ├── RusherBehavior        ← (planned) telegraph, charge, cooldown (melee only)
  ├── TankBehavior          ← (planned) slow approach, heavy slow projectile
  ├── RobberBehavior        ← done: Hiding/Stealing/Fleeing FSM, steals via `WeaponHolder.Equip` (4.4)
  └── BossBehavior          ← (cut) boss enemy removed from scope; the Final Room uses normal enemies
```

#### Melee Enemy FSM Detail
> First per-type behavior of Phase 4. Same 3-state FSM as the baseline `Enemy` — only the decision rules change.

```
Chasing (always toward the player)
  ├─ has melee weapon & in strike range → Attacking
  ├─ no weapon → SeekingWeapon (melee weapons only)
  └─ holds a ranged weapon → drop it → SeekingWeapon

SeekingWeapon (melee filter)
  ├─ equips closest unequipped, non-empty weapon whose
  │   WeaponFireStrategy is MeleeFireStrategy
  ├─ no melee weapon available → Chasing (chases barehanded; never
  │   settles for a ranged weapon)
  └─ on pickup → Chasing

Attacking
  ├─ faces the player, fires at melee reach (short attack range)
  ├─ melee weapons are Infinite ammo → never reloads, never drops
  └─ player out of reach → Chasing (immediately closes distance again)
```

- **Weapon filter**: `EnemyWeaponDecision.GetClosestAvailableWeapon(filter)` / `TryEquipNearbyWeapon(filter)` take an optional `Func<Weapon, bool>` — `MeleeBehavior` overrides `CanAcceptWeapon()` to accept only weapons whose `WeaponFireStrategy is MeleeFireStrategy`; the baseline `Enemy` keeps accepting any weapon (hook returns `true`).
- **Attack range**: driven by `EnemyData.AttackRange` (applied via `EnemyDataApplier` → `EnemyCombat.SetAttackRange`), tuned to the melee weapon's reach (~1.25, matching `MeleeFireStrategy._range`) so `CanAttack()` only succeeds inside strike distance — the enemy never kites.
- **Spawn equipped**: `EnemyStartingWeapon` instantiates a weapon prefab (e.g. Katana) and `WeaponHolder.Equip`s it in `Start`, so the enemy is armed even when the room spawned no melee pickups.
- **No fallback to ranged**: unlike the baseline enemy, it never equips a firearm; if no melee weapon is available, it keeps pressuring the player barehanded (contact damage is a future option).
- **Distinct from Rusher**: the Rusher telegraphs and dashes (a charge attack with cooldown); the Melee enemy simply walks up and swings — steady pressure, no dash.

#### Shooter Enemy FSM Detail
> Kiting mirror of the baseline enemy: same FSM, but it backs away instead of closing in.

```
Chasing (toward the player)
  ├─ has projectile weapon & in attack range → Attacking
  ├─ no weapon → SeekingWeapon (projectile weapons only)
  └─ holds a melee weapon → drop it → SeekingWeapon

SeekingWeapon (projectile filter)
  ├─ equips closest unequipped, non-empty weapon whose
  │   WeaponFireStrategy is ProjectileFireStrategy
  ├─ none available → stays in SeekingWeapon: faces the player
  │   and retreats (keeps distance while barehanded)
  └─ on pickup → Chasing

Attacking
  ├─ faces the player, fires while retreating (0.6× speed)
  ├─ projectile weapons reload (Pistol) — keeps backing away meanwhile
  ├─ weapon empty (mag + reserve 0) → drop it → SeekingWeapon
  └─ player out of attack range → Chasing (walks back into range)
```

- **Retreat mechanics**: `EnemyAIInput.SetRetreating(bool)` inverts the move direction; serialized `_retreatSpeedMultiplier` (Shooter prefab: `0.6`) makes backing off slower than approaching. `ShooterBehavior` flips the flag per state: `false` in Chasing/SeekingWeapon, `true` in Attacking.
- **Unarmed behavior**: if no acceptable weapon exists on the floor, it stays in `SeekingWeapon`, faces the player and retreats — it never walks into melee empty-handed (unlike the baseline fallback).
- **Reload loop**: `HandleAttacking` keeps firing; at 0 ammo `Weapon.Reload()` runs while the enemy continues to retreat (Pistol: 7 mag / 21 reserve).

#### Files (implemented)
| File | Role |
|------|------|
| `EnemyData.cs` (SO) | Per-type stats: name, sprite, max HP, move speed, attack range. Menu: `Enemies/Enemy Data` |
| `EnemyDataApplier.cs` | `[DefaultExecutionOrder(-90)]` — pushes `EnemyData` into `Health.SetMaxHealth`, `Movement.SetBaseSpeed`, `EnemyCombat.SetAttackRange`, `SpriteRenderer` at Awake |
| `MeleeBehavior.cs` | `EnemyBehavior` subclass — overrides `CanAcceptWeapon()` (melee only) |
| `ShooterBehavior.cs` | `EnemyBehavior` subclass — projectile-only filter + retreat-while-firing (`SetRetreating` per state) |
| `RobberBehavior.cs` | `EnemyBehavior` subclass — `Hiding`/`Stealing`/`Fleeing` states via `HandleState` override; steals the player's held weapon, flees `_fleeDuration`, plants and fires |
| `EnemyStartingWeapon.cs` | Spawns the assigned weapon prefab and equips it in `Start` |
| `MeleeEnemy.cs` (`Entities/EnemyRelated`) | Entity shell — requires `MeleeBehavior` instead of `EnemyBehavior` |
| `ShooterEnemy.cs` (`Entities/EnemyRelated`) | Entity shell — requires `ShooterBehavior` |
| `RobberEnemy.cs` (`Entities/EnemyRelated`) | Entity shell — requires `RobberBehavior`; spawns barehanded (no `EnemyStartingWeapon`) |

**Status**: ✅ **Phase 4 (Enemy Variety) complete** — Melee (4.1 + 4.2), Shooter (4.3) and Robber (4.4) code + assets + prefabs authored and verified in Play mode; spawner (4.5) was already done. Boss (Phase 5) was **cut — out of scope**; the Final Room is a normal-enemy encounter that ends the run.

#### Robber FSM Detail
```
Hiding (player has no weapon)
  ├─ player picks up weapon → Stealing
  └─ stays far, avoids contact

Stealing (player has weapon)
  ├─ rush toward player's holder
  ├─ on reach: steal weapon (unequip from player, equip on self)
  └─ → Fleeing

Fleeing
  ├─ run away from player for N seconds
  └─ → Attacking

Attacking
  ├─ stop, face player, fire stolen weapon
  └─ continue attacking until weapon empty or player dies
```

- **State extension**: `RobberBehavior` starts at `InitialState = Hiding` and overrides `HandleState` for the three new states; it never enters `Chasing`/`SeekingWeapon`.
- **Theft mechanics**: no new API needed — `WeaponHolder.Equip(weapon)` already drops a weapon from its current holder, so equipping the player's held weapon *is* the steal (range = `EnemyWeaponDecision.IsWeaponInPickupRange`, i.e. the robber must reach the player).
- **Player weapon detection**: lazily resolves `Player → WeaponHolder` via `FindFirstObjectByType<Player>()` (no `Awake` in the subclass — Unity would shadow the base's private `Awake`).
- **Standing still while shooting**: `EnemyAIInput.SetMoveEnabled(false)` — unique to the Robber's Attacking; the Shooter fires on the run, the Robber plants its feet.
- **Hiding comfort zone**: retreats only while the player is within `EnemyCombat` attack range; holds position when the player is far.
- **Stolen weapon variety**: no filter — it steals *whatever* the player holds (a stolen Katana makes it close in like a melee enemy, since `HandleAttacking` approaches when out of range).

- Each type is a `MonoBehaviour` that inherits from or composes `EnemyBehavior`
- `EnemyCombat` adapts per type (some use `WeaponHolder`, some use custom logic)

---

### 4.16 Game Flow & Scene Flow

**Files**: `GameState.cs`, `GameManager.cs`, `RunManager.cs`, `RunStats.cs`, `SceneLoader.cs`, `SceneId.cs`, `Bootstrap.cs` (namespace `Systems.Game`)

Core infrastructure shared by every scene. Built as Stage A of the Phase 1 foundation.

#### Game States
| State | `Time.timeScale` | Meaning |
|-------|------------------|---------|
| `MainMenu` | 1 | In a menu |
| `Running` | 1 | Playing a run |
| `Paused` | 0 | Run suspended |
| `GameOver` | 0 | Run ended (freeze world, show results) |

#### Components
| Component | Role |
|-----------|------|
| `GameManager` | Persistent singleton. Holds `CurrentState`, fires `StateChanged(prev, next)`. **Only writer of `Time.timeScale`** — nothing else touches it. `TogglePause()` switches Running ⇄ Paused. |
| `RunManager` | Persistent singleton. `ElapsedTime`, `Kills`, `RoomsCleared`, `IsRunning`. `StartRun()`, `EndRun()` (fires `RunEnded(RunStats)`), `AddKill()`, `AddRoomCleared()`. Uses `Time.deltaTime`, so it freezes automatically while paused. |
| `SceneLoader` | Persistent singleton. The **only** caller of `SceneManager`. `Load(SceneId, onComplete)` async single-load with `ProgressChanged` / `SceneLoaded` events. Throws if the scene is not in Build Settings or a load is already in flight. |
| `Bootstrap` | Entry point of the `Bootstrap` scene. Verifies the managers exist (fail-fast), loads `GameSettings`, applies display settings and audio volumes, sets `MainMenu`, then transitions to the `MainMenu` scene. Runs in `Start()` so every manager `Awake()` has completed. |
| `PauseInput` | Persistent reader for the `Pause` action (`Player/Pause`). Raises `PausePressed`; the Pause Menu controller decides what a press means (close settings, or toggle pause). |
| `SceneId` | Enum of loadable scenes. **Values must match scene file names** — `Bootstrap`, `MainMenu`, `Gameplay`. |

#### Scene Flow
```
Bootstrap (index 0)  →  MainMenu  ⇄  Gameplay
        │                   │            │
   creates the         Start Game    Run in progress;
   persistent root       → Running    Return to Menu / Game Over
```

- The `Bootstrap` scene contains a single `Persistent` object holding `GameManager`, `RunManager`, `SceneLoader`, `PauseInput`, `AudioManager` and `Bootstrap`.
- All six call `DontDestroyOnLoad`, so they survive scene changes as one unit.
- Scene changes are requested by `SceneId` and resolved by name via `SceneLoader` — gameplay code never references `SceneManager`.

#### Persistent Object Lifecycle
- `DontDestroyOnLoad` is set inside each manager's `Awake` — there is no manual "don't destroy" step in the Inspector.
- Each manager uses the same fail-fast singleton guard as `PoolManager` (throws on a second instance, nulls `Instance` in `OnDestroy`).
- Always run the game from the `Bootstrap` scene. Playing `MainMenu`/`Gameplay` directly leaves the manager references null by design.

#### Known limitations
- No loading screen or fade yet; `ProgressChanged` exists for Phase 3.5 to consume.
- `RunStats` is displayed by the Game Over screen (§4.11); it has no other consumer yet.

**Status**: ✅ Complete (state, run tracking, scene flow); menus done (§4.11), audio done (§4.17)

---

### 4.17 Audio System

**Files**: `AudioManager.cs` (namespace `Systems.Audio`)

Persistent singleton providing two mixer-driven layers. It mirrors the Settings volumes, so the sliders control everything from one place.

#### Layers
| Layer | Backing | API |
|-------|---------|-----|
| **Music** | looping `AudioSource` created as a child | `PlayMusic(clip, loop)` / `StopMusic()` |
| **SFX** | one-shot `AudioSource` (`PlayOneShot`, overlaps) | `PlaySfx(clip, volumeScale)` |

#### Volume
- `AudioMixer` (`MainMixer`) with `Master → Music` and `Master → SFX` groups; the group volumes are exposed as `MusicVolume` / `SfxVolume`.
- `ApplyVolumes()` reads `GameSettings.MusicVolume` / `SfxVolume` and calls `AudioMixer.SetFloat` in decibels (`Mathf.Log10(v) * 20f`; clamped to `-80 dB` at silence).
- Subscribes to `GameSettings.MusicVolumeChanged` / `SfxVolumeChanged` in `OnEnable` and unsubscribes in `OnDisable`, so the sliders apply live.
- `Bootstrap` calls `ApplyVolumes()` once after `GameSettings.Load()` to apply the saved values at startup.
- Warns once if an exposed parameter name does not match the mixer.

#### Wiring
- Lives on the `Bootstrap` scene's `Persistent` object (sixth component), so music survives scene changes.
- `Bootstrap` declares `[RequireComponent(typeof(AudioManager))]` and fails fast if it is missing.
- `Bootstrap` starts the **global music track** (`[SerializeField] private AudioClip _music`) via `PlayMusic` right after `ApplyVolumes()`; the same-clip guard prevents accidental restarts.
- Existing `WeaponAudioFeedback` / `HealthAudioFeedback` sources route to the `SFX` group through their `Output` field, so they obey the SFX slider with no code changes.

#### Known limitations
- No cross-fade, ducking, or per-scene music automation yet (Phase 6 territory — `SceneLoader.SceneLoaded` is the hook when needed).

**Status**: ✅ Complete (mixer + both layers wired; global music playing from `Bootstrap`)

---

## 5. Input System

**Asset**: `PlayerInputActions.inputactions`

| Action | Type | Bindings |
|--------|------|----------|
| Move | Value (Vector2) | WASD, Gamepad dpad, Gamepad leftStick |
| LookPointer | Value (Vector2) | Mouse position |
| LookStick | Value (Vector2) | Gamepad rightStick |
| Fire | Button | Mouse leftButton, Gamepad rightShoulder |
| Interact | Button | Keyboard E, Gamepad buttonSouth |
| Reload | Button | Keyboard R, Gamepad buttonEast |

> `PlayerInputActions.cs` is auto-generated — do not edit manually.

---

## 6. Scenes

| Scene | In Build | Status | Contents |
|-------|----------|--------|----------|
| **Bootstrap** | ✅ Yes (index 0) | Entry | `Persistent` object: `GameManager`, `RunManager`, `SceneLoader`, `PauseInput`, `AudioManager`, `Bootstrap` |
| **MainMenu** | ✅ Yes (index 1) | ✅ Done | Main Menu UI (Title, New Game, Settings, Quit) + shared Settings panel (prefab instance) |
| **Gameplay** | ✅ Yes (index 2) | ✅ Done | Player, `PoolManager`, weapon pickups, Pause/Settings UI, HUD (health / timer / weapon) and Game Over overlay. Room system + map-driven room swapping done (3A/3B); combat rooms spawn from `RoomData` (3C.1); item room authored (3.8). |
| **SampleScene** | ✅ Yes (index 3) | Legacy sandbox | Original test scene (played directly, no `GameManager`) — not part of the game flow |

> **Always press Play from `Bootstrap`** (build index 0). The persistent managers are created there; the other scenes rely on them surviving the scene load (see §4.16).

---

## 7. Prefabs

| Prefab | Description |
|--------|-------------|
| **Player** | Player entity shell with all components (100 HP, speed 5, `PlayerDeathHandler`) |
| **Enemy** | Enemy entity shell with AI, combat, weapon detection, `DestroyOnDeath`, `EnemyKillCounter` |
| **MeleeEnemy** | Melee enemy (prefab variant of Enemy) — `MeleeBehavior`, `EnemyDataApplier` (`EnemyData_Melee`), `EnemyStartingWeapon` (Katana) |
| **ShooterEnemy** | Shooter enemy (prefab variant of Enemy) — `ShooterBehavior`, `EnemyDataApplier` (`EnemyData_Shooter`), `EnemyStartingWeapon` (Pistol), retreat 0.6× |
| **RobberEnemy** | Robber enemy (prefab variant of Enemy) — `RobberBehavior`, `EnemyDataApplier` (`EnemyData_Robber`), barehanded spawn |
| **Projectile** | Bullet — speed 15, pooled, 5 s lifetime, trigger collider |
| **Pistol / Colt / Mosin / Katana** | Weapon pickups (each with its own `WeaponData`, sprite and audio feedback) |
| **PoolManagerProjectile** | `PoolManager` pre-configured for projectile pooling |
| **SettingPanel** | Shared settings UI (instanced on the Main Menu and in Gameplay) |
| **GameplayCanvas** | Game canvas: `HUD`, `PauseMenu` and `GameOverScreen` overlays |

---

## 8. Packages

### Core
| Package | Version | Purpose |
|---------|---------|---------|
| `com.unity.render-pipelines.universal` | 17.3.0 | URP 2D rendering |
| `com.unity.inputsystem` | 1.19.0 | New Input System |
| `com.unity.ugui` | 2.0.0 | uGUI + TextMeshPro |
| `com.unity.test-framework` | 1.6.0 | Unit testing (imported, unused) |
| `com.unity.timeline` | 1.8.12 | Timeline (unused) |
| `com.unity.visualscripting` | 1.9.12 | Visual scripting (unused) |

### 2D
| Package | Version |
|---------|---------|
| `com.unity.2d.animation` | 13.0.5 |
| `com.unity.2d.tilemap` | 1.0.0 |
| `com.unity.2d.tilemap.extras` | 6.0.2 |
| `com.unity.2d.sprite` | 1.0.0 |
| `com.unity.2d.spriteshape` | 13.0.0 |
| `com.unity.2d.psdimporter` | 12.0.2 |
| `com.unity.2d.aseprite` | 3.0.2 |
| `com.unity.2d.pixel-perfect` | 5.1.1 |

---

## 9. Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| **Composition over inheritance** | Entities are thin shells; behavior is in `[RequireComponent]` MonoBehaviours — easier to test, mix, and swap |
| **ScriptableObjects for data** | Weapon stats, strategies, modifiers are SO assets — no code changes for new weapons/modifiers |
| **Strategy pattern for fire behavior** | `WeaponFireStrategy` (abstract SO) — melee vs projectile vs future types are interchangeable |
| **Add-then-Multiply pipeline** | Deterministic, order-independent for same operation type — standard game stat formula |
| **Events for decoupling** | All cross-component communication via `Action` — no direct references between feedback and logic |
| **Interface segregation** | `IMoveInput`, `IInteractable`, `IModifierProvider`, `IEnemy` — minimal contracts, multiple implementations |
| **Enemy AI as FSM** | Simple 3-state machine — easy to extend, easy to debug |
| **No `.asmdef` files** | Default `Assembly-CSharp` — simpler for a learning project of this scale |
| **New Input System only** | Legacy input disabled — clean, future-proof |
| **Room-based spatial loop** | Inspired by *The Binding of Isaac* — rooms are the core unit of progression |
| **Predefined room layouts** | v1 uses hand-crafted room prefabs (with a spare 16×16 tileset available) — procedural generation deferred to Phase 7 |
| **Object pooling from start** | Avoids GC spikes — projectiles are pooled and reused instead of instantiated/destroyed; enemies/pickups can register later |
| **Audio via mixer groups** | Music and SFX route to `AudioMixer` groups — one place controls each category's volume; `GameSettings` feeds it in decibels |
| **Enemy types via composition** | Each enemy type is a `MonoBehaviour` composing or extending base FSM — not a monolithic class |
| **Raycast steering, no nav asset** | `ObstacleAvoidance` whiskers + `ObstacleRaycast` LOS gating — ~100 lines solves wall-welding and shoot-through-walls for hand-crafted rooms; A* would be overkill until rooms become maze-like |
| **Run ends in a room, not a scene** | The Final Room (final encounter) is a special room type, not a separate scene — keeps state consistent; the boss *enemy* was cut (out of scope), so the room is a normal-enemy wave that ends the run on clear |

---

## 10. What Has Been Done — Status Matrix

| System | Status | Notes |
|--------|--------|-------|
| Health | ✅ Done | Core + Visual + Audio feedback |
| Movement | ✅ Done | Interface-driven, modifier-aware |
| Look Direction | ✅ Done | 2D rotation |
| Weapon Core | ✅ Done | Equip, Fire, Reload, Ammo, Cooldown |
| Weapon Strategies | ✅ Done | Melee + Projectile |
| Projectile | ✅ Done | Movement, damage, pooled 5 s lifetime |
| Modifier System | ✅ Done | Add/Multiply pipeline, temporary, pickups |
| Interaction | ✅ Done | Trigger-based, closest selection |
| Weapon Pickup | ✅ Done | IInteractable integration |
| Weapon Detection | ✅ Done | Area-based weapon scanning |
| Player Input | ✅ Done | Mouse + Gamepad, IMoveInput |
| Enemy AI | ✅ Done | FSM — 3 baseline states + per-type states (`HandleState` Template Method), weapon seeking (filterable), combat |
| EnemyData | ✅ Done | `Components/EnemyComponents` — `EnemyData` (SO) + `EnemyDataApplier`; per-type stats pushed into Health/Movement/EnemyCombat at spawn |
| Melee Enemy | ✅ Done | First per-type behavior — `MeleeBehavior` (melee-only weapon filter via `CanAcceptWeapon` hook), `MeleeEnemy` prefab, `EnemyStartingWeapon` (spawns equipped) |
| Shooter Enemy | ✅ Done | `ShooterBehavior` — retreat-while-firing (`EnemyAIInput.SetRetreating`, 0.6×), projectile-only weapon filter, keeps distance when unarmed; `ShooterEnemy` prefab (Pistol) |
| Robber Enemy | ✅ Done | `RobberBehavior` — `Hiding`/`Stealing`/`Fleeing` states, steals the player's held weapon via `WeaponHolder.Equip`, flees 2.5 s, plants and fires; `RobberEnemy` prefab (barehanded) |
| Weapon UI | ✅ Done | `WeaponHudUI` — icon, name, ammo, reload (legacy `WeaponAmmoUI` in SampleScene) |
| Debug Tools | ✅ Done | Loggers + testers for all systems |
| Object Pool | ✅ Done | `Systems/Pool` — `ObjectPool<T>` + `PoolManager`; projectiles pooled (prewarm 32, grows on demand) |
| Game State | ✅ Done | `Systems/Game` — `GameManager` singleton, owns `Time.timeScale` |
| Run Manager | ✅ Done | Run timer + stats, persistent (survives scene unload) |
| Scene Flow | ✅ Done | `SceneLoader` async loads; Bootstrap → MainMenu → Gameplay |
| Main Menu | ✅ Done | New Game, Settings, Quit |
| Pause Menu | ✅ Done | ESC toggle — Resume, Settings, Quit to Menu |
| Settings | ✅ Done | Music/SFX volume, resolution, fullscreen (`GameSettings`, PlayerPrefs) |
| Audio Manager | ✅ Done | `Systems/Audio` — music + SFX layers; mixer volume driven by `GameSettings` |
| HUD | ✅ Done | `Scripts/UI/HUD` — health bar, run timer, weapon HUD |
| Death Handling | ✅ Done | Enemy destroyed + kill counted; player death → run end + `GameOver` |
| Room Core | ✅ Done | `Components/Rooms` — RoomData/RoomController/RoomDoor/RoomEnemy; lock/clear verified (`8a0de8d`) |
| Map & Transitions | ✅ Done | `Components/Map` — MapData/DungeonManager/RoomCamera + fade overlay; map-driven room swapping verified |
| Enemy Spawner | ✅ Done | `Components/Rooms` — `RoomEnemySpawner` + `RoomSpawnPoint`/`EnemySpawnEntry`; combat rooms spawn once from `RoomData.Enemies` |
| Backtracking | ✅ Done | Rooms cached (never destroyed) + auto-reversed door links; cleared state, drops, and taken pickups persist |
| Item Spawner | ✅ Done | `RoomItemSpawner` + `RoomData.ItemPickupPrefab` — spawns one weapon/modifier on room activate; item room content authored (3.8) |
| Final Room | ✅ Done | `RoomType.Final` reuses the same `RoomEnemySpawner` with normal-enemy content; clearing it ends the run → victory screen (5.5). The multi-phase boss enemy was **cut** (out of scope) |

---

## 11. What's Missing — Identified Gaps

> Open (not-yet-built) work only — completed systems are listed in §10. This table is the priority index; §12 sequences the work into tasks with dependencies.

| Gap | Category | Priority | Roadmap |
|-----|----------|----------|---------|
| **Enemy types** — Rusher, Floater, Tank, Support (later) | Core | 🟢 Low | 7.3 |
| ~~**Boss** — multi-phase enemy~~ | — | **Cut** (out of scope) — the Final Room uses normal enemies and ends the run on clear |
| **Modifier HUD** — active modifiers + duration | UI | 🟡 Medium | 6.9 |
| **Animations** — idle / walk / attack / death | Visual | 🟡 Medium | 6.2 |
| **Particle effects** — blood, sparks, bursts | Visual | 🟡 Medium | 6.7 |
| **Screen shake** | Visual | 🟡 Medium | 2.8 |
| **Room / enemy SFX** | Audio | 🟡 Medium | 6.5–6.6 |
| **Unit tests** | Technical | 🟡 Medium | — |
| **Music content** — menu, combat, item | Audio | 🟢 Low | 6.4 |
| **Damage numbers** | Visual | 🟢 Low | 6.8 |
| **Minimap** | UI | 🟢 Low | 6.10 |
| **Save / meta-progression** | Technical | 🟢 Low | 7.6 |
| **Localization** | Technical | 🟢 Low | — |

---

## 12. Roadmap — Work Plan

> Ordered by dependency. Each phase builds on the previous.
> Reference: *The Binding of Isaac* (room flow, item rooms) / *Enter The Gungeon* (projectile density, dodge mechanics, weapon variety).
> Open work is summarized by priority in §11; the tables below sequence it into tasks and dependencies.

### Phase 1: Foundation (🔴 Critical)

> Core infrastructure that everything else depends on.

| # | Task | Description | Depends On |
|---|------|-------------|------------|
| 1.1 | **Object Pool** ✅ | Generic `ObjectPool<T>` + `IPoolable` interface. Pre-warm, get, return. | — |
| 1.2 | **Game State Manager** ✅ | Singleton: `MainMenu`, `Running`, `Paused`, `GameOver`. Scene-agnostic. | — |
| 1.3 | **Run Manager** ✅ | Tracks run timer, current room, run stats. Consumes GameState. | 1.2 |
| 1.4 | **Scene Flow** ✅ | Bootstrap → MainMenu → Gameplay. `SceneManager.LoadSceneAsync`. | 1.2 |
| 1.5 | **Main Menu UI** ✅ | Title, Start Game, Settings, Quit. | 1.4 |
| 1.6 | **Settings Menu** ✅ | Music volume, SFX volume, resolution dropdown, fullscreen toggle. | 1.4 |
| 1.7 | **Pause Menu** ✅ | ESC toggle. Resume, Settings, Quit to Menu. | 1.2 |
| 1.8 | **Audio Manager** ✅ | Singleton. BGM layer + SFX layer. Volume control via Settings. | 1.6 |

### Phase 2: Death & Feedback (🔴 Critical)

> Make entities die properly and give the player feedback.

| # | Task | Description | Depends On |
|---|------|-------------|------------|
| 2.1 | **Enemy Death Handling** ✅ | `Health.Died` → destroy + count kill (`DestroyOnDeath`, `EnemyKillCounter`). Animation deferred. | 1.1, 1.2 |
| 2.2 | **Player Death Handling** ✅ | `Health.Died` → `RunManager.EndRun()` + `GameOver` (input frozen by state). Animation deferred. | 1.2 |
| 2.3 | **Health Bar UI** ✅ | Slider bound to `Health.HealthChanged`. | — |
| 2.4 | **Run Timer UI** ✅ | Elapsed time (`mm:ss`) from `RunManager`. | 1.3 |
| 2.5 | **Weapon HUD** ✅ | Weapon icon, name, ammo/reserve, reload indicator. | — |
| 2.6 | **Game Over Screen** ✅ | Run stats (time, kills, rooms cleared), Restart, Main Menu buttons. | 2.2, 1.2 |
| 2.7 | **Projectile Pooling** ✅ | Refactor `ProjectileFireStrategy` to use `ObjectPool<Projectile>`. | 1.1 |
| 2.8 | **Screen Shake** | Camera shake on fire, hit, death. Coroutine-based. **Deferred by request.** | — |

### Phase 3: Room System (🔴 Critical)

> The spatial backbone of the game.

| # | Task | Description | Depends On |
|---|------|-------------|------------|
| 3.1 | **RoomData (SO)** ✅ | Room type, dimensions, door positions, enemy spawn list. | — |
| 3.2 | **RoomController** ✅ | Tracks enemies alive. `RoomCleared` event. Lock/unlock doors. | 1.2 |
| 3.3 | **RoomDoor** ✅ | Collider2D trigger. Locks on room enter, unlocks on room clear. | 3.2 |
| 3.4 | **Map Structure** ✅ | Linear room sequence: Start → Combat → Item → Combat → ... → Final. `MapData`/`MapRoomEntry`/`RoomDoorLink` + editor `Map_Test`. | 3.1, 3.2 |
| 3.5 | **Room Transitions** ✅ | Player enters door → fade → swap room → teleport → camera frame. `DungeonManager`/`RoomCamera`/`RoomTransitionOverlay`. | 3.4, 1.4 |
| 3.7 | **Combat Room** ✅ | Spawns enemies (once) on enter. Clears when all enemies dead. `RoomEnemySpawner`. | 3.2, 2.1 |
| 3.8 | **Item Room** ✅ | Item room content authored; the pickup spawns via `RoomItemSpawner` + `RoomData.ItemPickupPrefab` on room activate. | 3.2 |
| 3.9 | **Start Room** ✅ | Entry point. No enemies, no pickups. `Rooms/Start/Room_Start_1_N` prefab + data. | 3.2 |
| 3.10 | **Backtracking** ✅ | Revisit cleared rooms; room state persists (dead enemies stay dead, dropped items stay). | 3.5 |

### Phase 4: Enemy Variety (🟡 Important)

> Different enemies create emergent gameplay.

| #   | Task | Description                                                                    | Depends On |
|-----|------|--------------------------------------------------------------------------------|------------|
| 4.1 | **EnemyData (SO)** ✅ | Per-type stats: speed, HP, attack range, sprite. `EnemyData` + `EnemyDataApplier`; `EnemyData_Melee` asset authored. | — |
| 4.2 | **Melee Enemy** ✅ | Only equips melee weapons. Always closes distance to the player and strikes at reach. First per-type behavior (`MeleeBehavior`); `MeleeEnemy` prefab + `EnemyStartingWeapon` (Katana). Verified in Play mode. | 4.1, 2.1 |
| 4.3 | **Shooter Enemy** ✅ | Shoots while slowly backing away. Projectile-only weapon filter; seeks a new firearm when empty. `ShooterBehavior` + `EnemyData_Shooter` + `ShooterEnemy` prefab (Pistol, retreat 0.6×). Verified in Play mode. | 4.1, 2.1, 1.1 |
| 4.4 | **Robber Enemy** ✅ | Steals player weapon if equipped, flees, then shoots. Hides if player unarmed. `RobberBehavior` + `EnemyState` extension (`Hiding`/`Stealing`/`Fleeing`) + `EnemyData_Robber` + `RobberEnemy` prefab (barehanded). Verified in Play mode. | 4.1, 2.1, 1.1 |
| 4.5 | **Enemy Spawner** ✅ | Room-level spawner. Spawns enemies from `RoomData` definitions. | 3.7 |

### Phase 5: Final Room & Run End (🟡 Important)

> The climax of each map run. The multi-phase boss enemy was **cut (out of scope)** — the Final Room is the run-ending encounter, built from normal enemies.

| # | Task | Description | Depends On |
|---|------|-------------|------------|
| 5.1 | **Final Room** ✅ | Final room type (`RoomType.Final`): locks on enter, spawns a dense normal-enemy wave, clears → run end. | 3.2 |
| 5.2 | ~~Boss Enemy~~ ❌ | **Cut** — multi-phase boss removed from scope. | — |
| 5.3 | ~~Boss Phases~~ ❌ | **Cut** — removed with the boss enemy. | — |
| 5.5 | **Map Complete** ✅ | Run end → victory screen → back to main menu. | 5.1, 2.6 |

### Phase 6: Polish & Juice (🟢 Nice to Have)

> Make it feel good — visuals, audio, game feel.

| # | Task | Description | Depends On |
|---|------|-------------|------------|
| 6.1 | **Placeholder Sprites** | Replace colored squares with temp pixel art for all entities. | — |
| 6.2 | **Sprite Animations** | Idle, walk, attack, death for player and enemies. | 6.1 |
| 6.4 | **Music** | BGM per state: menu, combat, item room. | 1.8 |
| 6.5 | **Enemy SFX** | Attack, death, and ambient sounds per enemy type. | 1.8, 4.2-4.4 |
| 6.6 | ~~**Boss SFX**~~ | **Cut** — removed with the boss enemy. | — |
| 6.7 | **Particle Effects** | Blood splatter, muzzle sparks, death burst, room clear effect. | 6.1 |
| 6.8 | **Damage Numbers** | Floating text on hit. Color-coded (normal, crit). | — |
| 6.9 | **Modifier HUD** | Active modifier icons with duration bars. | — |
| 6.10 | **Minimap** | Room overview showing visited/current room. | 3.4 |

### Phase 7: Content Expansion (Future)

> More stuff, more runs, more replayability.

| # | Task | Description | Depends On |
|---|------|-------------|------------|
| 7.1 | **More Weapon Types** | New fire strategies: beam, shotgun, launcher, thrown. | 1.1 |
| 7.2 | **More Modifiers** | New stat and weapon fire modifiers. | — |
| 7.3 | **More Enemy Types** | Floater, Tank, Support, etc. | 4.1 |
| 7.4 | **Room Templates** | More room layouts for variety. | 3.1 |
| 7.5 | **Procedural Generation** | `RoomGenerator` picks from template pool, connects valid graphs. | 3.4 |
| 7.6 | **Meta-progression** | Persistent upgrades across runs. | 1.3 |
| 7.7 | **Shop System** | Spend currency on weapons/modifiers between rooms. | 4.5 |
| 7.8 | **Challenge Runs** | Difficulty modifiers (more enemies, less HP, tougher final room). | 7.6 |

---

## 13. Asset Inventory

### ScriptableObject Assets

| Category | Assets |
|----------|--------|
| **Weapons** | Pistol, Colt, Mosin, Katana |
| **Fire Strategies** | Melee, Projectile |
| **Modifiers** | DoubleDamage, DoubleFireRate, DoubleReload, DoubleSpeed, DoubleShot |
| **Enemies** | `Data/Enemies/EnemyData_Melee`, `Data/Enemies/EnemyData_Shooter`, `Data/Enemies/EnemyData_Robber` |
| **Rooms** | `Data/Rooms/Combat/Room_Combat_1_ES`, `Data/Rooms/Start/Room_Start_1_N` (+ item and Final Room data) |
| **Maps** | `Data/Maps/Map_Test` — 2 entries: Start → Combat |

### Audio

| Category | Count | Examples |
|----------|-------|---------|
| **Mixer** | 1 | `Assets/Audio/Mixers/MainMixer.mixer` — Master → Music + SFX (exposed `MusicVolume` / `SfxVolume`) |
| **Music** | 0 | (none yet — `Assets/Audio/Music` ready for tracks) |
| **Combat & Gore** | 17 | bone_snap, crunch, punch, slap, squelching, swipe |
| **Weapons** | 15 | sword_clash, shot_muffled, weapon_equip, weapon_upgrade |

### Sprites

| Asset | Notes |
|-------|-------|
| `Assets/Sprites/Entity/Player.png` | Player entity — sprite `Player 1` 18×26 px (32×32 canvas) @ 16 PPU |
| `Assets/Sprites/Entity/Enemy.png` | Generic enemy — sprite `Enemy` 18×26 px (32×32 canvas) @ 16 PPU |
| `Assets/Sprites/Weapons/Pistol.png` | Pistol pickup + UI icon — sprite 14×10 px (16×16 canvas) |
| `Assets/Sprites/Weapons/Colt.png` | Colt pickup + UI icon — sprite 20×10 px (24×16 canvas) |
| `Assets/Sprites/Weapons/Mosin.png` | Mosin pickup + UI icon — sprite 20×10 px (24×16 canvas) |
| `Assets/Sprites/Weapons/Katana.png` | Katana pickup + UI icon — sprite 20×8 px (24×16 canvas) |
| `Assets/Sprites/Modifiers/*.png` | One 16×16 icon per modifier (DoubleDamage, DoubleFireRate, DoubleReload, DoubleShot, DoubleSpeed) |
| `Assets/Sprites/Effects/MuzzleFlash.png` | Weapon VFX — sprite 14×16 px (16×16 canvas) |

> **Planned sprites** — per-enemy-type art (Melee/Shooter/Rusher 32×32, Tank 48×48) and a dedicated bullet sprite (the projectile currently uses a built-in placeholder). Canvas sizes in [§1 → Sprite Canvas Specs](#visual-specs); all at 16 PPU, Point filter, no compression.

### Prefabs

| Prefab | Purpose |
|--------|---------|
| Player | Player entity (Health, PlayerInput, WeaponHolder, LookDirection, Interaction, `PlayerDeathHandler`) |
| Enemy | Enemy entity (AI, WeaponHolder, Health, `DestroyOnDeath`, `EnemyKillCounter`) |
| MeleeEnemy | Melee enemy entity — prefab variant of Enemy (`MeleeBehavior`, `EnemyDataApplier`, `EnemyStartingWeapon` with Katana) |
| ShooterEnemy | Shooter enemy entity — prefab variant of Enemy (`ShooterBehavior`, `EnemyDataApplier`, `EnemyStartingWeapon` with Pistol, retreat 0.6×) |
| RobberEnemy | Robber enemy entity — prefab variant of Enemy (`RobberBehavior`, `EnemyDataApplier`, barehanded; *Flee Duration* 2.5 s) |
| Projectile | Bullet entity (pooled) |
| Colt / Pistol / Mosin / Katana | Weapon pickups (each with its own `WeaponData`) |
| PoolManagerProjectile | `PoolManager` pre-configured for projectile pooling |
| Room_Combat / Room_Start | Room prefabs (`RoomController` + doors + walls; Combat has enemies + `EnemyTargetAssigner`) |
| SettingPanel | Shared settings UI (instanced on the Main Menu and in Gameplay) |
| GameplayCanvas | Game canvas (`HUD`, `PauseMenu`, `GameOverScreen`) + `RoomTransitionOverlay` (3B) |

---

## 14. Technical Notes

### ModifierCalculator — Add-then-Multiply Pipeline

```csharp
// Pseudocode
result = baseValue;

// Phase 1: All additions (across all providers)
foreach provider in providers:
    foreach modifier in provider.Modifiers:
        if modifier.Stat == targetStat AND modifier.Operation == Add:
            result += modifier.Value;

// Phase 2: All multiplications (across all providers)
foreach provider in providers:
    foreach modifier in provider.Modifiers:
        if modifier.Stat == targetStat AND modifier.Operation == Multiply:
            result *= modifier.Value;

return result;
```

This ensures consistent calculation order regardless of modifier application sequence.

### WeaponFireContext Pipeline

```
1. Weapon.TryFire() creates WeaponFireContext(this, direction)
2. ApplyModifiers() iterates weapon's ModifierInventory
3. ApplyModifiers() iterates holder's ModifierProvider
4. WeaponFireStrategy.Execute(context) uses modified context
5. Strategy handles spread angle, attack count, damage application
```

### Enemy AI State Transitions

```
Chasing
  ├─ no weapon → SeekingWeapon
  ├─ weapon empty → drop weapon → SeekingWeapon
  └─ in attack range → Attacking

SeekingWeapon
  ├─ no weapons available → Chasing (fallback)
  └─ weapon in range → pick up → Chasing

Attacking
  ├─ no weapon → SeekingWeapon
  ├─ weapon empty → drop weapon → SeekingWeapon
  ├─ ammo=0 → reload
  └─ out of attack range → Chasing
```

---

*This document is a living roadmap. Update it as development progresses.*
