namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// Cycles Clear -&gt; Rain -&gt; Storm -&gt; Clear on a timer. Rain is a
    /// ParticleSystem created in code that follows the player from above;
    /// Storm darkens the scene Directional Light (if one exists), thickens fog
    /// and strengthens the rain; Clear restores light and fog. All scene
    /// lookups are null-safe: the system runs with no player and no light.
    /// </summary>
    public sealed class WeatherSystem : MonoBehaviour
    {
        public float CycleSeconds = 90f;
        public string CurrentWeather = "Clear";

        private static readonly string[] Cycle = { "Clear", "Rain", "Storm" };

        private float timer;
        private int cycleIndex;
        private GameObject rainGo;
        private ParticleSystem rain;
        private ParticleSystem.EmissionModule rainEmission;
        private Light directionalLight;
        private bool hasBaseIntensity;
        private float baseIntensity = 1f;
        private ThirdPersonPlayerController player;
        private float playerSearchTimer;

        private void Start()
        {
            if (CycleSeconds < 1f) CycleSeconds = 1f;
            cycleIndex = 0;
            CurrentWeather = "Clear";
            CreateRain();
            FindDirectionalLight();
            ApplyWeather(CurrentWeather);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= CycleSeconds)
            {
                timer = 0f;
                cycleIndex = (cycleIndex + 1) % Cycle.Length;
                ApplyWeather(Cycle[cycleIndex]);
            }

            // Rain follows the player from above; without a player it sits
            // above this object's position.
            if (rainGo != null)
            {
                if (player == null)
                {
                    playerSearchTimer -= Time.deltaTime;
                    if (playerSearchTimer <= 0f)
                    {
                        playerSearchTimer = 1f;
                        player = FindPlayer();
                    }
                }
                Vector3 anchor = player != null ? player.transform.position : transform.position;
                rainGo.transform.position = anchor + Vector3.up * 18f;
            }
        }

        /// <summary>Sets the weather immediately ("Clear", "Rain" or "Storm") and restarts the cycle timer.</summary>
        public void SetWeather(string weather)
        {
            for (int i = 0; i < Cycle.Length; i++)
            {
                if (Cycle[i] == weather)
                {
                    cycleIndex = i;
                    timer = 0f;
                    ApplyWeather(weather);
                    return;
                }
            }
        }

        /// <summary>Exponential-squared fog density for a weather name; Clear value for anything unknown.</summary>
        public float FogDensityFor(string weather)
        {
            if (weather == "Rain") return 0.02f;
            if (weather == "Storm") return 0.045f;
            return 0.008f;
        }

        private void ApplyWeather(string weather)
        {
            CurrentWeather = weather;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = FogDensityFor(weather);

            if (directionalLight == null) FindDirectionalLight();
            if (directionalLight != null)
            {
                if (!hasBaseIntensity)
                {
                    baseIntensity = directionalLight.intensity;
                    hasBaseIntensity = true;
                }
                if (weather == "Rain") directionalLight.intensity = baseIntensity * 0.75f;
                else if (weather == "Storm") directionalLight.intensity = baseIntensity * 0.45f;
                else directionalLight.intensity = baseIntensity;
            }

            if (rain != null)
            {
                rainEmission.enabled = weather != "Clear";
                if (weather == "Rain") rainEmission.rateOverTime = new ParticleSystem.MinMaxCurve(350f);
                else if (weather == "Storm") rainEmission.rateOverTime = new ParticleSystem.MinMaxCurve(900f);
            }
        }

        private void CreateRain()
        {
            rainGo = new GameObject("RainParticles");
            rainGo.transform.position = transform.position + Vector3.up * 18f;
            rain = rainGo.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = rain.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 2f;
            main.startSpeed = 0f;
            main.startSize = 0.06f;
            main.startColor = new Color(0.65f, 0.8f, 1f, 0.6f);
            main.gravityModifier = 2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 2000;

            rainEmission = rain.emission;
            rainEmission.enabled = false;
            rainEmission.rateOverTime = new ParticleSystem.MinMaxCurve(350f);

            ParticleSystem.ShapeModule shape = rain.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(40f, 1f, 40f);

            ParticleSystemRenderer renderer = rainGo.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Stretch;
                renderer.lengthScale = 2f;
            }

            rain.Play();
        }

        private void FindDirectionalLight()
        {
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].type == LightType.Directional)
                {
                    directionalLight = lights[i];
                    return;
                }
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
