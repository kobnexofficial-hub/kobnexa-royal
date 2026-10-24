# KOBNEXA ROYAL - Setup and Launch

Target editor: **Unity 6 LTS (6000.0.x)** - the project was authored against
6000.0.35f1 (`ProjectSettings/ProjectVersion.txt`). Any later 6000.0.x patch
will open it with a harmless version-upgrade prompt.

## 1. Install Unity

1. Download and install **Unity Hub** from https://unity.com/download
2. Sign in to Unity Hub with your Unity account (a free Personal license is
   enough for this project).
3. In Unity Hub: **Installs > Install Editor > Unity 6 LTS (6000.0.x)**.
   Include at least: your target platform build support (Windows/Mac/Android),
   and let it install Visual Studio / VS Code support if offered.

## 2. Open the project

1. Unity Hub > **Projects > Add > Add project from disk**, select this
   `kobnexa-royal` folder.
2. Open it. On first open Unity imports the packages from
   `Packages/manifest.json` (URP 17.0.3, uGUI 2.0.0, Test Framework) - this
   needs an internet connection once and can take a few minutes.

## 3. Generate guaranteed-clean scenes (important, do this once)

The shipped `.unity` files were written by hand in Unity YAML format. The
supported, guaranteed path is to regenerate them through the Editor API:

1. Menu **KOBNEXA ROYAL > Rebuild All Scenes**
   - Creates `Assets/Scenes/Bootstrap.unity` and `Assets/Scenes/RoyalLobby.unity`
     with the Main Camera (+ LobbyCameraController, AudioListener), a golden
     Directional Light, and the `RoyalLobbyRoot` object carrying all lobby
     systems (GameBootstrap, environment, hero, UI, particles).
   - Sets Build Settings with Bootstrap first, RoyalLobby second.
2. Menu **KOBNEXA ROYAL > Apply Project Settings**
   - Company KOBNEXA, product KOBNEXA ROYAL, version 0.1.0, bundle id
     com.kobnexa.royal, default resolution 1920x1080.
3. Menu **KOBNEXA ROYAL > Validate Project** - logs any missing required file.

## 4. One manual setting

**Edit > Project Settings > Player > Other Settings > Active Input Handling**
set to **Both**. The lobby camera and Escape-key navigation use the legacy
`UnityEngine.Input` API; "Both" keeps that working alongside the Input System.

## 5. Run the lobby

Open `Assets/Scenes/RoyalLobby.unity` and press **Play**.

What you should see: the castle lobby with the hero on the glowing platform,
the full reference-style UI, and a status line at the bottom. Try:

- Each top nav section and its BACK button (or the Escape key).
- LOCKER > EQUIP / PREVIEW - the hero's glowing accents change live.
- EMOTE (bottom right) - the hero does a spin.
- PLAY > START LOCAL PROTOTYPE - a confirmation explains there is no online
  matchmaking yet, then a 3-2-1 prototype start sequence runs.
- SETTINGS - change volumes/quality/fullscreen, APPLY + SAVE, restart Play:
  your settings persist (PlayerPrefs).

## 6. Run the tests

**Window > General > Test Runner > EditMode > Run All** - 11 tests cover the
navigation stack, settings clamping and profile XP/currency logic.

## Troubleshooting

- **Pink materials**: URP did not finish importing. Let package import finish,
  then check Edit > Project Settings > Graphics has a URP pipeline asset; if
  missing, create one (Assets > Create > Rendering > URP Asset with Universal
  Renderer) and assign it, and store it under `Assets/Settings/`. The scripts
  fall back to the Standard shader only when URP Lit is unavailable.
- **Scene looks empty / no UI**: run **KOBNEXA ROYAL > Rebuild All Scenes**
  and make sure you opened `RoyalLobby` (not an empty untitled scene). The UI,
  environment and hero are all built at runtime by scripts on `RoyalLobbyRoot`.
- **No keyboard/mouse response**: set Active Input Handling to **Both**
  (step 4) - the EventSystem is created automatically by the UI at runtime.
- **Console warning "Scene ... is not in Build Settings"**: run Rebuild All
  Scenes (it fixes Build Settings), or add the scenes in File > Build Settings.
