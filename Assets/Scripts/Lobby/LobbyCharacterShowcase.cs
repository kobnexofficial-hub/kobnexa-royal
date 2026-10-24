namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// The Crown Sovereign hero placeholder, built from primitives in the exact
    /// identity of the reference: black armor, royal gold trim, glowing blue core
    /// and visor, crown helm and a deep blue cape. Stands on the showcase platform
    /// with a breathing / slow-turn idle. Swap this builder for a final rigged
    /// model later - callers only use SetAccentScheme() and DisplayName.
    /// </summary>
    public sealed class LobbyCharacterShowcase : MonoBehaviour
    {
        public string DisplayName = "KobnexaEsteril";
        public string StatusText = "Ready";

        private Transform heroRoot;
        private Transform torso;
        private Material accentGlow;
        private Material capeMat;
        private float baseY;
        private float timeOffset;
        private float emoteEndTime = -1f;
        private float emoteStartTime = 0f;

        /// <summary>Quick celebratory 360-degree spin, triggered by the Emote button.</summary>
        public void PlayEmote()
        {
            emoteStartTime = Time.time;
            emoteEndTime = Time.time + 1.2f;
        }

        private static readonly Color[] AccentColors = new Color[]
        {
            new Color(0.35f, 0.65f, 1.00f),  // 0 Gold Sovereign (blue core, as reference)
            new Color(0.45f, 0.55f, 1.00f),  // 1 Midnight Royal
            new Color(0.25f, 0.40f, 0.95f),  // 2 Deep Navy Guard
        };

        private void Start()
        {
            Build();
        }

        private void Update()
        {
            if (heroRoot == null) return;
            timeOffset += Time.deltaTime;
            heroRoot.position = new Vector3(heroRoot.position.x, baseY + Mathf.Sin(timeOffset * 1.3f) * 0.045f, heroRoot.position.z);
            if (Time.time < emoteEndTime)
            {
                float emoteT = (Time.time - emoteStartTime) / 1.2f;
                heroRoot.localRotation = Quaternion.Euler(0f, emoteT * 360f, 0f);
            }
            else
            {
                heroRoot.localRotation = Quaternion.Euler(0f, Mathf.Sin(timeOffset * 0.22f) * 14f, 0f);
            }
            if (torso != null)
            {
                float breathe = 1f + Mathf.Sin(timeOffset * 1.3f) * 0.012f;
                torso.localScale = new Vector3(torso.localScale.x, breathe, torso.localScale.z);
            }
        }

        public void SetAccentScheme(int scheme)
        {
            if (accentGlow == null) return;
            if (scheme < 0 || scheme >= AccentColors.Length) scheme = 0;
            Color c = AccentColors[scheme];
            accentGlow.SetColor("_EmissionColor", c * 2.2f);
            accentGlow.color = c;
            Debug.Log("[KOBNEXA ROYAL] Locker accent scheme applied: " + scheme);
        }

        private void Build()
        {
            heroRoot = new GameObject("Hero_CrownSovereign").transform;
            heroRoot.SetParent(transform, false);
            heroRoot.position = LobbyEnvironmentBuilder.ShowcasePosition;
            baseY = heroRoot.position.y;

            Material blackArmor = MakeMat("HeroBlackArmor", new Color(0.03f, 0.03f, 0.06f), 0.85f, 0.55f, Color.black, 0f);
            Material gold = MakeMat("HeroGoldTrim", RoyalColors.RoyalGold, 0.95f, 0.35f, Color.black, 0f);
            accentGlow = MakeMat("HeroCoreGlow", RoyalColors.RoyalBlue, 0.2f, 0.6f, RoyalColors.RoyalBlueGlow, 2.2f);
            capeMat = MakeMat("HeroCape", new Color(0.04f, 0.08f, 0.45f), 0.05f, 0.75f, Color.black, 0f);

            // Legs and armored boots.
            Part(PrimitiveType.Capsule, "Leg_L", new Vector3(-0.22f, 0.75f, 0f), new Vector3(0.30f, 0.75f, 0.30f), blackArmor);
            Part(PrimitiveType.Capsule, "Leg_R", new Vector3(0.22f, 0.75f, 0f), new Vector3(0.30f, 0.75f, 0.30f), blackArmor);
            Part(PrimitiveType.Cube, "Boot_L", new Vector3(-0.22f, 0.09f, -0.06f), new Vector3(0.34f, 0.18f, 0.62f), gold);
            Part(PrimitiveType.Cube, "Boot_R", new Vector3(0.22f, 0.09f, -0.06f), new Vector3(0.34f, 0.18f, 0.62f), gold);
            Part(PrimitiveType.Cube, "ShinGlow_L", new Vector3(-0.22f, 0.55f, -0.17f), new Vector3(0.10f, 0.5f, 0.05f), accentGlow);
            Part(PrimitiveType.Cube, "ShinGlow_R", new Vector3(0.22f, 0.55f, -0.17f), new Vector3(0.10f, 0.5f, 0.05f), accentGlow);
            Part(PrimitiveType.Cube, "Knee_L", new Vector3(-0.22f, 1.02f, -0.14f), new Vector3(0.30f, 0.22f, 0.16f), gold);
            Part(PrimitiveType.Cube, "Knee_R", new Vector3(0.22f, 1.02f, -0.14f), new Vector3(0.30f, 0.22f, 0.16f), gold);

            // Torso with glowing chest core.
            GameObject torsoGo = Part(PrimitiveType.Capsule, "Torso", new Vector3(0f, 1.95f, 0f), new Vector3(0.95f, 0.85f, 0.55f), blackArmor);
            torso = torsoGo.transform;
            Part(PrimitiveType.Cube, "ChestPlate", new Vector3(0f, 2.10f, -0.26f), new Vector3(0.72f, 0.72f, 0.12f), blackArmor);
            Part(PrimitiveType.Sphere, "ChestCore", new Vector3(0f, 2.12f, -0.34f), new Vector3(0.30f, 0.30f, 0.14f), accentGlow);
            Part(PrimitiveType.Cube, "GoldBelt", new Vector3(0f, 1.42f, 0f), new Vector3(0.78f, 0.16f, 0.52f), gold);
            Part(PrimitiveType.Cube, "AbsGlow", new Vector3(0f, 1.68f, -0.25f), new Vector3(0.12f, 0.34f, 0.05f), accentGlow);

            // Pauldrons and arms.
            Part(PrimitiveType.Sphere, "Pauldron_L", new Vector3(-0.68f, 2.42f, 0f), new Vector3(0.55f, 0.45f, 0.55f), gold);
            Part(PrimitiveType.Sphere, "Pauldron_R", new Vector3(0.68f, 2.42f, 0f), new Vector3(0.55f, 0.45f, 0.55f), gold);
            GameObject armL = Part(PrimitiveType.Capsule, "Arm_L", new Vector3(-0.74f, 1.80f, 0f), new Vector3(0.26f, 0.70f, 0.26f), blackArmor);
            GameObject armR = Part(PrimitiveType.Capsule, "Arm_R", new Vector3(0.74f, 1.80f, 0f), new Vector3(0.26f, 0.70f, 0.26f), blackArmor);
            armL.transform.localRotation = Quaternion.Euler(0f, 0f, 7f);
            armR.transform.localRotation = Quaternion.Euler(0f, 0f, -7f);
            Part(PrimitiveType.Cube, "GauntletGlow_L", new Vector3(-0.80f, 1.22f, -0.05f), new Vector3(0.20f, 0.34f, 0.24f), accentGlow);
            Part(PrimitiveType.Cube, "GauntletGlow_R", new Vector3(0.80f, 1.22f, -0.05f), new Vector3(0.20f, 0.34f, 0.24f), accentGlow);

            // Crown helm with glowing visor.
            Part(PrimitiveType.Sphere, "Helm", new Vector3(0f, 2.98f, 0f), new Vector3(0.62f, 0.68f, 0.62f), blackArmor);
            Part(PrimitiveType.Cube, "Visor", new Vector3(0f, 2.98f, -0.28f), new Vector3(0.44f, 0.12f, 0.08f), accentGlow);
            Part(PrimitiveType.Cylinder, "CrownBand", new Vector3(0f, 3.32f, 0f), new Vector3(0.66f, 0.10f, 0.66f), gold);
            for (int i = 0; i < 5; i++)
            {
                float a = (i - 2) * 0.55f;
                Vector3 p = new Vector3(Mathf.Sin(a) * 0.28f, 3.58f, -Mathf.Cos(a) * 0.28f + 0.05f);
                GameObject spike = Part(PrimitiveType.Cylinder, "CrownSpike_" + i, p, new Vector3(0.07f, 0.22f, 0.07f), gold);
                spike.transform.localScale = new Vector3(0.10f, 0.22f, 0.10f);
            }
            Part(PrimitiveType.Sphere, "CrownGem", new Vector3(0f, 3.36f, -0.32f), new Vector3(0.16f, 0.16f, 0.08f), accentGlow);

            // Cape: two angled panels flowing behind, as in the reference.
            GameObject capeL = Part(PrimitiveType.Cube, "Cape_L", new Vector3(-0.34f, 1.55f, 0.42f), new Vector3(0.62f, 2.5f, 0.05f), capeMat);
            GameObject capeR = Part(PrimitiveType.Cube, "Cape_R", new Vector3(0.34f, 1.55f, 0.42f), new Vector3(0.62f, 2.5f, 0.05f), capeMat);
            capeL.transform.localRotation = Quaternion.Euler(-9f, 0f, 5f);
            capeR.transform.localRotation = Quaternion.Euler(-9f, 0f, -5f);
        }

        private GameObject Part(PrimitiveType type, string name, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(heroRoot, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            Renderer r = go.GetComponent<Renderer>();
            if (r != null && mat != null) r.sharedMaterial = mat;
            Collider c = go.GetComponent<Collider>();
            if (c != null) c.enabled = false;
            return go;
        }

        private static Material MakeMat(string name, Color color, float metallic, float smoothness, Color emission, float emissionIntensity)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material m = new Material(shader);
            m.name = name;
            m.color = color;
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            if (emissionIntensity > 0f)
            {
                m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", emission * emissionIntensity);
            }
            return m;
        }
    }
}
