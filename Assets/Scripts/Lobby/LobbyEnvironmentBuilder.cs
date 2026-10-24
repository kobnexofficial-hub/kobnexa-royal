namespace KobnexaRoyal
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Procedurally builds the Royal Lobby environment from the approved reference:
    /// grand castle, floating islands with waterfalls, two crowned king statues,
    /// wide stairs, royal banners, glowing showcase platform and polished floor.
    /// Everything is real GameObjects/materials/lights - replaceable piece by piece
    /// with final art assets later without changing any other system.
    /// </summary>
    public sealed class LobbyEnvironmentBuilder : MonoBehaviour
    {
        public const float PlatformTopY = 0.85f;
        public static readonly Vector3 ShowcasePosition = new Vector3(0f, PlatformTopY, 0.6f);

        private readonly List<Transform> bannerPivots = new List<Transform>();
        private readonly List<Transform> energyArcs = new List<Transform>();
        private Material ringMaterial;
        private Material waterfallMaterial;
        private Texture2D waterfallTexture;
        private float timeOffset;

        private Material stoneGold;
        private Material navyStone;
        private Material darkPolished;
        private Material goldMetal;
        private Material goldEmissive;
        private Material blueEmissive;
        private Material bannerNavy;
        private Material rockMat;
        private Material roofGold;

        private void Start()
        {
            BuildMaterials();
            Build();
        }

        private void Update()
        {
            timeOffset += Time.deltaTime;
            for (int i = 0; i < bannerPivots.Count; i++)
            {
                if (bannerPivots[i] != null)
                    bannerPivots[i].localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(timeOffset * 0.8f + i * 1.3f) * 3.5f);
            }
            for (int i = 0; i < energyArcs.Count; i++)
            {
                if (energyArcs[i] != null) energyArcs[i].Rotate(0f, 0f, Time.deltaTime * 6f, Space.Self);
            }
            if (ringMaterial != null)
            {
                float pulse = 0.85f + 0.35f * Mathf.Sin(timeOffset * 2.0f);
                ringMaterial.SetColor("_EmissionColor", RoyalColors.RoyalBlueGlow * pulse);
            }
            if (waterfallMaterial != null && waterfallTexture != null)
            {
                waterfallMaterial.mainTextureOffset = new Vector2(0f, -timeOffset * 0.55f);
            }
        }

        // ---------------- materials ----------------

        private void BuildMaterials()
        {
            darkPolished = MakeMat("FloorPolished", new Color(0.05f, 0.07f, 0.15f), 0.85f, 0.9f, Color.black, 0f);
            navyStone = MakeMat("NavyStone", new Color(0.07f, 0.10f, 0.22f), 0.35f, 0.55f, Color.black, 0f);
            stoneGold = MakeMat("StatueStone", new Color(0.42f, 0.40f, 0.36f), 0.55f, 0.5f, Color.black, 0f);
            goldMetal = MakeMat("RoyalGoldMetal", RoyalColors.RoyalGold, 0.95f, 0.35f, Color.black, 0f);
            roofGold = MakeMat("RoofGold", new Color(0.55f, 0.40f, 0.12f), 0.8f, 0.45f, Color.black, 0f);
            goldEmissive = MakeMat("GoldGlow", RoyalColors.RoyalGoldBright, 0.2f, 0.6f, RoyalColors.RoyalGoldBright, 1.6f);
            blueEmissive = MakeMat("BlueGlow", RoyalColors.RoyalBlue, 0.2f, 0.6f, RoyalColors.RoyalBlueGlow, 2.0f);
            bannerNavy = MakeMat("BannerNavy", new Color(0.05f, 0.10f, 0.35f), 0.1f, 0.7f, Color.black, 0f);
            rockMat = MakeMat("IslandRock", new Color(0.16f, 0.15f, 0.22f), 0.05f, 0.9f, Color.black, 0f);
            ringMaterial = MakeMat("PlatformRingGlow", RoyalColors.RoyalBlue, 0.2f, 0.6f, RoyalColors.RoyalBlueGlow, 1.8f);

            waterfallTexture = CreateWaterfallTexture();
            waterfallMaterial = MakeMat("Waterfall", new Color(0.45f, 0.75f, 1f), 0f, 0.9f, new Color(0.3f, 0.6f, 1f), 0.9f);
            waterfallMaterial.mainTexture = waterfallTexture;
            waterfallMaterial.mainTexture.wrapMode = TextureWrapMode.Repeat;
            SetTransparent(waterfallMaterial, 0.55f);
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

        private static void SetTransparent(Material m, float alpha)
        {
            Color c = m.color; c.a = alpha; m.color = c;
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
            if (m.HasProperty("_Mode")) m.SetFloat("_Mode", 3f);
            m.renderQueue = 3000;
        }

        private static Texture2D CreateWaterfallTexture()
        {
            Texture2D tex = new Texture2D(32, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float streak = 0.65f + 0.35f * Mathf.Sin(x * 1.7f) * Mathf.Sin(x * 0.6f + y * 0.05f);
                    float fade = 0.75f + 0.25f * Mathf.Sin(y * 0.35f);
                    tex.SetPixel(x, y, new Color(streak, streak, 1f, fade));
                }
            }
            tex.Apply();
            return tex;
        }

        // ---------------- construction ----------------

        private void Build()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.008f;
            RenderSettings.fogColor = new Color(0.03f, 0.05f, 0.12f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.30f, 0.50f);

            Transform env = new GameObject("RoyalEnvironment").transform;
            env.SetParent(transform, false);

            // Polished reflective-look floor (ReflectionProbe below sells the reflection).
            Prim(PrimitiveType.Cylinder, "RoyalFloor", env, new Vector3(0f, -0.26f, 14f), new Vector3(56f, 0.26f, 56f), darkPolished, true);
            GameObject probeGo = new GameObject("LobbyReflectionProbe");
            probeGo.transform.SetParent(env, false);
            probeGo.transform.position = new Vector3(0f, 3f, 4f);
            ReflectionProbe probe = probeGo.AddComponent<ReflectionProbe>();
            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
            probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
            probe.resolution = 128;
            probe.size = new Vector3(60f, 20f, 60f);

            // Glowing concentric platform rings, like the reference.
            Prim(PrimitiveType.Cylinder, "Ring_Gold_Outer", env, new Vector3(0f, 0.03f, 0.6f), new Vector3(9.6f, 0.05f, 9.6f), goldEmissive, false);
            Prim(PrimitiveType.Cylinder, "Ring_Blue", env, new Vector3(0f, 0.06f, 0.6f), new Vector3(8.6f, 0.05f, 8.6f), ringMaterial, false);
            Prim(PrimitiveType.Cylinder, "Ring_Dark", env, new Vector3(0f, 0.08f, 0.6f), new Vector3(7.4f, 0.05f, 7.4f), navyStone, false);

            // Showcase platform steps.
            Prim(PrimitiveType.Cylinder, "Platform_Step1", env, new Vector3(0f, 0.15f, 0.6f), new Vector3(6.8f, 0.30f, 6.8f), navyStone, true);
            Prim(PrimitiveType.Cylinder, "Platform_Step2", env, new Vector3(0f, 0.40f, 0.6f), new Vector3(5.4f, 0.50f, 5.4f), navyStone, true);
            Prim(PrimitiveType.Cylinder, "Platform_Top", env, new Vector3(0f, 0.62f, 0.6f), new Vector3(4.2f, 0.46f, 4.2f), darkPolished, true);
            Prim(PrimitiveType.Cylinder, "Platform_GoldTrim", env, new Vector3(0f, 0.84f, 0.6f), new Vector3(4.35f, 0.04f, 4.35f), goldMetal, false);

            BuildPillarsAndBanners(env);
            BuildStairsAndTerrace(env);
            BuildCastle(env);
            BuildStatue(env, -8.5f, 7f, "KingStatue_Left");
            BuildStatue(env, 8.5f, 7f, "KingStatue_Right");
            BuildFloatingIsland(env, new Vector3(-17f, 13f, 26f), 1.0f, "Island_Left");
            BuildFloatingIsland(env, new Vector3(16f, 15f, 28f), 1.15f, "Island_Right");
            BuildFloatingIsland(env, new Vector3(-9f, 18f, 40f), 0.8f, "Island_BackLeft");
            BuildFloatingIsland(env, new Vector3(10f, 19f, 42f), 0.9f, "Island_BackRight");
            BuildEnergyArc(env, new Vector3(-13f, 9f, 18f), 6f, "EnergyArc_Left");
            BuildEnergyArc(env, new Vector3(13f, 10f, 20f), 7f, "EnergyArc_Right");

            // Lobby point lights: warm gold front, cool blue sides.
            AddPointLight(env, new Vector3(0f, 6f, -5f), RoyalColors.RoyalGoldBright, 2.2f, 22f, "Light_GoldFront");
            AddPointLight(env, new Vector3(-9f, 5f, 6f), RoyalColors.RoyalBlueGlow, 1.8f, 20f, "Light_BlueLeft");
            AddPointLight(env, new Vector3(9f, 5f, 6f), RoyalColors.RoyalBlueGlow, 1.8f, 20f, "Light_BlueRight");
            AddPointLight(env, new Vector3(0f, 4f, 1f), RoyalColors.RoyalBlueGlow, 1.6f, 10f, "Light_PlatformGlow");
        }

        private void BuildPillarsAndBanners(Transform env)
        {
            float[] xs = { -11f, 11f };
            float[] zs = { -2f, 5f, 12f };
            for (int xi = 0; xi < xs.Length; xi++)
            {
                for (int zi = 0; zi < zs.Length; zi++)
                {
                    Vector3 basePos = new Vector3(xs[xi], 0f, zs[zi]);
                    string n = "Pillar_" + xi + "_" + zi;
                    Prim(PrimitiveType.Cube, n + "_Base", env, basePos + new Vector3(0f, 0.5f, 0f), new Vector3(1.7f, 1f, 1.7f), navyStone, false);
                    Prim(PrimitiveType.Cylinder, n + "_Shaft", env, basePos + new Vector3(0f, 5f, 0f), new Vector3(1.2f, 4f, 1.2f), navyStone, false);
                    Prim(PrimitiveType.Cylinder, n + "_GoldBand", env, basePos + new Vector3(0f, 8.2f, 0f), new Vector3(1.35f, 0.25f, 1.35f), goldMetal, false);
                    Prim(PrimitiveType.Cube, n + "_Capital", env, basePos + new Vector3(0f, 9.4f, 0f), new Vector3(1.8f, 0.8f, 1.8f), goldMetal, false);

                    if (zi == 1)
                    {
                        GameObject pivot = new GameObject(n + "_BannerPivot");
                        pivot.transform.SetParent(env, false);
                        pivot.transform.position = basePos + new Vector3(0f, 8.8f, 0f);
                        Prim(PrimitiveType.Cube, n + "_Banner", pivot.transform, new Vector3(0f, -1.7f, 0f), new Vector3(1.5f, 3.4f, 0.08f), bannerNavy, false);
                        Prim(PrimitiveType.Cube, n + "_BannerTrim", pivot.transform, new Vector3(0f, -3.3f, 0f), new Vector3(1.5f, 0.25f, 0.1f), goldMetal, false);
                        Prim(PrimitiveType.Cube, n + "_BannerCrown", pivot.transform, new Vector3(0f, -1.5f, 0.06f), new Vector3(0.5f, 0.4f, 0.05f), goldEmissive, false);
                        bannerPivots.Add(pivot.transform);
                    }
                }
            }
        }

        private void BuildStairsAndTerrace(Transform env)
        {
            for (int i = 0; i < 9; i++)
            {
                float y = 0.25f + i * 0.22f;
                float z = 10f + i * 1.05f;
                Prim(PrimitiveType.Cube, "GrandStair_" + i, env, new Vector3(0f, y - 0.25f, z), new Vector3(15f, 0.5f + i * 0.22f, 1.05f), navyStone, true);
                Prim(PrimitiveType.Cube, "StairRail_L_" + i, env, new Vector3(-7.9f, y + 0.4f, z), new Vector3(0.35f, 0.9f, 1.05f), goldMetal, false);
                Prim(PrimitiveType.Cube, "StairRail_R_" + i, env, new Vector3(7.9f, y + 0.4f, z), new Vector3(0.35f, 0.9f, 1.05f), goldMetal, false);
            }
            Prim(PrimitiveType.Cube, "CastleTerrace", env, new Vector3(0f, 1.0f, 24f), new Vector3(38f, 2f, 13f), navyStone, true);
        }

        private void BuildCastle(Transform env)
        {
            Transform castle = new GameObject("RoyalCastle").transform;
            castle.SetParent(env, false);
            castle.position = new Vector3(0f, 2f, 30f);

            Prim(PrimitiveType.Cube, "Castle_MainKeep", castle, new Vector3(0f, 8f, 0f), new Vector3(15f, 16f, 8f), navyStone, false);
            Prim(PrimitiveType.Cube, "Castle_GoldTrim", castle, new Vector3(0f, 15.6f, 0f), new Vector3(15.6f, 0.7f, 8.6f), goldMetal, false);
            Cone("Castle_MainSpire", castle, new Vector3(0f, 19.5f, 0f), 4.5f, 7f, roofGold);
            Prim(PrimitiveType.Sphere, "Castle_MainFinial", castle, new Vector3(0f, 23.4f, 0f), new Vector3(0.8f, 0.8f, 0.8f), blueEmissive, false);
            Prim(PrimitiveType.Cube, "Castle_Gate", castle, new Vector3(0f, 3f, -4.2f), new Vector3(4.5f, 6f, 0.4f), darkPolished, false);
            Prim(PrimitiveType.Cube, "Castle_GateGlow", castle, new Vector3(0f, 3f, -4.45f), new Vector3(3.6f, 5.2f, 0.1f), blueEmissive, false);

            float[] towerX = { -11f, -6f, 6f, 11f };
            float[] towerH = { 20f, 24f, 24f, 20f };
            for (int i = 0; i < towerX.Length; i++)
            {
                float h = towerH[i];
                Prim(PrimitiveType.Cylinder, "Castle_Tower_" + i, castle, new Vector3(towerX[i], h * 0.5f, 0f), new Vector3(4f, h * 0.5f, 4f), navyStone, false);
                Cone("Castle_TowerRoof_" + i, castle, new Vector3(towerX[i], h + 2.4f, 0f), 3.0f, 5f, roofGold);
                Prim(PrimitiveType.Sphere, "Castle_TowerFinial_" + i, castle, new Vector3(towerX[i], h + 5.2f, 0f), new Vector3(0.55f, 0.55f, 0.55f), blueEmissive, false);
            }
            // Lit windows.
            for (int x = -5; x <= 5; x += 2)
            {
                for (int y = 4; y <= 13; y += 3)
                {
                    if (x == 0 && y < 8) continue;
                    Prim(PrimitiveType.Cube, "Castle_Window", castle, new Vector3(x * 1.2f, y, -4.15f), new Vector3(0.6f, 1.1f, 0.1f), goldEmissive, false);
                }
            }
            // Castle waterfalls, like the reference.
            Prim(PrimitiveType.Cube, "CastleFall_L", castle, new Vector3(-9f, -1f, -4.3f), new Vector3(1.6f, 6f, 0.12f), waterfallMaterial, false);
            Prim(PrimitiveType.Cube, "CastleFall_R", castle, new Vector3(9f, -1f, -4.3f), new Vector3(1.6f, 6f, 0.12f), waterfallMaterial, false);
        }

        private void BuildStatue(Transform env, float x, float z, string name)
        {
            Transform s = new GameObject(name).transform;
            s.SetParent(env, false);
            s.position = new Vector3(x, 0f, z);
            Prim(PrimitiveType.Cube, name + "_Pedestal", s, new Vector3(0f, 1.1f, 0f), new Vector3(2.6f, 2.2f, 2.6f), navyStone, false);
            Prim(PrimitiveType.Cube, name + "_PedestalTrim", s, new Vector3(0f, 2.25f, 0f), new Vector3(2.8f, 0.18f, 2.8f), goldMetal, false);
            Cone(name + "_Robe", s, new Vector3(0f, 4.1f, 0f), 1.35f, 3.8f, stoneGold);
            Prim(PrimitiveType.Capsule, name + "_Torso", s, new Vector3(0f, 6.3f, 0f), new Vector3(1.5f, 1.1f, 1.0f), stoneGold, false);
            Prim(PrimitiveType.Sphere, name + "_Head", s, new Vector3(0f, 7.9f, 0f), new Vector3(0.85f, 0.95f, 0.85f), stoneGold, false);
            Prim(PrimitiveType.Cylinder, name + "_CrownBand", s, new Vector3(0f, 8.45f, 0f), new Vector3(1.0f, 0.22f, 1.0f), goldMetal, false);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 1.7f - 2.4f;
                Cone(name + "_CrownSpike_" + i, s, new Vector3(Mathf.Sin(a) * 0.35f, 8.85f, Mathf.Cos(a) * 0.35f), 0.14f, 0.6f, goldMetal);
            }
            // Sword held point-down in front, like the reference statues.
            Prim(PrimitiveType.Cube, name + "_SwordBlade", s, new Vector3(0f, 4.6f, -1.0f), new Vector3(0.22f, 3.6f, 0.08f), goldMetal, false);
            Prim(PrimitiveType.Cube, name + "_SwordGuard", s, new Vector3(0f, 6.35f, -1.0f), new Vector3(1.1f, 0.18f, 0.12f), goldMetal, false);
            Prim(PrimitiveType.Cube, name + "_Cape", s, new Vector3(0f, 5.6f, 0.65f), new Vector3(1.9f, 3.8f, 0.12f), bannerNavy, false);
        }

        private void BuildFloatingIsland(Transform env, Vector3 pos, float scale, string name)
        {
            Transform isl = new GameObject(name).transform;
            isl.SetParent(env, false);
            isl.position = pos;
            isl.localScale = Vector3.one * scale;
            GameObject rock = Cone(name + "_Rock", isl, new Vector3(0f, -2.6f, 0f), 4.2f, 5.5f, rockMat);
            rock.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
            Prim(PrimitiveType.Cylinder, name + "_Top", isl, new Vector3(0f, 0.1f, 0f), new Vector3(8.6f, 0.35f, 8.6f), navyStone, false);
            Prim(PrimitiveType.Cube, name + "_Keep", isl, new Vector3(0f, 1.6f, 0f), new Vector3(2.6f, 3f, 2.6f), navyStone, false);
            Cone(name + "_Roof", isl, new Vector3(0f, 3.9f, 0f), 2.1f, 1.8f, roofGold);
            Prim(PrimitiveType.Cube, name + "_WindowGlow", isl, new Vector3(0f, 1.6f, -1.36f), new Vector3(0.7f, 1.2f, 0.1f), blueEmissive, false);
            // Waterfall pouring off the island edge.
            float fallLength = pos.y - 1f;
            Prim(PrimitiveType.Cube, name + "_Waterfall", isl, new Vector3(1.8f, -(fallLength * 0.5f) / scale, 0f), new Vector3(1.3f, fallLength / scale, 0.12f), waterfallMaterial, false);
        }

        private void BuildEnergyArc(Transform env, Vector3 center, float radius, string name)
        {
            Transform arc = new GameObject(name).transform;
            arc.SetParent(env, false);
            arc.position = center;
            int points = 22;
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                float ang = Mathf.PI * t;
                Vector3 p = new Vector3(Mathf.Cos(ang) * radius, Mathf.Sin(ang) * radius * 0.6f, 0f);
                Prim(PrimitiveType.Sphere, name + "_Node_" + i, arc, p, Vector3.one * 0.22f, blueEmissive, false);
            }
            energyArcs.Add(arc);
        }

        // ---------------- helpers ----------------

        private static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            Renderer r = go.GetComponent<Renderer>();
            if (r != null && mat != null) r.sharedMaterial = mat;
            Collider c = go.GetComponent<Collider>();
            if (c != null && !keepCollider) c.enabled = false;
            return go;
        }

        private static GameObject Cone(string name, Transform parent, Vector3 localPos, float radius, float height, Material mat)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = CreateConeMesh(radius, height, 14);
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            if (mat != null) mr.sharedMaterial = mat;
            return go;
        }

        private static Mesh CreateConeMesh(float radius, float height, int segments)
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            Vector3 apex = new Vector3(0f, height * 0.5f, 0f);
            Vector3 baseCenter = new Vector3(0f, -height * 0.5f, 0f);
            verts.Add(apex); // 0
            verts.Add(baseCenter); // 1
            for (int i = 0; i < segments; i++)
            {
                float a = (float)i / segments * Mathf.PI * 2f;
                verts.Add(new Vector3(Mathf.Cos(a) * radius, -height * 0.5f, Mathf.Sin(a) * radius));
            }
            for (int i = 0; i < segments; i++)
            {
                int cur = 2 + i;
                int nxt = 2 + (i + 1) % segments;
                tris.Add(0); tris.Add(nxt); tris.Add(cur);
                tris.Add(1); tris.Add(cur); tris.Add(nxt);
            }
            Mesh mesh = new Mesh();
            mesh.name = "ConeMesh";
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddPointLight(Transform parent, Vector3 pos, Color color, float intensity, float range, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            Light l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = range;
        }
    }
}
