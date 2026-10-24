namespace KobnexaRoyal.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// One-click scene generation. This is the SUPPORTED way to (re)create the
    /// project's scenes: it builds them through the Editor API, so the saved
    /// .unity files are always in the exact format of the installed Unity version.
    /// </summary>
    public static class RoyalLobbySceneBuilder
    {
        private const string BootstrapPath = "Assets/Scenes/Bootstrap.unity";
        private const string LobbyPath = "Assets/Scenes/RoyalLobby.unity";
        private const string RoyalBattlePath = "Assets/Scenes/RoyalBattle.unity";

        [MenuItem("KOBNEXA ROYAL/Rebuild All Scenes")]
        public static void RebuildAllScenes()
        {
            Directory.CreateDirectory("Assets/Scenes");
            BuildBootstrapScene();
            BuildLobbyScene();
            BuildRoyalBattleScene();
            SetBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[KOBNEXA ROYAL] Scenes rebuilt: Bootstrap + RoyalLobby + RoyalBattle, Build Settings updated (Bootstrap first).");
        }

        [MenuItem("KOBNEXA ROYAL/Apply Project Settings")]
        public static void ApplyProjectSettings()
        {
            PlayerSettings.companyName = "KOBNEXA";
            PlayerSettings.productName = "KOBNEXA ROYAL";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, "com.kobnexa.royal");
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            AssetDatabase.SaveAssets();
            Debug.Log("[KOBNEXA ROYAL] Project settings applied: KOBNEXA / KOBNEXA ROYAL / com.kobnexa.royal / 1920x1080.");
        }

        [MenuItem("KOBNEXA ROYAL/Validate Project")]
        public static void ValidateProject()
        {
            string[] required =
            {
                "Assets/Scenes/RoyalLobby.unity",
                "Assets/Scenes/Bootstrap.unity",
                "Assets/Scripts/Core/GameBootstrap.cs",
                "Assets/Scripts/UI/LobbyUIManager.cs",
                "Assets/Scripts/Lobby/LobbyEnvironmentBuilder.cs",
                "Assets/Scripts/Lobby/LobbyCharacterShowcase.cs",
                "Packages/manifest.json",
                "ProjectSettings/ProjectVersion.txt",
            };
            int missing = 0;
            for (int i = 0; i < required.Length; i++)
            {
                if (!File.Exists(required[i])) { Debug.LogError("[KOBNEXA ROYAL] MISSING: " + required[i]); missing++; }
            }
            if (missing == 0) Debug.Log("[KOBNEXA ROYAL] Validation passed: all required files exist.");
            else Debug.LogError("[KOBNEXA ROYAL] Validation failed: " + missing + " file(s) missing.");
        }

        private static void BuildBootstrapScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Bootstrap";
            GameObject root = new GameObject("GameRoot");
            root.AddComponent<GameBootstrap>();
            EditorSceneManager.SaveScene(scene, BootstrapPath);
        }

        private static void BuildLobbyScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "RoyalLobby";

            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.04f, 0.12f);
            cam.fieldOfView = 42f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 220f;
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0f, 3.1f, -9.9f);
            camGo.AddComponent<LobbyCameraController>();

            GameObject sunGo = new GameObject("Directional Light (Royal Sun)");
            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.88f, 0.62f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(48f, -18f, 0f);

            GameObject root = new GameObject("RoyalLobbyRoot");
            root.AddComponent<GameBootstrap>();
            root.AddComponent<LobbyEnvironmentBuilder>();
            root.AddComponent<LobbyCharacterShowcase>();
            root.AddComponent<LobbyUIManager>();
            root.AddComponent<RoyalEnergyParticles>();

            EditorSceneManager.SaveScene(scene, LobbyPath);
        }

        [MenuItem("KOBNEXA ROYAL/Build Royal Battle Scene")]
        public static void BuildRoyalBattleScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "RoyalBattle";

            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.10f, 0.20f);
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 500f;
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0f, 3f, -38f);

            GameObject sunGo = new GameObject("Directional Light");
            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.88f, 0.62f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject root = new GameObject("RoyalBattleRoot");
            root.AddComponent<GameBootstrap>();
            root.AddComponent<RoyalWorldBuilder>();
            root.AddComponent<WeatherSystem>();
            root.AddComponent<WorldZoneSystem>();
            root.AddComponent<MatchManager>();
            root.AddComponent<GameplayHUD>();

            // Playable hero: without this object MatchManager would fall back to an
            // immobile capsule. The controller adds a CharacterController in code.
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1f, -30f);
            player.AddComponent<ThirdPersonPlayerController>();
            player.AddComponent<HealthComponent>();
            player.AddComponent<RoyalBlaster>();
            player.AddComponent<MeleeWeapon>();
            player.AddComponent<PlayerInventory>();

            // Starting loot: 3 chests, 2 weapon pickups, 2 ground loot drops.
            Vector3[] chestPositions = { new Vector3(6f, 0.5f, -20f), new Vector3(-12f, 0.5f, 8f), new Vector3(14f, 0.5f, 30f) };
            for (int i = 0; i < chestPositions.Length; i++)
            {
                GameObject chest = new GameObject("LootChest_" + i);
                chest.transform.position = chestPositions[i];
                chest.AddComponent<LootChest>();
            }
            GameObject blasterPickup = new GameObject("WeaponPickup_Blaster");
            blasterPickup.transform.position = new Vector3(-4f, 1f, -24f);
            WeaponPickup bp = blasterPickup.AddComponent<WeaponPickup>();
            bp.Kind = WeaponPickup.PickupKind.Blaster;
            bp.AmmoBonus = 60;
            GameObject meleePickup = new GameObject("WeaponPickup_Melee");
            meleePickup.transform.position = new Vector3(4f, 1f, -24f);
            meleePickup.AddComponent<WeaponPickup>().Kind = WeaponPickup.PickupKind.Melee;
            GameObject ammoLoot = new GameObject("GroundLoot_Ammo");
            ammoLoot.transform.position = new Vector3(-2f, 0.6f, -18f);
            ammoLoot.AddComponent<GroundLoot>().Item = new InventoryItem { Kind = ItemKind.Ammo, Name = "Blaster Ammo", Amount = 24 };
            GameObject healthLoot = new GameObject("GroundLoot_Health");
            healthLoot.transform.position = new Vector3(2f, 0.6f, -18f);
            healthLoot.AddComponent<GroundLoot>().Item = new InventoryItem { Kind = ItemKind.HealthPack, Name = "Health Pack", Amount = 1 };

            EditorSceneManager.SaveScene(scene, RoyalBattlePath);
        }

        private static void SetBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(BootstrapPath, true),
                new EditorBuildSettingsScene(LobbyPath, true),
                new EditorBuildSettingsScene(RoyalBattlePath, true),
            };
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
