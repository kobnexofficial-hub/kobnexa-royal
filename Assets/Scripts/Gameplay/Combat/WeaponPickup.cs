namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// Floating weapon pickup. A Blaster pickup enables the player's
    /// RoyalBlaster and adds AmmoBonus to its reserve; a Melee pickup enables
    /// the player's MeleeWeapon. Destroyed on pickup. Defensive about how the
    /// player is identified: tag "Player", GameObject name "Player", or a
    /// HealthComponent on a root named "Player".
    /// </summary>
    public class WeaponPickup : MonoBehaviour
    {
        public enum PickupKind { Blaster, Melee }

        public PickupKind Kind = PickupKind.Blaster;
        public int AmmoBonus = 30;

        private Transform visual;
        private Vector3 basePosition;
        private bool collected;

        private void Awake()
        {
            SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 2f;
            BuildVisual();
            basePosition = transform.position;
        }

        private void BuildVisual()
        {
            PrimitiveType shape = Kind == PickupKind.Blaster ? PrimitiveType.Box : PrimitiveType.Capsule;
            GameObject visualGo = GameObject.CreatePrimitive(shape);
            visualGo.name = "PickupVisual";
            Collider visualCollider = visualGo.GetComponent<Collider>();
            if (visualCollider != null) Object.Destroy(visualCollider);
            visualGo.transform.SetParent(transform, false);
            visualGo.transform.localPosition = new Vector3(0f, 1f, 0f);
            if (Kind == PickupKind.Blaster) visualGo.transform.localScale = new Vector3(0.9f, 0.25f, 0.25f);
            Renderer renderer = visualGo.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Standard");
                if (shader != null)
                {
                    Material mat = new Material(shader);
                    mat.color = RoyalColors.RoyalGold;
                    mat.SetFloat("_Metallic", 0.8f);
                    mat.SetFloat("_Smoothness", 0.4f);
                    renderer.material = mat;
                }
            }
            Light light = gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = RoyalColors.RoyalGoldBright;
            light.intensity = 1.5f;
            light.range = 6f;
            visual = visualGo.transform;
        }

        private void Update()
        {
            if (collected) return;
            if (visual != null)
            {
                visual.Rotate(Vector3.up, 60f * Time.deltaTime, Space.World);
                transform.position = basePosition + new Vector3(0f, Mathf.Sin(Time.time * 2f) * 0.25f, 0f);
            }
            // Fallback for a player with no collider / missed trigger event.
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                ThirdPersonPlayerController controller = FindObjectOfType<ThirdPersonPlayerController>();
                if (controller != null) player = controller.gameObject;
            }
            if (player != null && Vector3.Distance(player.transform.position, transform.position) < 2f)
            {
                GrantTo(player);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected || other == null) return;
            if (IsPlayerObject(other.gameObject)) GrantTo(other.gameObject);
        }

        private static bool IsPlayerObject(GameObject go)
        {
            if (go == null) return false;
            if (go.name == "Player") return true;
            if (go.CompareTag("Player")) return true;
            if (go.GetComponent<ThirdPersonPlayerController>() != null) return true;
            Transform root = go.transform.root;
            if (root != null && root.name == "Player") return true;
            if (root != null && root.GetComponent<ThirdPersonPlayerController>() != null) return true;
            return false;
        }

        private void GrantTo(GameObject playerObject)
        {
            if (collected || playerObject == null) return;
            collected = true;

            Transform root = playerObject.transform.root;
            GameObject player = root != null ? root.gameObject : playerObject;

            if (Kind == PickupKind.Blaster)
            {
                RoyalBlaster blaster = player.GetComponent<RoyalBlaster>();
                if (blaster == null) blaster = player.GetComponentInChildren<RoyalBlaster>();
                if (blaster == null) blaster = player.AddComponent<RoyalBlaster>();
                blaster.enabled = true;
                blaster.AddAmmo(AmmoBonus);
            }
            else
            {
                MeleeWeapon melee = player.GetComponent<MeleeWeapon>();
                if (melee == null) melee = player.GetComponentInChildren<MeleeWeapon>();
                if (melee == null) melee = player.AddComponent<MeleeWeapon>();
                melee.enabled = true;
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();
            Object.Destroy(gameObject);
        }
    }
}
