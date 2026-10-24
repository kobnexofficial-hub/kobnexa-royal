# KOBNEXA ROYAL - Architecture (Missions 01 + 02)

## Scenes and startup flow

- `Bootstrap` - tiny entry scene. `GameBootstrap` ensures the core managers
  exist, applies saved settings, then loads `RoyalLobby` via `SceneLoader`.
- `RoyalLobby` - the hub scene. One root object (`RoyalLobbyRoot`) carries:
  GameBootstrap, SettingsManager, AudioManager, SceneLoader,
  LobbyEnvironmentBuilder, LobbyCharacterShowcase, RoyalEnergyParticles and
  LobbyUIManager. The Main Camera carries LobbyCameraController.
- `RoyalBattle` - the match scene. One root object (`RoyalBattleRoot`) carries:
  GameBootstrap, RoyalWorldBuilder, WeatherSystem, WorldZoneSystem,
  MatchManager, GameplayHUD; a `Player` object carries
  ThirdPersonPlayerController, HealthComponent, RoyalBlaster, MeleeWeapon and
  PlayerInventory; the scene also ships 3 loot chests, 2 weapon pickups and
  2 ground loot drops. It loads from the lobby PLAY flow through the same
  `SceneLoader`.

## Layers

- **Presentation (Lobby/)** - procedural environment (castle, islands,
  waterfalls, statues, banners, platform), hero showcase with idle/emote and
  accent schemes, cinematic camera, dust/star particles. Everything is built
  from primitives + generated materials so final art can replace pieces one
  by one without touching UI or logic.
- **UI (UI/)** - `UIFactory` (uGUI construction helpers, legacy font, no
  TextMeshPro dependency) and `LobbyUIManager` (builds the entire reference
  layout at runtime, owns navigation, dialogs, and all button behavior).
- **Logic (Core/, pure C#)** - `MenuNavigationStack` (Back history),
  `SettingsData` (clamped settings), `PlayerProfileData` (XP, levels,
  Crowns). These have no scene dependencies and are unit-tested.
- **Services (Core/, MonoBehaviour)** - `SettingsManager` (PlayerPrefs
  persistence + apply), `AudioManager` (assigned clips, or synthesized pad
  music and UI tones as a safe fallback), `SceneLoader`, `GameBootstrap`.
- **Gameplay (Gameplay/)** - Player (third-person controller), World
  (procedural terrain, castle keep, village, watchtowers, 3 biome zones),
  Weather (Clear/Rain/Storm), Combat (health/shields, blaster, melee,
  pickups), AI (12 named bots, wander/seek/attack), Match (zone shrink,
  kill feed, Victory/Defeat, results), Inventory (loot chests, ground loot,
  8-slot inventory), UI (gameplay HUD).
- **Cross-scene state (Core/)** - `RoyalGameState` (static): the lobby reads
  CurrentProfile/QuestProgress/Party; a match writes LastMatchResult and the
  lobby applies it once on return (double-apply guarded). Pure helpers:
  `CombatMath` (shield-first damage, zone radius), `MatchResultData` (XP
  rule), `QuestProgressData` (3 daily quests), `InventoryData`, `PartyData`
  (local parties only), `BotNames`.
- **Data (Data/)** - `PrototypeCatalog`: clearly labeled prototype content
  (daily quests from the reference, store items, battle pass tiers, locker
  items, friends, career stats). Real services will replace this provider
  later without changing the UI.

## Key design rules

- No invented APIs and no hard package dependencies beyond URP + uGUI +
  Test Framework (all in `Packages/manifest.json`).
- Missing-asset fallbacks everywhere: shaders (URP Lit -> Standard), audio
  (assigned -> synthesized), EventSystem (created at runtime), settings
  (corrupt JSON -> defaults).
- Honest prototypes: anything not backed by a real service is labeled a
  prototype in the UI, PLAY never pretends to matchmake online, and
  parties are local only (online matchmaking is NOT built - it requires
  a live backend service).
- Palette lives in one place: `RoyalColors` (black, deep navy, royal gold,
  royal blue), shared by 3D materials and UI.

## Rendering

URP is the target pipeline for broad hardware compatibility. Until a URP
pipeline asset is created in the Editor (see SETUP troubleshooting), materials
are created with whichever of URP Lit / Standard is available, and both use
the same property names for metallic/smoothness/emission in this codebase.
