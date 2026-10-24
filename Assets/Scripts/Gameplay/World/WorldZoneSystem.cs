namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// Once per second, reads the player position (via ThirdPersonPlayerController)
    /// and tints the scene for the current zone: blue-gold dusk on the Royal
    /// Plains, dark green-blue in the Dark Forest, pale blue-white on the Frozen
    /// Peaks. Null-safe: with no player present it simply keeps CurrentZone.
    /// Only fog colour and ambient light are owned here; fog density stays
    /// with WeatherSystem.
    /// </summary>
    public sealed class WorldZoneSystem : MonoBehaviour
    {
        public string CurrentZone = "Royal Plains";

        private float checkTimer;

        private void Start()
        {
            UpdateZone();
        }

        private void Update()
        {
            checkTimer += Time.deltaTime;
            if (checkTimer >= 1f)
            {
                checkTimer = 0f;
                UpdateZone();
            }
        }

        private void UpdateZone()
        {
            ThirdPersonPlayerController player = FindPlayer();
            if (player == null) return;

            CurrentZone = RoyalWorldBuilder.ZoneAt(player.transform.position);
            RenderSettings.fog = true;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            if (CurrentZone == "Dark Forest")
            {
                RenderSettings.fogColor = new Color(0.03f, 0.10f, 0.10f);
                RenderSettings.ambientLight = new Color(0.08f, 0.16f, 0.14f);
            }
            else if (CurrentZone == "Frozen Peaks")
            {
                RenderSettings.fogColor = new Color(0.70f, 0.78f, 0.85f);
                RenderSettings.ambientLight = new Color(0.55f, 0.62f, 0.70f);
            }
            else // Royal Plains: blue-gold dusk.
            {
                RenderSettings.fogColor = new Color(0.14f, 0.19f, 0.36f);
                RenderSettings.ambientLight = new Color(0.32f, 0.30f, 0.28f);
            }
        }

        private static ThirdPersonPlayerController FindPlayer()
        {
            ThirdPersonPlayerController[] players =
                Object.FindObjectsByType<ThirdPersonPlayerController>(FindObjectsSortMode.None);
            return players.Length > 0 ? players[0] : null;
        }
    }
}
