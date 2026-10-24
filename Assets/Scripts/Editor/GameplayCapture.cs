// KOBNEXA ROYAL - Gameplay capture tool (Editor-only, documented tool, not gameplay).
// Mirrors LobbyCapture for the RoyalBattle scene: opens RoyalBattle, enters
// Play mode so the world/player/HUD builders run, renders the gameplay
// camera to PNG files at 1920x1080, then exits. Run via:
//   Unity -batchmode -projectPath <project> -executeMethod KobnexaRoyal.Editor.GameplayCapture.CaptureAndExit
// Output: ~/workspace/your_files/KOBNEXA_ROYAL_Gameplay_Render.png (with HUD)
//         ~/workspace/your_files/KOBNEXA_ROYAL_Gameplay_Render_3D.png (world only)
namespace KobnexaRoyal.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    public static class GameplayCapture
    {
        private static int frames;
        private static int phase; // 0 = waiting for play, 1 = captured, waiting to exit play
        private static string outDir;

        [MenuItem("KOBNEXA ROYAL/Capture Gameplay Screenshot")]
        public static void CaptureAndExit()
        {
            outDir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "workspace/your_files");
            Directory.CreateDirectory(outDir);
            EditorSceneManager.OpenScene("Assets/Scenes/RoyalBattle.unity", OpenSceneMode.Single);
            frames = 0;
            phase = 0;
            EditorApplication.update += Tick;
            EditorApplication.EnterPlaymode();
            Debug.Log("[KOBNEXA ROYAL] Capture: entering Play mode for RoyalBattle.");
        }

        private static void Tick()
        {
            if (phase == 1)
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.update -= Tick;
                    Debug.Log("[KOBNEXA ROYAL] Capture: finished, exiting Editor.");
                    EditorApplication.Exit(0);
                }
                return;
            }
            if (!EditorApplication.isPlaying) return;
            frames++;
            if (frames < 180) return; // let Start() builders + HUD build finish

            try
            {
                Camera cam = Camera.main;
                if (cam == null) cam = Object.FindObjectOfType<Camera>();
                if (cam == null) throw new System.Exception("No camera found in RoyalBattle.");

                Canvas canvas = Object.FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = cam;
                    canvas.planeDistance = 1f;
                }

                SaveCameraPng(cam, Path.Combine(outDir, "KOBNEXA_ROYAL_Gameplay_Render.png"));
                if (canvas != null) canvas.enabled = false;
                SaveCameraPng(cam, Path.Combine(outDir, "KOBNEXA_ROYAL_Gameplay_Render_3D.png"));
                Debug.Log("[KOBNEXA ROYAL] Capture: PNGs written to " + outDir);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[KOBNEXA ROYAL] Capture failed: " + e);
            }
            phase = 1;
            EditorApplication.ExitPlaymode();
        }

        private static void SaveCameraPng(Camera cam, string path)
        {
            RenderTexture rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            RenderTexture prev = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            cam.targetTexture = prev;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
        }
    }
}
