# KOBNEXA ROYAL - QA Report (Missions 01 + 02)

## Honest status first

This project was **built and verified on a Linux VM with no Unity Editor and
no .NET toolchain installed**. It has **NOT** been compiled, opened, or
play-tested inside Unity yet. All checks below are structural/static. The
first-open checklist in `SETUP_AND_LAUNCH.md` (import, Rebuild All Scenes,
press Play, run Test Runner) is the remaining verification step and must
happen in the Unity Editor (a separate install agent is working on that on
this same computer).

## Verified here - Mission 02 full-game pass (automated, 2026-10-24)

A Python verification pass over the complete project returned **ALL PASS**
(one flagged item was a false positive: Unity's built-in spot-cookie GUID
`0000000000000000e000000000000000`, present in the original lobby scene too,
correctly excluded from script-reference resolution):

- JSON/asmdef parse: **6 files** - all valid JSON.
- C# syntax balance: **40 scripts**, every file has balanced braces/parens/brackets
  (string- and comment-aware scan).
- Asset metas: every `.cs` and `.unity` file has a `.meta`; zero missing.
- Scene references: RoyalLobby **9**, RoyalBattle **14**, Bootstrap **1**
  script GUIDs - every one resolves to a real `.meta` file.
- Required files: **21/21 present** (all 3 scenes, player, world, weather,
  combat, AI, match, inventory, loot, HUD, tests, core state, manifest, version).
- Cross-script API check: **40 APIs** checked (combat, zone, inventory, quest,
  party, match, audio, lobby) - every called method exists in its target class.
- Editor builder check: Rebuild All Scenes includes RoyalBattle, and the
  battle scene builder adds a playable Player (controller + health + blaster +
  melee + inventory), MatchManager, GameplayHUD, 3 loot chests, 2 weapon
  pickups and 2 ground loot drops.
- Lobby PLAY check: PLAY loads `RoyalBattle` via SceneLoader, with the old
  countdown sequence kept as fallback if the scene is not in Build Settings.
- EditMode tests written: **38 total** (12 lobby + 26 gameplay) covering
  navigation, settings, profile, damage/shield math, zone shrink math,
  inventory, quest progress, match XP, and parties. NOT yet executed - they
  must be run in Unity's Test Runner.
- Known coexistence note: MatchManager builds a minimal live HUD and
  GameplayHUD builds the full HUD; both are null-safe and GameplayHUD places
  its readouts below MatchManager's blocks. One HUD should own match status
  in a future polish pass.
- Known pre-existing risk (unchanged): hand-written `.unity` YAML may be
  normalized by Unity 6 on first import; supported fallback is
  KOBNEXA ROYAL > Rebuild All Scenes.


## Honest status first

This project was **built and verified on a Linux VM with no Unity Editor and
no .NET toolchain installed**. It has **NOT** been compiled, opened, or
play-tested inside Unity yet. All checks below are structural/static. The
first-open checklist in `SETUP_AND_LAUNCH.md` (import, Rebuild All Scenes,
press Play, run Test Runner) is the remaining verification step and must
happen in Wilno's Unity Editor.

## Verified here (automated, 2026-10-24)

A Python verification pass over the project returned **ALL PASS**:

- JSON/asmdef parse: **4 files** (Packages/manifest.json + 3 asmdefs) - all valid JSON.
- C# syntax balance: **17 scripts**, every file has balanced braces/parens/brackets
  (string- and comment-aware scan).
- Asset metas: **20** scripts/asmdefs checked - every one has a `.meta`;
  **23 meta GUIDs total, zero duplicates**.
- Scene references: **9 script GUIDs** referenced by the hand-written
  RoyalLobby/Bootstrap scenes - every one resolves to a real `.meta` file.
- Required files: **16/16 present** (scenes, UI, editor builder, tests, docs, reference image).
- RoyalLobby scene carries all **9 lobby systems** (GameBootstrap, environment,
  hero, UI, particles, camera controller, audio, settings, scene loader);
  Bootstrap carries GameBootstrap.
- Both scenes contain the standard OcclusionCulling / RenderSettings /
  Lightmap / NavMesh header objects.
- EditorBuildSettings lists both scenes with GUIDs matching the scene `.meta` files.
- Cross-script API check: every method the UI calls on other systems
  (PlayEmote, SetAccentScheme, SettingsManager, AudioManager, navigation,
  catalog) exists in the target class.
- Project size: **61 files** (excluding two build-helper JSON files that are
  not part of the shipped zip).

## Implemented and ready to test in the Editor

- All 8 nav sections open as overlay panels with real catalog content;
  Back button and Escape both unwind the navigation history.
- PLAY shows an honest local-prototype confirmation (no fake matchmaking),
  then a 3-2-1 start sequence.
- Store purchases spend local prototype Crowns and persist per session;
  insufficient-funds path shows a status message.
- Locker EQUIP/PREVIEW recolors the hero showcase live; Emote spins the hero.
- Settings sliders/dropdown/toggles bind to SettingsManager and persist via
  PlayerPrefs (Apply + Save and Reset both wired).
- 11 NUnit EditMode tests written for navigation, settings clamping and
  profile logic (run via Test Runner in the Editor).

## Known risks for first open (untested in Unity)

1. The hand-written `.unity` YAML may be upgraded/normalized by Unity 6 on
   first import. **Supported fallback:** KOBNEXA ROYAL > Rebuild All Scenes
   regenerates both scenes through the Editor API - this is the intended path.
2. Runtime-built UI layout uses absolute anchors at a 1920x1080 reference
   resolution; exact visual spacing vs the reference image needs a screenshot
   comparison pass in Play mode and may need small offset tweaks.
3. URP pipeline asset: until one is created/assigned in the Editor, materials
   use the available Lit/Standard shader fallback; colors may differ slightly
   between pipelines.
4. Procedural hero/environment are placeholders by design - proportions were
   reasoned in code, not visually inspected in a viewport yet.
5. PlayerSettings.SetApplicationIdentifier may log a deprecation warning on
   the newest Unity 6 patches; it is not an error and the menu still applies
   the remaining settings.

## Not verified / not done

- No compilation result, no Play-mode run, no screenshots from inside Unity.
- No build/player export attempted.
- No gameplay (Mission 02 scope).
