namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// A single ground loot pickup: a small floating, spinning primitive.
    /// Press E while the player is within 3 units to add <see cref="Item"/> to
    /// the player's PlayerInventory. The pickup only disappears once the item
    /// was actually added (a full inventory, or no inventory, keeps it on the
    /// ground and logs the reason). Set <see cref="Item"/> in the inspector
    /// or from code; an unset item defaults to Blaster Ammo.
    /// </summary>
    public sealed class GroundLoot : MonoBehaviour
    {
        public InventoryItem Item;

        private GameObject visual;
        private Vector3 visualBasePosition;
        private GameObject playerObject;
        private float reResolveTimer;

        private void Awake()
        {
            visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "LootVisual";
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            visual.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            Collider collider = visual.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            visualBasePosition = visual.transform.localPosition;
        }

        private void Start()
        {
            if (Item == null) Item = new InventoryItem { Kind = ItemKind.Ammo, Name = "Blaster Ammo", Amount = 12 };
            RefreshVisual();
            ResolvePlayer();
        }

        private void Update()
        {
            if (visual != null)
            {
                visual.transform.Rotate(0f, 90f * Time.deltaTime, 0f);
                visual.transform.localPosition = visualBasePosition + Vector3.up * (Mathf.Sin(Time.time * 2f) * 0.15f);
            }
            if (playerObject == null)
            {
                reResolveTimer += Time.deltaTime;
                if (reResolveTimer >= 0.5f)
                {
                    reResolveTimer = 0f;
                    ResolvePlayer();
                }
                return;
            }
            if (Vector3.Distance(playerObject.transform.position, transform.position) <= 3f
                && Input.GetKeyDown(KeyCode.E))
            {
                TryPickup();
            }
        }

        private void TryPickup()
        {
            PlayerInventory inventory = null;
            if (playerObject != null) inventory = playerObject.GetComponent<PlayerInventory>();
            if (inventory == null) inventory = FindObjectOfType<PlayerInventory>();
            if (inventory == null)
            {
                Debug.Log("[KOBNEXA ROYAL] Ground loot not picked up: no PlayerInventory found.");
                return;
            }
            if (Item == null)
            {
                Debug.Log("[KOBNEXA ROYAL] Ground loot has no item configured.");
                return;
            }
            if (!inventory.AddItem(Item))
            {
                Debug.Log("[KOBNEXA ROYAL] Ground loot did not fit: " + Item.Name + " (inventory full).");
                return;
            }
            if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();
            Object.Destroy(gameObject);
        }

        private void RefreshVisual()
        {
            if (visual == null || Item == null) return;
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer == null) return;
            switch (Item.Kind)
            {
                case ItemKind.HealthPack:
                    renderer.material.color = RoyalColors.Success;
                    break;
                case ItemKind.ShieldCell:
                    renderer.material.color = RoyalColors.RoyalBlue;
                    break;
                case ItemKind.Ammo:
                case ItemKind.WeaponBlaster:
                case ItemKind.WeaponMelee:
                default:
                    renderer.material.color = RoyalColors.RoyalGoldBright;
                    break;
            }
        }

        private void ResolvePlayer()
        {
            ThirdPersonPlayerController controller = FindObjectOfType<ThirdPersonPlayerController>();
            if (controller != null)
            {
                playerObject = controller.gameObject;
                return;
            }
            playerObject = GameObject.Find("Player");
        }
    }
}
