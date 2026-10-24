namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// Atmosphere: floating gold/blue dust in the lobby plus a distant starfield.
    /// Both are real ParticleSystems created in code, with sprite-shader fallbacks.
    /// </summary>
    public sealed class RoyalEnergyParticles : MonoBehaviour
    {
        private void Start()
        {
            CreateDust();
            CreateStars();
        }

        private static Material ParticleMaterial(Color color)
        {
            Shader shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
            Material m = new Material(shader);
            m.color = color;
            return m;
        }

        private void CreateDust()
        {
            GameObject go = new GameObject("RoyalDust");
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(0f, 3f, 6f);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.16f);
            main.startColor = new ParticleSystem.MinMaxGradient(RoyalColors.RoyalGoldBright, RoyalColors.RoyalBlueGlow);
            main.maxParticles = 160;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 22f;
            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(26f, 7f, 22f);
            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = ParticleMaterial(Color.white);
            ps.Play();
        }

        private void CreateStars()
        {
            GameObject go = new GameObject("Starfield");
            go.transform.SetParent(transform, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startLifetime = 999f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.28f);
            main.maxParticles = 420;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0f;
            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = ParticleMaterial(new Color(0.85f, 0.90f, 1f));

            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[420];
            for (int i = 0; i < particles.Length; i++)
            {
                float x = Random.Range(-90f, 90f);
                float y = Random.Range(14f, 70f);
                float z = Random.Range(25f, 110f);
                particles[i].position = new Vector3(x, y, z);
                particles[i].startSize = Random.Range(0.10f, 0.28f);
                particles[i].startColor = new Color(0.85f, 0.90f, 1f, Random.Range(0.5f, 1f));
                particles[i].remainingLifetime = 999f;
            }
            ps.SetParticles(particles, particles.Length);
            ps.Play();
        }
    }
}
