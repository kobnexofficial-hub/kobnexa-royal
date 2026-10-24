# KOBNEXA ROYAL

An original 3D action game with a premium royal-fantasy / futuristic lobby.
**Mission 01 + Mission 02 (this build): the Royal Lobby AND a playable Royal Battle game**
- a real Unity 6 project that recreates the approved lobby reference (see
`Documentation/Reference/KOBNEXA_ROYAL_Lobby_Reference.jpg`) and lets you
actually play a full match from it.

## What is in this build

- A procedural 3D royal environment built from real GameObjects, materials and
  lights: grand castle with gold spires and lit windows, floating islands with
  animated waterfalls, two crowned king statues, wide grand stairs, royal
  pillars and swaying banners, energy arcs, dust and starfield particles, and a
  glowing blue/gold showcase platform on a polished floor.
- The Crown Sovereign hero placeholder (black/gold armor, glowing blue core and
  visor, crown helm, blue cape) on the platform with a breathing idle, slow
  turn, locker-driven accent colors and a spin emote.
- A full lobby UI matching the reference layout: top-center navigation
  (PLAY / LOCKER / STORE / BATTLE PASS / QUESTS / CAREER / FRIENDS / SETTINGS),
  profile card (KobnexaEsteril, Level 1), Season 1 card, big PLAY card,
  Daily Quests card, Store promo, Next Reward card, currencies, party slots,
  and Chat / Emote / Back controls. Every button does something real.
- Working local systems: navigation with Back history, prototype store using
  local Crowns, quest XP claims, locker preview that recolors the hero,
  settings (volume / quality / fullscreen / sensitivity) saved to PlayerPrefs,
  and synthesized fallback music and UI sounds when no audio files exist.

## Honest prototype labeling

## Mission 02 - playable Royal Battle (new in this build)

Press PLAY in the lobby and a real local match loads (`RoyalBattle` scene):
third-person movement (WASD, mouse orbit camera, jump, sprint, crouch), a
procedural open world (castle keep, village, watchtowers, 3 biomes, dynamic
Clear/Rain/Storm weather), combat (hitscan Royal Blaster with ammo/reload,
melee swing, weapon pickups), loot chests and ground loot, an 8-slot inventory
with usable health/shield items, 12 AI bots, a shrinking royal zone, kill
feed, alive counter, Victory Royale / Defeat screens, and return to lobby.
Match results (eliminations, damage, placement, survival) feed back into the
local profile, XP/levels, Battle Pass tiers, and the 3 daily quests, which
can now actually be completed and claimed in the lobby. Parties are local
only (invite friends from the list, ready-up) - there is **no** online
matchmaking, account server, or real-money store anywhere, and the UI says
so wherever it applies. PLAY never fakes a server connection.

## Quick start

1. Install Unity Hub and Unity **6 LTS (6000.0.x)** with the URP template
   components (see `Documentation/SETUP_AND_LAUNCH.md`).
2. Open this folder as a project in Unity Hub.
3. In Unity: **KOBNEXA ROYAL > Rebuild All Scenes**, then
   **KOBNEXA ROYAL > Apply Project Settings**.
4. Open `Assets/Scenes/RoyalLobby.unity` and press Play. From the lobby,
   press PLAY to load a Royal Battle match.

Full instructions and troubleshooting: `Documentation/SETUP_AND_LAUNCH.md`.
Architecture: `Documentation/ARCHITECTURE.md`. Test status:
`Documentation/QA_REPORT.md`. Next steps: `Documentation/ROADMAP.md`.

## Project layout

- `Assets/Scenes/` - Bootstrap + RoyalLobby + RoyalBattle scenes
- `Assets/Scripts/Gameplay/` - player, world, weather, combat, AI bots, match rules, inventory, loot, HUD
- `Assets/Scripts/Core/` - bootstrap, settings, audio, scene loading, palette, pure logic
- `Assets/Scripts/Data/` - prototype catalog data (store, quests, pass, locker, friends, career)
- `Assets/Scripts/Lobby/` - procedural environment, hero showcase, camera, particles
- `Assets/Scripts/UI/` - runtime lobby UI (UIFactory + LobbyUIManager)
- `Assets/Scripts/Editor/` - one-click scene builder / project setup / validation
- `Assets/Tests/EditMode/` - NUnit tests for the pure lobby logic
