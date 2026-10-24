namespace KobnexaRoyal
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Builds the RoyalBattle world procedurally at Start from primitives only:
    /// a 200x200 ground plane, a castle keep with towers and gold cone roofs,
    /// a village of 6 houses, 4 watchtowers, scattered trees (cone + cylinder)
    /// and rocks, plus a spawn point named "PlayerSpawn" at (0, 1, -30).
    /// No prefab is required. ZoneAt gives the named zone for a position.
    /// </summary>
    public sealed class RoyalWorldBuilder : MonoBehaviour
    {
        private bool built;
        private Material groundMat;
        private Material stoneMat;
        private Material goldMat;
        private Material woodMat;
        private Material leafMat;
        private Material rockMat;
        private Material snowMat;

        private void Start()
        {
            Build();
        }

        /// <summary>
        /// Zone precedence, in order: z &lt; 0 is Royal Plains, then x &lt; -30 is
        /// Dark Forest, then z &gt; 60 is Frozen Peaks, otherwise Royal Plains.
        /// </summary>
        public static string ZoneAt(Vector3 pos)
        {
            if (pos.z < 0f) return "Royal Plains";
            if (pos.x < -30f) return "Dark Forest";
            if (pos.z > 60f) return "Frozen Peaks";
            return "Royal Plains";
        }

        private void Build()
        {
            if (built) return;
            built = true;

            BuildMaterials();

            Transform world = new GameObject("RoyalWorld").transform;
            world.SetParent(transform, false);
            world.localPosition = Vector3.zero;

            // 200x200 ground plane (Unity plane primitive is 10x10, so scale 20).
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "RoyalGround";
            ground.transform.SetParent(world, false);
            ground.transform.localPosition = Vector3.zero;
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
            Renderer groundRenderer = ground.GetComponent<Renderer>();
            if (groundRenderer != null) groundRenderer.sharedMaterial = groundMat;
            // The plane's MeshCollider is kept, so the player walks on terrain.

            BuildCastle(world);
            BuildVillage(world);
            BuildWatchtowers(world);
            BuildScenery(world);

            // Spawn point: exact world position, unparented so it cannot drift
            // if this builder's root is ever moved.
            GameObject spawn = new GameObject("PlayerSpawn");
            spawn.transform.position = new Vector3(0f, 1f, -30f);
        }

        private void BuildMaterials()
        {
            groundMat = MakeMat("RoyalGroundDark", new Color(0.05f, 0.07f, 0.15f), 0.85f, 0.9f);
            stoneMat = MakeMat("RoyalStone", new Color(0.35f, 0.35f, 0.42f), 0.2f, 0.6f);
            goldMat = MakeMat("RoyalGold", new Color(0.85f, 0.65f, 0.15f), 0.9f, 0.4f);
            woodMat = MakeMat("RoyalWood", new Color(0.35f, 0.22f, 0.12f), 0.05f, 0.7f);
            leafMat = MakeMat("RoyalLeaf", new Color(0.08f, 0.30f, 0.14f), 0.05f, 0.8f);
            rockMat = MakeMat("RoyalRock", new Color(0.35f, 0.35f, 0.38f), 0.1f, 0.7f);
            snowMat = MakeMat("RoyalSnow", new Color(0.85f, 0.90f, 0.95f), 0.05f, 0.6f);
        }

        private static Material MakeMat(string name, Color color, float metallic, float smoothness)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material m = new Material(shader);
            m.name = name;
            m.color = color;
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            return m;
        }

        private void BuildCastle(Transform world)
        {
            Transform castle = new GameObject("CastleKeep").transform;
            castle.SetParent(world, false);
            castle.localPosition = new Vector3(0f, 0f, 48f);

            // Main keep: box body, gold trim, gate facing the spawn (south).
            Prim(PrimitiveType.Cube, "Keep_Body", castle, new Vector3(0f, 6f, 0f), new Vector3(16f, 12f, 10f), stoneMat, true);
            Prim(PrimitiveType.Cube, "Keep_GoldTrim", castle, new Vector3(0f, 12.3f, 0f), new Vector3(17f, 0.6f, 11f), goldMat, false);
            Prim(PrimitiveType.Cube, "Keep_Gate", castle, new Vector3(0f, 2.5f, -5.2f), new Vector3(4f, 5f, 0.4f), woodMat, false);
            Prim(PrimitiveType.Cube, "Keep_Banner", castle, new Vector3(0f, 8f, -5.3f), new Vector3(2f, 3f, 0.2f), goldMat, false);

            // Four corner towers with gold cone roofs.
            float[] towerX = { -10f, 10f, -10f, 10f };
            float[] towerZ = { -6f, -6f, 6f, 6f };
            for (int i = 0; i < towerX.Length; i++)
            {
                Prim(PrimitiveType.Cylinder, "Keep_Tower_" + i, castle,
                    new Vector3(towerX[i], 9f, towerZ[i]), new Vector3(4f, 9f, 4f), stoneMat, true);
                Cone("Keep_TowerRoof_" + i, castle,
                    new Vector3(towerX[i], 19.5f, towerZ[i]), 3f, 3f, 14, goldMat);
            }
            Cone("Keep_MainRoof", castle, new Vector3(0f, 14.5f, 0f), 6f, 5f, 4, goldMat);
            GameObject mainRoof = castle.Find("Keep_MainRoof") != null ? castle.Find("Keep_MainRoof").gameObject : null;
            if (mainRoof != null) mainRoof.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        }

        private void BuildVillage(Transform world)
        {
            Vector3[] houses = new Vector3[]
            {
                new Vector3(-28f, 0f, -12f),
                new Vector3(-18f, 0f, 2f),
                new Vector3(18f, 0f, -8f),
                new Vector3(28f, 0f, 8f),
                new Vector3(-24f, 0f, 22f),
                new Vector3(24f, 0f, 28f),
            };
            float[] rotations = { 0f, 15f, -20f, 30f, -10f, 45f };
            for (int i = 0; i < houses.Length; i++)
            {
                Transform house = new GameObject("VillageHouse_" + i).transform;
                house.SetParent(world, false);
                house.localPosition = houses[i];
                house.localRotation = Quaternion.Euler(0f, rotations[i], 0f);
                Prim(PrimitiveType.Cube, "House_Walls", house, new Vector3(0f, 1.5f, 0f), new Vector3(4f, 3f, 4f), woodMat, true);
                Cone("House_Roof", house, new Vector3(0f, 4f, 0f), 3.2f, 2f, 4, goldMat);
                GameObject roof = house.Find("House_Roof") != null ? house.Find("House_Roof").gameObject : null;
                if (roof != null) roof.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
                Prim(PrimitiveType.Cube, "House_Door", house, new Vector3(0f, 1f, -2.1f), new Vector3(1f, 2f, 0.2f), stoneMat, false);
            }
        }

        private void BuildWatchtowers(Transform world)
        {
            Vector3[] towers = new Vector3[]
            {
                new Vector3(-55f, 0f, -55f),
                new Vector3(55f, 0f, -55f),
                new Vector3(-55f, 0f, 55f),
                new Vector3(55f, 0f, 55f),
            };
            for (int i = 0; i < towers.Length; i++)
            {
                Transform tower = new GameObject("Watchtower_" + i).transform;
                tower.SetParent(world, false);
                tower.localPosition = towers[i];
                // Cylinder primitive is 2 tall, so scale y = height / 2.
                Prim(PrimitiveType.Cylinder, "Watchtower_Shaft", tower, new Vector3(0f, 6f, 0f), new Vector3(2.5f, 6f, 2.5f), stoneMat, true);
                Prim(PrimitiveType.Cube, "Watchtower_Platform", tower, new Vector3(0f, 12.3f, 0f), new Vector3(4f, 0.6f, 4f), woodMat, false);
                Cone("Watchtower_Roof", tower, new Vector3(0f, 14f, 0f), 2.6f, 3f, 14, goldMat);
            }
        }

        private void BuildScenery(Transform world)
        {
            // Deterministic scatter (fixed arithmetic, no RNG state to drift).
            for (int i = 0; i < 36; i++)
            {
                float x = -90f + ((i * 37) % 180);
                float z = -90f + ((i * 53) % 180);
                if (IsNearSpawnOrCastle(x, z)) continue;
                Transform tree = new GameObject("Tree_" + i).transform;
                tree.SetParent(world, false);
                tree.localPosition = new Vector3(x, 0f, z);
                Prim(PrimitiveType.Cylinder, "Tree_Trunk", tree, new Vector3(0f, 1f, 0f), new Vector3(0.6f, 1f, 0.6f), woodMat, false);
                Cone("Tree_Foliage", tree, new Vector3(0f, 4f, 0f), 1.8f, 4f, 10, leafMat);
                if (z > 60f)
                {
                    Cone("Tree_Snow", tree, new Vector3(0f, 5.2f, 0f), 1f, 1.6f, 10, snowMat);
                }
            }
            for (int i = 0; i < 24; i++)
            {
                float x = -85f + ((i * 61) % 170);
                float z = -85f + ((i * 29) % 170);
                if (IsNearSpawnOrCastle(x, z)) continue;
                float size = 0.8f + (i % 4) * 0.4f;
                Prim(PrimitiveType.Sphere, "Rock_" + i, world, new Vector3(x, size * 0.4f, z), Vector3.one * size, rockMat, false);
            }
        }

        private static bool IsNearSpawnOrCastle(float x, float z)
        {
            // Keep the spawn point and the castle keep clear.
            if (Mathf.Abs(x) < 6f && Mathf.Abs(z + 30f) < 6f) return true;
            if (Mathf.Abs(x) < 18f && Mathf.Abs(z - 48f) < 14f) return true;
            return false;
        }

        // ---------------- helpers (own implementations) ----------------

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

        private static GameObject Cone(string name, Transform parent, Vector3 localPos, float radius, float height, int segments, Material mat)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = CreateConeMesh(radius, height, segments);
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            if (mat != null) mr.sharedMaterial = mat;
            return go;
        }

        private static Mesh CreateConeMesh(float radius, float height, int segments)
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            verts.Add(new Vector3(0f, height * 0.5f, 0f)); // 0: apex
            verts.Add(new Vector3(0f, -height * 0.5f, 0f)); // 1: base centre
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
            mesh.name = "RoyalConeMesh";
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
