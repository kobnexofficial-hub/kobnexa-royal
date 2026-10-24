# KOBNEXA ROYAL - Roadmap

## Mission 01 - Royal Lobby (this build)

Real Unity 6 project, procedural royal environment, hero showcase, full
reference-layout lobby UI with working local systems (navigation, prototype
store/quests/locker, persistent settings, fallback audio), EditMode tests.

**First-open checklist (needs Wilno's Unity Editor):** import, Rebuild All
Scenes, press Play, screenshot the lobby vs the reference, run Test Runner.

## Mission 02 - Playable Royal Battle (COMPLETE in this build)

1. `RoyalBattle` scene loaded from the lobby PLAY flow via SceneLoader. DONE
2. Third-person character movement (WASD, mouse orbit, jump, sprint, crouch). DONE
3. Health + shield with regen, hitscan blaster + melee, weapon pickups, 12 AI bots. DONE
4. Procedural open world: castle keep, village, watchtowers, 3 biomes
   (Royal Plains / Dark Forest / Frozen Peaks), dynamic weather
   (Clear/Rain/Storm), loot chests + ground loot + inventory UI. DONE
5. Shrinking royal zone, kill feed, Victory Royale / Defeat screens,
   return to lobby, match results feeding profile/XP/Battle Pass/quests. DONE
6. Local parties (invite + ready-up); online matchmaking honestly NOT built. DONE (as labeled local feature)

**Remaining verification (separate Unity install agent):** open in the
Unity Editor, Rebuild All Scenes, press Play, run EditMode tests, screenshot
lobby + match vs the reference.

## Mission 03 - Progression and polish (next)

1. Final hero model + animations replacing the procedural placeholder.
2. Sound design pass with real clips; music states (lobby / match).
3. Locker items unlock from Battle Pass tiers (tiers already unlock from XP).
4. Larger world content: more locations, vehicles/mounts, more bot difficulty tiers.

## Later missions

- Parties, friends and REAL online matchmaking (replaces the labeled local
  parties; requires a live backend service - not built yet).
- Character customization, persistent accounts, live store.

## Explicitly not in scope until later

Online services of any kind, real-money purchases, anti-cheat, and shipping
builds. The lobby labels every placeholder honestly until its real system
lands.
