# Project-Roguelite — Architecture & Roadmap

> **Document State**: Alive roadmap — updated during development.
> **Last Updated**: 2026-10-12

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
| &emsp; 4.5 [Projectile System](#45-projectile-system) | Bullet movement, damage, auto-destroy |
| &emsp; 4.6 [Modifier System](#46-modifier-system) | Add-then-Multiply pipeline |
| &emsp; 4.7 [Interaction System](#47-interaction-system) | Trigger-based closest selection |
| &emsp; 4.8 [Weapon Pickup & Detection](#48-weapon-pickup--detection) | Pickup and area scanning |
| &emsp; 4.9 [Player Input](#49-player-input-system) | Mouse + Gamepad, IMoveInput |
| &emsp; 4.10 [Enemy AI](#410-enemy-ai-system) | 3-state FSM |
| &emsp; 4.11 [UI System](#411-ui-system) | Ammo display |
| &emsp; 4.12 [Debug System](#412-debug-system) | Loggers and testers |
| &emsp; 4.13 [Object Pool](#413-object-pool-system) | Generic pooling |
| &emsp; 4.14 [Room System](#414-room-system-planned) | Rooms, doors, map structure (planned) |
| &emsp; 4.15 [Enemy Types](#415-enemy-types-planned) | Per-type behaviors (planned) |
| [5. Input System](#5-input-system) | Action bindings |
| [6. Scenes](#6-scenes) | Scene list and build status |
| [7. Prefabs](#7-prefabs) | Prefab inventory |
| [8. Packages](#8-packages) | Package dependencies |
| [9. Key Design Decisions](#9-key-design-decisions) | Architectural rationale |
| [10. What Has Been Done](#10-what-has-been-done--status-matrix) | Status matrix |
| [11. What's Missing](#11-whats-missing--identified-gaps) | Identified gaps |
| [12. Roadmap](#12-roadmap--work-plan) | 7-phase work plan |
| [13. Asset Inventory](#13-asset-inventory) | SO assets, audio, sprites, prefabs |
| [14. Technical Notes](#14-technical-notes) | Pipeline diagrams, pseudocode |

---

## 1. Project Identity

| Field | Value |
|-------|-------|
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
| **Player** | 16×24 px | 32×32 px | 1.0 × 1.5 tiles |
| **Small enemy** (Chaser) | 16×24 px | 32×32 px | 1.0 × 1.5 tiles |
| **Medium enemy** (Shooter, Rusher) | 16×24 px | 32×32 px | 1.0 × 1.5 tiles |
| **Large enemy** (Tank) | 24×32 px | 48×48 px | 1.5 × 2.0 tiles |
| **Boss** | 48×48 px | 64×64 px | 3.0 × 3.0 tiles |

#### Sprite Canvas Specs — Weapons, Pickups & Projectiles

| Asset | Canvas | Body (visible) | Pivot |
|-------|--------|----------------|-------|
| Held gun — pistol / short | 16×16 px | ~8×12 px | Bottom-Center (grip) |
| Held gun — rifle (Mosin) | 16×24 px | ~7×18 px | Bottom-Center (grip) |
| Held melee (Katana) | 16×24 px | ~5×18 px | Bottom-Center (grip) |
| Weapon UI icon (`WeaponData._icon`) | 16×16 px | ~14×14 px | Center |
| Modifier / powerup pickup | 16×16 px | ~12×12 px | Center |
| Projectile / bullet | 8×8 px | ~4×6 px | Center |
| Muzzle flash (`MuzzleFlash.png`) | 14×16 px | — | Center |

> **Weapons are drawn pointing up (+Y) with a Bottom-Center (grip) pivot.** `WeaponSocket` is rotated −90° and the barrel (`Muzzle`) sits on local +Y, so the sprite rotates into the aim direction.
>
> **Draw at final size — never use `localScale` to resize pixel art.** The current `ProjectileWeapon` (0.46) and `Projectile` (0.3) transform scales distort pixels; reset them to `1` once real sprites exist.
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
| Pivot | Center (default) · Bottom-Center for held weapons — see table above |

> **Colliders are independent of the art.** Use a `CircleCollider2D` at the feet (~0.35 radius) — never a box that matches the full sprite.

### Branches

- `main` — stable, reviewed work
- `develop` — active integration branch

---

## 2. Game Vision

### 2.1 The Idea

A **2D top-down roguelite dungeon crawler** inspired by *The Binding of Isaac* and *Enter The Gungeon*. The player explores a dungeon composed of rooms, fighting enemies with a variety of weapons and collecting modifiers that alter their stats and abilities. Each run is unique due to random weapon drops, modifier combinations, and enemy encounters.

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
- **Enemy Variety**: Different enemy types (Chaser, Shooter, Rusher, Robber, Tank) require different strategies
- **Permadeath**: Every run is a fresh start. No continues, no checkpoints.

### 2.3 The Game Loop

Each run follows this cycle:

```
Start Room → [Combat Room → Item Room] × N → Boss Room → Victory / Game Over
```

| Phase | Description |
|-------|-------------|
| **1. Start Room** | Entry point. No enemies, no loot. Prepare and move forward. |
| **2. Combat Room** | Doors lock on entry. Waves of enemies spawn. Clear all enemies to unlock doors and proceed. |
| **3. Item Room** | Contains a weapon or modifier pickup. No enemies. Choose your reward. |
| **4. Repeat** | Cycle through combat and item rooms, growing stronger with each clear. |
| **5. Boss Room** | Final encounter. Multi-phase boss with unique attack patterns. Defeat it to complete the run. |

**Between Rooms:** Player chooses which door to enter (branching paths planned for future).

```
                         ┌─ Combat ─ Item ─┐
Start ─ Combat ─ Item ──┤                  ├─ Boss
                         └─ Combat ─ Item ─┘
```

### 2.4 Win & Loss Conditions

| Condition | Trigger | Result |
|-----------|---------|--------|
| **Win** | Defeat the Boss in the Boss Room | Victory screen → Main Menu → Start a new run |
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
| **Interface Segregation** | `IMoveInput`, `IInteractable`, `IModifierProvider` | Contracts with 1–2 members; classes choose what they implement |
| **Object Pool** | `ObjectPool<T>` + `PoolManager` | Reuse projectile instances — avoid Instantiate/Destroy GC pressure |
| **Room Controller** | `RoomController` (planned) | Locks exits, spawns enemies, tracks room clear state |
| **Template Method** | `EnemyBehavior` base + derived types | Shared FSM skeleton, overridden behavior per enemy type |

### Directory Structure

```
Scripts/
├── Interfaces/          # Contracts (IMoveInput, IInteractable, IModifierProvider)
├── Entities/            # Composition shells
│   ├── Player/          # Player.cs
│   └── Enemy/           # Enemy.cs
├── Components/
│   ├── Base/            # Domain-agnostic reusable components
│   │   ├── HealthRelated/   # Health, HealthVisualFeedback, HealthAudioFeedback
│   │   ├── Weapon/          # WeaponPickup, WeaponDetection
│   │   ├── Interaction.cs
│   │   ├── LookDirection.cs
│   │   └── Movement.cs
│   ├── Weapons/         # Weapon system (Strategy pattern)
│   │   ├── Weapon.cs, WeaponHolder.cs, WeaponData.cs
│   │   ├── WeaponFireStrategy.cs (abstract), MeleeFireStrategy.cs, ProjectileFireStrategy.cs
│   │   ├── WeaponFireContext.cs, WeaponFireResult.cs
│   │   ├── WeaponAmmoType.cs
│   │   └── WeaponAudioFeedback.cs, WeaponVisualFeedback.cs
│   ├── Projectile/      # Projectile.cs, ProjectileFireStrategy.cs
│   ├── Modifiers/       # Stat modifier pipeline
│   │   ├── ModifierDefinition.cs (abstract), StatModifierDefinition.cs
│   │   ├── DoubleShotModifierDefinition.cs
│   │   ├── ModifierInstance.cs, ModifierInventory.cs, ModifierCalculator.cs
│   │   ├── ModifierStat.cs, ModifierOperation.cs
│   │   └── ModifierPickup.cs, TemporaryModifierPickup.cs
│   ├── Enemy/           # Enemy AI (per-type subclasses)
│   │   ├── EnemyAIInput.cs, EnemyCombat.cs, EnemyWeaponDecision.cs
│   │   ├── EnemyBehavior/  # EnemyBehavior.cs (base), EnemyState.cs
│   │   ├── Types/          # (planned) ChaserEnemy, ShooterEnemy, RusherEnemy, etc.
│   │   └── Boss/           # (planned) BossBehavior, BossPhase
│   ├── PlayerInput/     # Unity Input System bridge
│   │   ├── PlayerInput.cs, AimDevice.cs
│   ├── Projectile/      # Projectile + ProjectileFireStrategy
│   └── Rooms/           # (planned) Room system
│       ├── RoomController.cs
│       ├── RoomDoor.cs
│       ├── RoomData.cs (ScriptableObject)
│       ├── RoomGenerator.cs
│       └── RoomType.cs (enum)
├── Systems/             # Global managers and services
│   ├── Pool/              # ObjectPool.cs, PoolManager.cs, IPoolable.cs
│   ├── GameManager.cs     # (planned)
│   ├── RunManager.cs      # (planned)
│   └── AudioManager.cs    # (planned)
├── UI/
│   ├── HUD/             # (planned) HealthBar, RunTimer, AmmoDisplay, ModifierDisplay
│   ├── Menus/           # (planned) MainMenu, PauseMenu, GameOverScreen, SettingsMenu
│   └── WeaponAmmoUI/    # WeaponAmmoUI.cs
└── Debug/               # Debug loggers and testers
    ├── HealthDebugger.cs, HealthTester.cs
    ├── WeaponDebugger.cs, ModifierDebugger.cs
```

---

## 4. Systems Documentation

### 4.1 Health System

**Files**: `Health.cs`, `HealthVisualFeedback.cs`, `HealthAudioFeedback.cs`

| Component | Role |
|-----------|------|
| `Health` | Core HP container. `TakeDamage()`, `Heal()`, `RestoreFullHealth()`. Events: `DamageTaken(float)`, `HealthChanged(float,float)`, `Died` |
| `HealthVisualFeedback` | Sprite flash on damage (coroutine-based color swap) |
| `HealthAudioFeedback` | Plays damage and death audio clips via `AudioSource.PlayOneShot` |

**Design Decisions**:
- Health is clamped `[0, MaxHealth]`; `IsDead` check guards all operations
- Visual feedback uses coroutine with `StopCoroutine` to prevent stacking
- Audio requires both clips at `Awake` (fail-fast with `MissingReferenceException`)

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

**Core Files**: `Weapon.cs`, `WeaponHolder.cs`, `WeaponData.cs`, `WeaponFireStrategy.cs`, `WeaponFireContext.cs`, `WeaponFireResult.cs`, `WeaponAmmoType.cs`

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
- Tracks ammo, cooldowns via `Time.time`

#### Weapon Holder (`WeaponHolder`)
- Manages equip/drop lifecycle with socket parenting
- `Equip()` handles mutual exclusion (drops current, steals from other holders)
- Events: `WeaponChanged(Weapon)` for UI binding

#### Fire Strategies
| Strategy | Implementation |
|----------|---------------|
| `MeleeFireStrategy` | `Physics2D.OverlapCircleAll` with spread angle, deduplication via `HashSet<Health>`, minimum radius multiplier |
| `ProjectileFireStrategy` | Instantiates `Projectile` prefab per attack count with spread angle |

**Assets**:
| Weapon | Damage | FireRate | Mag | Reload | Reserve | Spread | Strategy | Ammo |
|--------|--------|----------|-----|--------|---------|--------|----------|------|
| Pistol | 10 | 3 | 7 | 2s | 21 | 10° | Projectile | Magazine |
| Colt | 5 | 10 | 30 | 5s | 90 | 70° | Projectile | Magazine |
| Mosin | 30 | 1 | 5 | 5s | 15 | 10° | Projectile | Magazine |
| Katana | 10 | 1 | 1 | 0s | 0 | 90° | Melee | Infinite |

**Status**: ✅ Complete

---

### 4.5 Projectile System

**Files**: `Projectile.cs`, `ProjectileFireStrategy.cs`

- `Projectile` moves via `Rigidbody2D.linearVelocity` in direction
- Deals damage on `OnTriggerEnter2D` via `Health.TakeDamage`
- Auto-destroys after 5 seconds
- `ProjectileFireStrategy` instantiates from SO reference

**Status**: ✅ Complete

---

### 4.6 Modifier System

**Core Files**: `ModifierDefinition.cs`, `StatModifierDefinition.cs`, `DoubleShotModifierDefinition.cs`, `ModifierInstance.cs`, `ModifierInventory.cs`, `ModifierCalculator.cs`, `ModifierStat.cs`, `ModifierOperation.cs`

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
| `ModifierPickup` | `IInteractable` → adds permanent modifier → destroys self |
| `TemporaryModifierPickup` | `IInteractable` → adds temporary modifier with duration → destroys self |

> Each modifier shows a **unique 16×16 icon** (see §1 → Sprite Canvas Specs — Weapons, Pickups & Projectiles). Both pickup prefabs currently reuse a single tinted square; replace it with a per-modifier sprite.

**Status**: ✅ Complete

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
- Events: `FirePressed`, `FireHeld`, `ReloadPressed`, `InteractPressed`
- `AimDevice` enum tracks Mouse vs Gamepad for aim source switching

**Status**: ✅ Complete

---

### 4.10 Enemy AI System

**Files**: `EnemyAIInput.cs`, `EnemyCombat.cs`, `EnemyWeaponDecision.cs`, `EnemyBehavior.cs`, `EnemyState.cs`

#### FSM States
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
| `EnemyAIInput` | Implements `IMoveInput` — calculates direction toward target |
| `EnemyCombat` | Range check (`Vector2.Distance`), calls `WeaponHolder.TryFire` |
| `EnemyWeaponDecision` | `GetClosestAvailableWeapon()` from `WeaponDetection`, `IsWeaponInPickupRange()`, `TryEquipNearbyWeapon()` |

**Status**: ✅ Complete

---

### 4.11 UI System

**File**: `WeaponAmmoUI.cs`

- Subscribes to `WeaponHolder.WeaponChanged` and `Weapon.AmmoChanged`
- Displays `CurrentAmmo / ReserveAmmo` or "∞ ∞" for infinite ammo
- Uses TextMeshPro `TextMeshProUGUI`

**Status**: ✅ Complete (minimal)

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

### 4.14 Room System (Planned)

> Reference: *The Binding of Isaac* — rooms are the core spatial unit.

**Files**: `RoomController.cs`, `RoomDoor.cs`, `RoomData.cs`, `RoomGenerator.cs`, `RoomType.cs`

#### Room Types
| Type | Behavior |
|------|----------|
| **Combat** | Spawns enemies. Exits lock. Clear all enemies to unlock. |
| **Item** | Contains a weapon or modifier pickup. No enemies. |
| **Boss** | Final room. Boss enemy with phases. Clears = map complete. |
| **Start** | Entry point. No enemies, no loot. |

#### Room Controller
- `RoomData` (ScriptableObject): room type, enemy wave definitions, loot tables
- `RoomController` (MonoBehaviour): tracks enemies alive, fires `RoomCleared` event
- Doors are `Collider2D` triggers — disabled when room is locked
- On player enter → lock doors, spawn enemies. On all enemies dead → unlock doors.

#### Map Structure
```
Start Room → Combat Room → Item Room → Combat Room → ... → Boss Room
```
- Map is a linear or branching sequence of rooms
- Rooms connected via door triggers
- Room layout is predefined (tilemap-based) — procedural generation is out of scope for v1

#### Room Generator (Future)
- If procedural generation is added later: `RoomGenerator` picks from a pool of room templates
- Templates are pre-built tilemap rooms with spawn points
- Generator connects them in a valid graph

**Status**: 🔲 Not started

---

### 4.15 Enemy Types (Planned)

> Reference: *The Binding of Isaac* — varied enemy behaviors create emergent difficulty.

**Base**: `EnemyBehavior` (existing FSM) extended with per-type subclasses.

| Type | Behavior | Attack Pattern |
|------|----------|----------------|
| **Chaser** | Moves directly toward player. Melee only. | Melee |
| **Shooter** | Keeps distance. Stops at optimal range and fires. | Ranged (projectile) |
| **Rusher** | Charges in a straight line when in range. Brief telegraph. Melee only. | Dash (charge) |
| **Floater** | Moves erratically (sine wave). Fires sporadically. | Ranged (slow, erratic) |
| **Tank** | Slow, high HP. Uses a heavy slow weapon with great damage and slow projectile speed. | Ranged (heavy, slow) |
| **Support** | Stays far. Buffs nearby enemies (speed/damage aura). | Passive aura |
| **Robber** | If player has weapon: rushes in, steals it, runs away, then shoots player with it after a few seconds. If player has no weapon: hides and stays as far as possible until player picks one up. | Adaptive (steal → ranged) |
| **Boss** | Multi-phase. Unique patterns per phase. Summons minions. | Mixed |

#### Implementation Approach
```
EnemyBehavior (base)        ← existing FSM skeleton
  ├── ChaserBehavior        ← override: seek + contact damage (melee only)
  ├── ShooterBehavior       ← override: maintain distance, fire
  ├── RusherBehavior        ← override: telegraph, charge, cooldown (melee only)
  ├── TankBehavior          ← override: slow approach, heavy slow projectile
  ├── RobberBehavior        ← override: steal weapon, flee, then shoot
  └── BossBehavior          ← override: phase system, minion spawning
```

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

- Each type is a `MonoBehaviour` that inherits from or composes `EnemyBehavior`
- `EnemyData` (ScriptableObject) per type: speed, HP, attack pattern, sprite, drop table
- `EnemyCombat` adapts per type (some use `WeaponHolder`, some use custom logic)

**Status**: 🔲 Not started

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
| **SampleScene** | ✅ Yes (index 0) | Active | Main gameplay — Player, Enemy, weapons, pickups, canvas |
| **Bootstrap** | ❌ No | Empty | Placeholder |
| **Gameplay** | ❌ No | Empty | Placeholder |
| **MainMenu** | ❌ No | Empty | Placeholder |

---

## 7. Prefabs

| Prefab | Description |
|--------|-------------|
| **Player** | Player entity shell with all components (100 HP, speed 5) |
| **Enemy** | Enemy entity shell with AI, combat, weapon detection |
| **Projectile** | Bullet — speed 15, auto-destroy 5s, trigger collider |
| **ProjectileWeapon** | Generic weapon prefab with WeaponPickup, visual/audio feedback |

---

## 8. Packages

### Core
| Package | Version | Purpose |
|---------|---------|---------|
| `com.unity.render-pipelines.universal` | 17.3.0 | URP 2D rendering |
| `com.unity.inputsystem` | 1.19.0 | New Input System |
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
| **Interface segregation** | `IMoveInput`, `IInteractable`, `IModifierProvider` — minimal contracts, multiple implementations |
| **Enemy AI as FSM** | Simple 3-state machine — easy to extend, easy to debug |
| **No `.asmdef` files** | Default `Assembly-CSharp` — simpler for a learning project of this scale |
| **New Input System only** | Legacy input disabled — clean, future-proof |
| **Room-based spatial loop** | Inspired by *The Binding of Isaac* — rooms are the core unit of progression |
| **Predefined room layouts** | v1 uses hand-crafted tilemap rooms — procedural generation deferred to Phase 7 |
| **Object pooling from start** | Avoids GC spikes — projectiles and entities reused, not instantiated/destroyed |
| **Enemy types via composition** | Each enemy type is a `MonoBehaviour` composing or extending base FSM — not a monolithic class |
| **Boss as room event** | Boss is a special room type, not a separate scene — keeps state consistent |

---

## 10. What Has Been Done — Status Matrix

| System | Status | Notes |
|--------|--------|-------|
| Health | ✅ Done | Core + Visual + Audio feedback |
| Movement | ✅ Done | Interface-driven, modifier-aware |
| Look Direction | ✅ Done | 2D rotation |
| Weapon Core | ✅ Done | Equip, Fire, Reload, Ammo, Cooldown |
| Weapon Strategies | ✅ Done | Melee + Projectile |
| Projectile | ✅ Done | Movement, Damage, Auto-destroy |
| Modifier System | ✅ Done | Add/Multiply pipeline, temporary, pickups |
| Interaction | ✅ Done | Trigger-based, closest selection |
| Weapon Pickup | ✅ Done | IInteractable integration |
| Weapon Detection | ✅ Done | Area-based weapon scanning |
| Player Input | ✅ Done | Mouse + Gamepad, IMoveInput |
| Enemy AI | ✅ Done | 3-state FSM, weapon seeking, combat |
| Weapon UI | ✅ Done | Ammo display (minimal) |
| Debug Tools | ✅ Done | Loggers + testers for all systems |
| Object Pool | ✅ Done | `Systems/Pool` — `ObjectPool<T>` + `PoolManager`; projectiles pooled (prewarm 32, grows on demand) |
| Room System | 🔲 Planned | Room types, doors, lock/clear mechanic |
| Map Structure | 🔲 Planned | Linear room sequence with transitions |
| Enemy Types | 🔲 Planned | Chaser, Shooter, Rusher, etc. |
| Boss System | 🔲 Planned | Multi-phase boss encounter |
| Game State | 🔲 Planned | Run/Paused/GameOver state machine |
| Scene Flow | 🔲 Planned | Bootstrap → Menu → Gameplay |
| Main Menu | 🔲 Planned | Start, Settings, Quit |
| Settings | 🔲 Planned | Volume, Resolution, Fullscreen |
| Audio Manager | 🔲 Planned | BGM + SFX layers |
| HUD | 🔲 Planned | Health, Timer, Weapon, Modifiers |
| Death Handling | 🔴 Blocked | Player + Enemy death logic needed |

---

## 11. What's Missing — Identified Gaps

> All gaps below are derived from the target vision: a room-based roguelite in the style of *The Binding of Isaac* and *Enter The Gungeon*.

### 10.1 Core Gameplay Gaps

| Gap | Priority | Description |
|-----|----------|-------------|
| **Player Death Handling** | 🔴 High | `Health.Died` fires but nothing consumes it — no game over, no run end |
| **Enemy Death Handling** | 🔴 High | No destroy, no loot drop, no death feedback |
| **Room System** | 🔴 High | No rooms, no doors, no lock/clear mechanic — the core spatial loop is missing |
| **Room Types** | 🔴 High | No combat rooms, item rooms, or boss room |
| **Enemy Spawner** | 🔴 High | Enemies manually placed — no wave/room-based spawning |
| **Enemy Types** | 🔴 High | Only one generic enemy — no behavioral variety |
| **Boss Enemy** | 🔴 High | No final boss, no multi-phase fight |
| **Health Pickup** | 🟡 Medium | `Health.Heal()` exists but nothing triggers it |
| **Drop System** | 🟡 Medium | Enemies don't drop weapons/modifiers on death |

### 10.2 Map & Flow Gaps

| Gap | Priority | Description |
|-----|----------|-------------|
| **Game State Manager** | 🔴 High | No singleton managing Run / Paused / GameOver states |
| **Scene Flow** | 🔴 High | Bootstrap, MainMenu, Gameplay scenes empty — no scene loading |
| **Main Menu** | 🔴 High | No main menu UI |
| **Run Manager** | 🔴 High | No run timer, no run state tracking |
| **Map Structure** | 🔴 High | No room sequence — no start → combat → item → boss flow |
| **Door / Lock System** | 🔴 High | No doors that lock on enter and unlock on room clear |
| **Game Over Screen** | 🔴 High | No death screen, no restart option |
| **Pause Menu** | 🟡 Medium | No pause functionality |
| **Settings Menu** | 🟡 Medium | No volume, resolution, or display settings |

### 10.3 UI Gaps

| Gap | Priority | Description |
|-----|----------|-------------|
| **Health Bar** | 🔴 High | No player health display |
| **Run Timer** | 🔴 High | No elapsed time display |
| **Ammo Display** | 🟡 Medium | Existing `WeaponAmmoUI` is minimal — needs weapon icon, name |
| **Modifier HUD** | 🟡 Medium | No active modifier display |
| **Minimap** | 🟢 Low | No room/level overview |

### 10.4 Audio Gaps

| Gap | Priority | Description |
|-----|----------|-------------|
| **Audio Manager** | 🔴 High | No global audio management — individual AudioSources only |
| **Music System** | 🔴 High | No background music |
| **Room SFX** | 🟡 Medium | No door lock/unlock sounds, no room clear jingle |
| **Enemy SFX** | 🟡 Medium | No enemy-specific attack/death sounds |
| **Boss SFX** | 🟡 Medium | No boss entrance, phase transition, death sounds |

### 10.5 Visual Gaps

| Gap | Priority | Description |
|-----|----------|-------------|
| **Player Sprite** | ✅ Done | 18×26 px body, 16 PPU, Point filter (`Player 1.png`) — see §13 |
| **Enemy Sprites** | 🔴 High | Red placeholder square — one sprite for all types |
| **Room Tilemap** | 🔴 High | No environment tiles — empty scenes |
| **Door Sprites** | 🟡 Medium | No visual door objects |
| **Animations** | 🟡 Medium | No sprite animations (2D Animation package imported, unused) |
| **Particle Effects** | 🟡 Medium | Muzzle flash is sprite swap only — no blood, sparks, etc. |
| **Screen Shake** | 🟡 Medium | No camera juice |
| **Damage Numbers** | 🟢 Low | No floating combat text |

### 10.6 Technical Gaps

| Gap | Priority | Description |
|-----|----------|-------------|
| **Scene Management** | 🔴 High | No `SceneManager` usage — no scene loading flow |
| **Unit Tests** | 🟡 Medium | `test-framework` imported — zero test files |
| **Save System** | 🟢 Low | No persistence |
| **Localization** | 🟢 Low | No i18n |

---

## 12. Roadmap — Work Plan

> Ordered by dependency. Each phase builds on the previous.
> Reference: *The Binding of Isaac* (room flow, item rooms, boss fights) / *Enter The Gungeon* (projectile density, dodge mechanics, weapon variety).

### Phase 1: Foundation (🔴 Critical)

> Core infrastructure that everything else depends on.

| # | Task | Description | Depends On |
|---|------|-------------|------------|
| 1.1 | **Object Pool** ✅ | Generic `ObjectPool<T>` + `IPoolable` interface. Pre-warm, get, return. | — |
| 1.2 | **Game State Manager** | Singleton: `MainMenu`, `Running`, `Paused`, `GameOver`. Scene-agnostic. | — |
| 1.3 | **Run Manager** | Tracks run timer, current room, run stats. Consumes GameState. | 1.2 |
| 1.4 | **Scene Flow** | Bootstrap → MainMenu → Gameplay. `SceneManager.LoadSceneAsync`. | 1.2 |
| 1.5 | **Main Menu UI** | Title, Start Game, Settings, Quit. | 1.4 |
| 1.6 | **Settings Menu** | Music volume, SFX volume, resolution dropdown, fullscreen toggle. | 1.4 |
| 1.7 | **Pause Menu** | ESC toggle. Resume, Settings, Quit to Menu. | 1.2 |
| 1.8 | **Audio Manager** | Singleton. BGM layer + SFX layer. Volume control via Settings. | 1.6 |

### Phase 2: Death & Feedback (🔴 Critical)

> Make entities die properly and give the player feedback.

| # | Task | Description | Depends On |
|---|------|-------------|------------|
| 2.1 | **Enemy Death Handling** | `Health.Died` → death animation, loot drop, destroy/return to pool | 1.1, 1.2 |
| 2.2 | **Player Death Handling** | `Health.Died` → disable input, death animation, GameOver state | 1.2 |
| 2.3 | **Health Bar UI** | Player health bar (slider or segmented). Binds to `Health.HealthChanged`. | — |
| 2.4 | **Run Timer UI** | Elapsed time display. Binds to RunManager. | 1.3 |
| 2.5 | **Weapon HUD** | Weapon icon, name, ammo/reserve, reload indicator. | — |
| 2.6 | **Game Over Screen** | Run stats (time, kills, rooms cleared), Restart, Main Menu buttons. | 2.2, 1.2 |
| 2.7 | **Projectile Pooling** ✅ | Refactor `ProjectileFireStrategy` to use `ObjectPool<Projectile>`. | 1.1 |
| 2.8 | **Screen Shake** | Camera shake on fire, hit, death. Coroutine-based. | — |

### Phase 3: Room System (🔴 Critical)

> The spatial backbone of the game.

| # | Task | Description | Depends On |
|---|------|-------------|------------|
| 3.1 | **RoomData (SO)** | Room type, dimensions, door positions, enemy wave definitions, loot table. | — |
| 3.2 | **RoomController** | Tracks enemies alive. `RoomCleared` event. Lock/unlock doors. | 1.2 |
| 3.3 | **RoomDoor** | Collider2D trigger. Locks on room enter, unlocks on room clear. | 3.2 |
| 3.4 | **Map Structure** | Linear room sequence: Start → Combat → Item → Combat → ... → Boss. | 3.1, 3.2 |
| 3.5 | **Room Transitions** | Player enters door → fade/transition → load next room. | 3.4, 1.4 |
| 3.6 | **Room Tilemap** | Base room template: walls, floor, door sockets. 16×16 tileset. | 3.1 |
| 3.7 | **Combat Room** | Spawns enemy waves on enter. Clears when all enemies dead. | 3.2, 2.1 |
| 3.8 | **Item Room** | Contains weapon or modifier pickup. No enemies. | 3.2 |
| 3.9 | **Start Room** | Entry point. No enemies, no loot. | 3.2 |

### Phase 4: Enemy Variety (🟡 Important)

> Different enemies create emergent gameplay.

| # | Task | Description | Depends On |
|---|------|-------------|------------|
| 4.1 | **EnemyData (SO)** | Per-type stats: speed, HP, damage, attack pattern, sprite, drop table. | — |
| 4.2 | **Chaser Enemy** | Moves toward player. Contact damage. Melee only. | 2.1 |
| 4.3 | **Shooter Enemy** | Maintains distance. Fires projectiles at player. | 2.1, 1.1 |
| 4.4 | **Rusher Enemy** | Telegraphs, then charges in a line. Melee only. Brief stun after. | 2.1 |
| 4.5 | **Robber Enemy** | Steals player weapon if equipped, flees, then shoots. Hides if player unarmed. | 2.1, 1.1 |
| 4.6 | **Enemy Spawner** | Room-level spawner. Spawns waves from `RoomData` definitions. | 3.7 |
| 4.7 | **Drop System** | Enemies drop weapons/modifiers/health on death based on `EnemyData` drop table. | 2.1 |
| 4.8 | **Health Pickup** | Consumable that heals player. Drops from enemies or found in item rooms. | 4.7, 3.8 |

### Phase 5: Boss & Progression (🟡 Important)

> The climax of each map run.

| # | Task | Description | Depends On |
|---|------|-------------|------------|
| 5.1 | **Boss Room** | Special room type. Triggers boss encounter on enter. | 3.2 |
| 5.2 | **Boss Enemy** | Multi-phase enemy. Unique attack patterns per phase. High HP. | 4.1 |
| 5.3 | **Boss Phases** | Phase transitions: HP thresholds trigger new attack patterns, telegraph, UI. | 5.2 |
| 5.4 | **Boss Drops** | Boss drops unique weapon/modifier on death. | 5.2, 4.6 |
| 5.5 | **Map Complete** | Boss death → victory screen → back to main menu. | 5.2, 2.6 |

### Phase 6: Polish & Juice (🟢 Nice to Have)

> Make it feel good — visuals, audio, game feel.

| # | Task | Description | Depends On |
|---|------|-------------|------------|
| 6.1 | **Placeholder Sprites** | Replace colored squares with temp pixel art for all entities. | — |
| 6.2 | **Sprite Animations** | Idle, walk, attack, death for player, enemies, boss. | 6.1 |
| 6.3 | **Room Tileset** | 16×16 tileset: walls, floors, doors, decorations. | 3.6 |
| 6.4 | **Music** | BGM per state: menu, combat, boss, item room. | 1.8 |
| 6.5 | **Enemy SFX** | Attack, death, and ambient sounds per enemy type. | 1.8, 4.2-4.4 |
| 6.6 | **Boss SFX** | Entrance, phase transition, death sounds. | 1.8, 5.2 |
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
| 7.4 | **Room Templates** | More room layouts for variety. | 3.6 |
| 7.5 | **Procedural Generation** | `RoomGenerator` picks from template pool, connects valid graphs. | 3.4 |
| 7.6 | **Meta-progression** | Persistent upgrades across runs. | 1.3 |
| 7.7 | **Shop System** | Spend currency on weapons/modifiers between rooms. | 4.6 |
| 7.8 | **Challenge Runs** | Difficulty modifiers (more enemies, less HP, harder bosses). | 7.6 |

---

## 13. Asset Inventory

### ScriptableObject Assets

| Category | Assets |
|----------|--------|
| **Weapons** | Pistol, Colt, Mosin, Katana |
| **Fire Strategies** | Melee, Projectile |
| **Modifiers** | DoubleDamage, DoubleFireRate, DoubleReload, DoubleSpeed, DoubleShot |

### Audio

| Category | Count | Examples |
|----------|-------|---------|
| **Combat & Gore** | 15 | bone_snap, crunch, punch, slap, squelching |
| **Weapons** | 16 | sword_clash, shot_muffled, weapon_equip, weapon_upgrade |

### Sprites

| Asset | Notes |
|-------|-------|
| `MuzzleFlash.png` | Weapon VFX |
| `Player 1.png` | Player entity — 18×26 px body @ 16 PPU |

> **Planned entity sprites** — canvas sizes defined in [§1 → Sprite Canvas Specs](#visual-specs): Chaser 32×32, Shooter/Rusher 32×32, Tank 48×48, Boss 64×64. All imported at 16 PPU, Point filter, no compression. (Enemy still uses the placeholder square — see gap 10.5.)
>
> **Planned item sprites** — weapon world sprites + UI icons, a unique 16×16 icon per modifier, and an 8×8 bullet (specs in [§1 → Sprite Canvas Specs — Weapons, Pickups & Projectiles](#visual-specs)).

### Prefabs

| Prefab | Purpose |
|--------|---------|
| Player | Player entity |
| Enemy | Enemy entity |
| Projectile | Bullet entity |
| ProjectileWeapon | Weapon pickup (generic) |

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
