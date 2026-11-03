# Script Index

Every C# script in the project, grouped by what it does, with a one-line explanation.

Use this when you want to change something and need to know **which file to open**. For deeper explanations of how the pieces interact, see `ARCHITECTURE.md` and `docs/ROOMS.md`.

- **Namespace root**: `Assets/Scripts/...` (folders mirror namespaces)
- **Scripts**: 106 (105 handwritten + 1 auto-generated: `PlayerInputActions.cs`)
- **Last updated**: 2026-11-03

---

## Index

| # | Section |
|---|---|
| — | [Quick answers — "I want to change..."](#quick-answers--i-want-to-change) |
| 1 | [Entry Point & Game Flow](#1-entry-point--game-flow) |
| 2 | [Settings & Audio](#2-settings--audio) |
| 3 | [Player](#3-player) |
| 4 | [Player Input](#4-player-input) |
| 5 | [Core Reusable Components](#5-core-reusable-components) |
| 6 | [Health & Damage](#6-health--damage) |
| 7 | [Weapons](#7-weapons) |
| 8 | [Projectiles](#8-projectiles) |
| 9 | [Modifiers (stat upgrades & power-ups)](#9-modifiers-stat-upgrades--power-ups) |
| 10 | [Weapon Pickup & Detection](#10-weapon-pickup--detection) |
| 11 | [Enemies](#11-enemies) |
| | ↳ [Enemy entity shells](#111-enemy-entity-shells) · [Enemy AI](#112-enemy-ai) · [Enemy setup, stats & rewards](#113-enemy-setup-stats--rewards) |
| 12 | [Rooms](#12-rooms) |
| 13 | [Map & Dungeon Flow](#13-map--dungeon-flow) |
| 14 | [HUD](#14-hud) |
| 15 | [Menus & Settings UI](#15-menus--settings-ui) |
| 16 | [Room Transition UI](#16-room-transition-ui) |
| 17 | [Object Pooling](#17-object-pooling) |
| 18 | [Debug](#18-debug) |
| 19 | [Interfaces (contracts)](#19-interfaces-contracts) |
| — | [Notes](#notes) |

---

## Quick answers — "I want to change..."

| I want to change... | Go to |
|---|---|
| Weapon damage, fire rate, ammo, reload | `Assets/Data/Weapons/*` (data) → then `WeaponData.cs` |
| How a weapon shoots (melee vs projectile) | `MeleeFireStrategy.cs`, `ProjectileFireStrategy.cs` |
| Enemy HP, speed, attack range per type | `Assets/Data/Enemies/*` → then `EnemyData.cs` |
| What each enemy type decides to do | `MeleeBehavior.cs`, `ShooterBehavior.cs`, `RobberBehavior.cs` |
| The shared enemy FSM (chase / seek / attack) | `EnemyBehavior.cs` |
| Modifier effects (damage ×2, double shot...) | `Assets/Data/Modifiers/*` → then `StatModifierDefinition.cs`, `DoubleShotModifierDefinition.cs` |
| Stat calculation order (Add then Multiply) | `ModifierCalculator.cs` |
| Which enemies spawn in a room | `Assets/Data/Rooms/*` → then `RoomData.cs`, `RoomEnemySpawner.cs` |
| Room locking / clearing rules | `RoomController.cs` |
| Dungeon layout and door connections | `Assets/Data/Maps/*` → then `MapData.cs`, `RoomDoorLink.cs`, `DungeonManager.cs` |
| Room transition fade + teleport | `DungeonManager.cs`, `RoomTransitionOverlay.cs` |
| Player controls / bindings | `PlayerInputActions.inputactions` (asset) → `PlayerInput.cs` |
| Game states, pause, win/lose flow | `GameManager.cs`, `GameState.cs`, `RunManager.cs` |
| HUD (health, ammo, timer) | `Scripts/UI/HUD/*` |
| Menus, settings, game over screen | `Scripts/UI/Menus/*` |
| Volume / resolution persistence | `GameSettings.cs`, `AudioManager.cs` |
| Projectile pooling | `PoolManager.cs`, `ObjectPool.cs`, `Projectile.cs` |
| Player death behavior | `PlayerDeathHandler.cs`, `DestroyOnDeath.cs` |

---

## 1. Entry Point & Game Flow

Owns the game's lifecycle: state, run stats, scene loading, pause.

| Script | Path | What it does |
|---|---|---|
| `Bootstrap` | `Scripts/Systems/Game/Bootstrap.cs` | Starting scene entry point. Loads settings, starts music, sets `MainMenu` state and loads the main menu. |
| `GameManager` | `Scripts/Systems/Game/GameManager.cs` | Persistent singleton holding the current `GameState`. Only place that changes `Time.timeScale`. Handles pause toggle. |
| `GameState` | `Scripts/Systems/Game/GameState.cs` | Enum: `MainMenu`, `Running`, `Paused`, `GameOver`, `Win`. |
| `RunManager` | `Scripts/Systems/Game/RunManager.cs` | Tracks one run: elapsed time, kills, rooms cleared. Fires `RunStarted` / `RunEnded`. |
| `RunStats` | `Scripts/Systems/Game/RunStats.cs` | Immutable data packet (time, kills, rooms) passed when a run ends. |
| `SceneLoader` | `Scripts/Systems/Game/SceneLoader.cs` | Persistent async scene loader with progress and `SceneLoaded` events. Fails fast if the scene is not in Build Settings. |
| `SceneId` | `Scripts/Systems/Game/SceneId.cs` | Enum of playable scenes: `Bootstrap`, `MainMenu`, `Gameplay`. |
| `PauseInput` | `Scripts/Systems/Game/PauseInput.cs` | Persistent listener for the `Pause` input action; raises `PausePressed`. |

---

## 2. Settings & Audio

| Script | Path | What it does |
|---|---|---|
| `GameSettings` | `Scripts/Systems/Settings/GameSettings.cs` | Static, `PlayerPrefs`-backed store for music/SFX volume, fullscreen and resolution. Raises change events. |
| `AudioManager` | `Scripts/Systems/Audio/AudioManager.cs` | Persistent audio service: one music source, one SFX source, volumes routed through the audio mixer. Reacts to `GameSettings` changes. |

---

## 3. Player

| Script | Path | What it does |
|---|---|---|
| `Player` | `Scripts/Entities/PlayerRelated/Player.cs` | Thin player shell. Reads input, aims (mouse or stick), fires while held, auto-reloads when the magazine is empty. |
| `PlayerDeathHandler` | `Scripts/Entities/PlayerRelated/PlayerDeathHandler.cs` | On player death: ends the run and switches game state to `GameOver`. |

---

## 4. Player Input

| Script | Path | What it does |
|---|---|---|
| `PlayerInput` | `Scripts/Components/PlayerInput/PlayerInput.cs` | Bridge to Unity Input System. Exposes move vector, aim data, fire-held state and `FirePressed` / `ReloadPressed` / `InteractPressed` events. Also implements `IMoveInput`. |
| `AimDevice` | `Scripts/Components/PlayerInput/AimDevice.cs` | Enum: `Mouse` or `Gamepad`. Tells the player which aim source to use. |
| `PlayerInputActions` | `PlayerInputActions.cs` | **Auto-generated** by the Input System from `PlayerInputActions.inputactions`. Do not edit by hand — change the `.inputactions` asset and let Unity regenerate it. |

---

## 5. Core Reusable Components

Domain-agnostic building blocks shared by player and enemies.

| Script | Path | What it does |
|---|---|---|
| `Movement` | `Scripts/Components/Base/Movement.cs` | Moves a `Rigidbody2D` using any `IMoveInput`, with speed values passed through the modifier pipeline. |
| `LookDirection` | `Scripts/Components/Base/LookDirection.cs` | Stores a normalized facing direction and rotates the transform toward it (2D Z rotation). |
| `Interaction` | `Scripts/Components/Base/Interaction.cs` | Trigger-based radar that tracks nearby `IInteractable` objects and interacts with the closest one. |

---

## 6. Health & Damage

| Script | Path | What it does |
|---|---|---|
| `Health` | `Scripts/Components/Base/HealthRelated/Health.cs` | Core HP container: `TakeDamage`, `Heal`, `RestoreFullHealth`, plus `DamageTaken` / `HealthChanged` / `Died` events. |
| `DestroyOnDeath` | `Scripts/Components/Base/HealthRelated/DestroyOnDeath.cs` | On death, drops any held weapon and destroys the GameObject. |
| `HealthVisualFeedback` | `Scripts/Components/Base/HealthRelated/HealthVisualFeedback.cs` | Flashes the sprite red briefly when damage is taken. |
| `HealthAudioFeedback` | `Scripts/Components/Base/HealthRelated/HealthAudioFeedback.cs` | Plays damage and death sound clips through `AudioManager`. |

---

## 7. Weapons

Everything about holding, shooting, ammo, and weapon data.

| Script | Path | What it does |
|---|---|---|
| `Weapon` | `Scripts/Components/Weapons/Weapon.cs` | Core weapon runtime: fire cooldown, ammo, reload timing, modifier application, then delegates the actual shot to its fire strategy. |
| `WeaponHolder` | `Scripts/Components/Weapons/WeaponHolder.cs` | Inventory slot (socket) for one weapon: equip, drop, reload, fire. Raises `WeaponChanged`. |
| `WeaponData` | `Scripts/Components/Weapons/WeaponData.cs` | ScriptableObject with weapon stats (name, icon, damage, fire rate, magazine, reload, reserve, strategy, ammo type). |
| `WeaponFireStrategy` | `Scripts/Components/Weapons/WeaponFireStrategy.cs` | Abstract ScriptableObject base for "how this weapon shoots". |
| `MeleeFireStrategy` | `Scripts/Components/Weapons/MeleeFireStrategy.cs` | Melee shot: overlaps a circle at reach, supports spread, hits each target only once (no friendly fire for enemies). |
| `WeaponFireContext` | `Scripts/Components/Weapons/WeaponFireContext.cs` | Data passed to a fire strategy: weapon, direction, number of attacks, spread angle. Modified by modifiers before firing. |
| `WeaponFireModifierDefinition` | `Scripts/Components/Weapons/WeaponFireModifierDefinition.cs` | Base for modifiers that change a shot (attack count, spread), not a stat. |
| `WeaponModifierLoadout` | `Scripts/Components/Weapons/WeaponModifierLoadout.cs` | On spawn, applies a preset list of modifiers to the weapon — used to create pre-modded weapon variants. |
| `WeaponAudioFeedback` | `Scripts/Components/Weapons/WeaponAudioFeedback.cs` | Plays fire, reload-start and reload-complete sounds. |
| `WeaponVisualFeedback` | `Scripts/Components/Weapons/WeaponVisualFeedback.cs` | Shows the muzzle flash sprite briefly when the weapon fires. |
| `WeaponAmmoType` | `Scripts/Components/Weapons/Enums/WeaponAmmoType.cs` | Enum: `Magazine` (uses ammo/reload) or `Infinite` (melee-style). |
| `WeaponFireResult` | `Scripts/Components/Weapons/Enums/WeaponFireResult.cs` | Result of a fire attempt: `Success`, `NotEquipped`, `Cooldown`, `NoAmmo`, `Reloading`. |

---

## 8. Projectiles

| Script | Path | What it does |
|---|---|---|
| `Projectile` | `Scripts/Components/Projectile/Projectile.cs` | Pooled bullet: flies in a direction, damages `Health` on hit, ignores its owner and allied enemies, returns to the pool after 5 seconds. |
| `ProjectileFireStrategy` | `Scripts/Components/Projectile/ProjectileFireStrategy.cs` | Spawns one pooled projectile per attack, applying the spread angle. |

---

## 9. Modifiers (stat upgrades & power-ups)

| Script | Path | What it does |
|---|---|---|
| `ModifierDefinition` | `Scripts/Components/Modifiers/ModifierDefinition.cs` | Abstract ScriptableObject base for every modifier. |
| `StatModifierDefinition` | `Scripts/Components/Modifiers/StatModifierDefinition.cs` | A modifier that adds or multiplies a `ModifierStat` (e.g. damage ×2). Authored as an asset. |
| `DoubleShotModifierDefinition` | `Scripts/Components/Modifiers/DoubleShotModifierDefinition.cs` | Fire modifier: +1 attack and +15° spread, producing a double shot. |
| `ModifierInstance` | `Scripts/Components/Modifiers/ModifierInstance.cs` | A live copy of a modifier definition inside an inventory; tracks temporary expiry and refresh. |
| `ModifierInventory` | `Scripts/Components/Modifiers/ModifierInventory.cs` | Holds a character's or weapon's active modifiers, expires temporary ones, raises add/remove events, calculates modified stats. |
| `ModifierCalculator` | `Scripts/Components/Modifiers/ModifierCalculator.cs` | The math: applies all `Add` modifiers first, then all `Multiply` ones, across multiple providers. |
| `ModifierStat` | `Scripts/Components/Modifiers/ModifierStat.cs` | Enum of modifiable stats: `Damage`, `FireRate`, `ReloadDuration`, `MovementSpeed`. |
| `ModifierOperation` | `Scripts/Components/Modifiers/ModifierOperation.cs` | Enum: `Add` or `Multiply`. |
| `ModifierPickup` | `Scripts/Components/Modifiers/ModifierPickup.cs` | World pickup: gives a permanent modifier on interact, then destroys itself. |
| `TemporaryModifierPickup` | `Scripts/Components/Modifiers/TemporaryModifierPickup.cs` | Same as above, but the modifier expires after a set duration. |

---

## 10. Weapon Pickup & Detection

| Script | Path | What it does |
|---|---|---|
| `WeaponPickup` | `Scripts/Components/Base/Weapon/WeaponPickup.cs` | Makes a loose weapon interactable so the player can pick it up. |
| `WeaponDetection` | `Scripts/Components/Base/Weapon/WeaponDetection.cs` | Trigger radar that tracks nearby loose weapons (used by enemies looking for a gun). |

---

## 11. Enemies

Enemy shells, AI, combat, movement helpers and data.

### 11.1 Enemy entity shells

| Script | Path | What it does |
|---|---|---|
| `Enemy` | `Scripts/Entities/EnemyRelated/Enemy.cs` | Baseline enemy shell: just requires all the generic AI components. |
| `MeleeEnemy` | `Scripts/Entities/EnemyRelated/MeleeEnemy.cs` | Shell that requires `MeleeBehavior` (melee weapons only). |
| `ShooterEnemy` | `Scripts/Entities/EnemyRelated/ShooterEnemy.cs` | Shell that requires `ShooterBehavior` (projectile weapons, keeps distance). |
| `RobberEnemy` | `Scripts/Entities/EnemyRelated/RobberEnemy.cs` | Shell that requires `RobberBehavior` (hides, steals the player's weapon, runs, then shoots). |

### 11.2 Enemy AI

| Script | Path | What it does |
|---|---|---|
| `EnemyBehavior` | `Scripts/Components/EnemyComponents/EnemyBehaviorRelated/EnemyBehavior.cs` | The shared enemy FSM: Chasing → SeekingWeapon → Attacking. Subclasses override specific decisions. |
| `EnemyState` | `Scripts/Components/EnemyComponents/EnemyBehaviorRelated/EnemyState.cs` | Enum of AI states (`Chasing`, `SeekingWeapon`, `Attacking`, `Hiding`, `Stealing`, `Fleeing`). |
| `MeleeBehavior` | `Scripts/Components/EnemyComponents/EnemyBehaviorRelated/MeleeBehavior.cs` | Accepts only melee weapons. |
| `ShooterBehavior` | `Scripts/Components/EnemyComponents/EnemyBehaviorRelated/ShooterBehavior.cs` | Accepts only projectile weapons and retreats while attacking or unarmed. |
| `RobberBehavior` | `Scripts/Components/EnemyComponents/EnemyBehaviorRelated/RobberBehavior.cs` | Custom FSM: hide when the player is unarmed, rush and steal the player's weapon, flee, then attack with it. |
| `EnemyAIInput` | `Scripts/Components/EnemyComponents/EnemyAIInput.cs` | `IMoveInput` for AI: walks toward/away from a target and passes the direction through obstacle avoidance. |
| `EnemyCombat` | `Scripts/Components/EnemyComponents/EnemyCombat.cs` | Range and line-of-sight checks, then fires the held weapon. |
| `EnemyWeaponDecision` | `Scripts/Components/EnemyComponents/EnemyWeaponDecision.cs` | Finds the closest usable weapon on the floor and equips it when in reach. |
| `EnemyTargetAssigner` | `Scripts/Components/EnemyComponents/EnemyTargetAssigner.cs` | On spawn, automatically points the AI and combat at the player. |
| `ObstacleAvoidance` | `Scripts/Components/EnemyComponents/ObstacleAvoidance.cs` | "Whisker" steering: if the path is blocked, tries left/right angles and turns along walls. |
| `ObstacleRaycast` | `Scripts/Components/EnemyComponents/ObstacleRaycast.cs` | Static helper: is the path clear? Ignores triggers and anything with `Health` (so allies don't block). |

### 11.3 Enemy setup, stats & rewards

| Script | Path | What it does |
|---|---|---|
| `EnemyData` | `Scripts/Components/EnemyComponents/EnemyData.cs` | ScriptableObject with per-type stats: name, sprite, max HP, move speed, attack range. |
| `EnemyDataApplier` | `Scripts/Components/EnemyComponents/EnemyDataApplier.cs` | Pushes an `EnemyData` asset into `Health`, `Movement`, `EnemyCombat` and the sprite on Awake. |
| `EnemyStartingWeapon` | `Scripts/Components/EnemyComponents/EnemyStartingWeapon.cs` | Instantiates and equips a starting weapon on spawn. |
| `EnemyKillCounter` | `Scripts/Components/EnemyComponents/EnemyKillCounter.cs` | Adds +1 kill to `RunManager` when this enemy dies. |
| `PlayerHealOnKill` | `Scripts/Components/EnemyComponents/PlayerHealOnKill.cs` | Heals the player when this enemy dies (lifesteal-style reward). |

---

## 12. Rooms

One room's contents: state, doors, spawning, enemy tracking.

| Script | Path | What it does |
|---|---|---|
| `RoomController` | `Scripts/Components/Rooms/RoomController.cs` | The room brain: `Idle → Active → Cleared`. Locks doors on entry, tracks enemies, unlocks and reports the clear; ends the run for a `Final` room. |
| `RoomData` | `Scripts/Components/Rooms/RoomData.cs` | ScriptableObject describing a room: type, dimensions, enemy spawn list, possible item pickups. |
| `RoomType` | `Scripts/Components/Rooms/RoomType.cs` | Enum: `Start`, `Combat`, `Item`, `Final`. |
| `RoomDoor` | `Scripts/Components/Rooms/RoomDoor.cs` | One door: direction, blocker collider, open/locked sprites. Raises `PlayerEntered` when the player walks through. |
| `DoorDirection` | `Scripts/Components/Rooms/DoorDirection.cs` | Enum: `North`, `East`, `South`, `West`. |
| `RoomEnemy` | `Scripts/Components/Rooms/RoomEnemy.cs` | Registers an enemy with its parent room and reports its death so the room can clear. |
| `RoomEnemySpawner` | `Scripts/Components/Rooms/RoomEnemySpawner.cs` | On room activation, spawns the enemies listed in `RoomData` at enemy spawn points. |
| `RoomItemSpawner` | `Scripts/Components/Rooms/RoomItemSpawner.cs` | On room activation, spawns one random item pickup from `RoomData`. |
| `RoomSpawnPoint` | `Scripts/Components/Rooms/RoomSpawnPoint.cs` | Marker component for where enemies or items may appear. |
| `SpawnPointKind` | `Scripts/Components/Rooms/SpawnPointKind.cs` | Enum: `Enemy` or `Item`. |
| `EnemySpawnEntry` | `Scripts/Components/Rooms/EnemySpawnEntry.cs` | Serializable pair: enemy prefab + how many to spawn. |
| `RoomDebugger` | `Scripts/Components/Rooms/RoomDebugger.cs` | Logs room activated/cleared events. Debug only. |

---

## 13. Map & Dungeon Flow

How rooms are placed, connected, shown and traversed.

| Script | Path | What it does |
|---|---|---|
| `DungeonManager` | `Scripts/Components/Map/DungeonManager.cs` | Builds the door connection table, instantiates/caches rooms, teleports the player and runs the fade transition between rooms. |
| `MapData` | `Scripts/Components/Map/MapData.cs` | ScriptableObject: the ordered list of rooms that make up one dungeon layout. |
| `MapRoomEntry` | `Scripts/Components/Map/MapRoomEntry.cs` | One map entry: room data, room prefab, world position and its door links. |
| `RoomDoorLink` | `Scripts/Components/Map/RoomDoorLink.cs` | A connection: "this door leads to that room via that door". Can be one-way. |
| `RoomCamera` | `Scripts/Components/Map/RoomCamera.cs` | Snaps the camera to the center of the active room (no player-follow). |

---

## 14. HUD

| Script | Path | What it does |
|---|---|---|
| `PlayerHealthBarUI` | `Scripts/UI/HUD/PlayerHealthBarUI.cs` | Drives the health `Slider` from the player's `Health` events. |
| `WeaponHudUI` | `Scripts/UI/HUD/WeaponHudUI.cs` | Shows weapon icon, name, ammo/reserve (or ∞) and a reload indicator. |
| `RunTimerUI` | `Scripts/UI/HUD/RunTimerUI.cs` | Displays the run time as `mm:ss`. |
| `WeaponAmmoUI` | `Scripts/UI/WeaponAmmoUI/WeaponAmmoUI.cs` | Legacy minimal ammo text (used in the sample scene). Superseded by `WeaponHudUI`. |

---

## 15. Menus & Settings UI

| Script | Path | What it does |
|---|---|---|
| `MainMenuController` | `Scripts/UI/Menus/MainMenuController.cs` | Main menu: New Game, open Settings, Quit. Hides menu items while settings are open. |
| `PauseMenuController` | `Scripts/UI/Menus/PauseMenuController.cs` | Pause menu: Resume, Settings, Quit to Menu. Routes the pause key and closes settings first. |
| `SettingsPanelController` | `Scripts/UI/Menus/SettingsPanelController.cs` | Shared settings panel: music/SFX sliders, fullscreen toggle, resolution dropdown. Raises `Opened` / `Closed`. |
| `GameOverScreen` | `Scripts/UI/Menus/GameOverScreen.cs` | End-of-run overlay (win or lose): shows time, kills, rooms; Restart and Main Menu buttons. |
| `QuitGame` | `Scripts/UI/QuitGame.cs` | Static quit helper: stops Play mode in the Editor, quits the app in a build. |

---

## 16. Room Transition UI

| Script | Path | What it does |
|---|---|---|
| `RoomTransitionOverlay` | `Scripts/UI/RoomTransition/RoomTransitionOverlay.cs` | Black screen fade (`FadeOut` / `FadeIn`) that hides room swaps and blocks clicks between rooms. |

---

## 17. Object Pooling

| Script | Path | What it does |
|---|---|---|
| `PoolManager` | `Scripts/Systems/Pool/PoolManager.cs` | Scene singleton that registers prefabs (with prewarm counts) and hands out / takes back pooled instances. |
| `ObjectPool<T>` | `Scripts/Systems/Pool/ObjectPool.cs` | Generic pool: stack of available objects, set of in-use objects, double-return protection. |
| `IPoolable` | `Scripts/Systems/Pool/IPoolable.cs` | Interface with `OnGetFromPool` / `OnReturnToPool` callbacks for objects that need reset logic. |

---

## 18. Debug

| Script | Path | What it does |
|---|---|---|
| `HealthDebugger` | `Scripts/Debug/HealthDebugger.cs` | Logs health changes, damage and death. |
| `HealthTester` | `Scripts/Debug/HealthTester.cs` | Right-click context menu actions to damage, heal or fully restore health. |
| `WeaponDebugger` | `Scripts/Debug/WeaponDebugger.cs` | Logs fire, reload start/complete and ammo changes. |
| `ModifierDebugger` | `Scripts/Debug/ModifierDebugger.cs` | Logs added/removed modifiers with stat, operation, value and remaining duration. |

---

## 19. Interfaces (contracts)

| Script | Path | What it does |
|---|---|---|
| `IMoveInput` | `Scripts/Interfaces/IMoveInput.cs` | Anything that can supply a movement vector (player input, enemy AI). |
| `IInteractable` | `Scripts/Interfaces/IInteractable.cs` | Anything the interaction radar can trigger (`Interact(GameObject)`). |
| `IModifierProvider` | `Scripts/Interfaces/IModifierProvider.cs` | Anything that exposes a list of active modifiers (inventory). |
| `IEnemy` | `Scripts/Interfaces/IEnemy.cs` | Marker interface used to tell enemies apart (friendly-fire checks). |

---

## Notes

- ScriptableObjects (`WeaponData`, `EnemyData`, `ModifierDefinition`, `RoomData`, `MapData`, `WeaponFireStrategy`) hold **data**, not logic — most balance changes belong in their `.asset` files under `Assets/Data/`, not in code.
- `PlayerInputActions.cs` is auto-generated. Edit `Assets/PlayerInputActions.inputactions` instead.
- Always play from the `Bootstrap` scene: the persistent managers (`GameManager`, `RunManager`, `SceneLoader`, `AudioManager`, `PauseInput`) are created there.
