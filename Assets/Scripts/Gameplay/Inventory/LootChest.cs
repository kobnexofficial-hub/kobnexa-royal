namespace KobnexaRoyal
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A loot chest built from primitives (box + gold lid) with a trigger zone.
    /// Press E while the player is near to open it exactly once: it grants 2-3
    /// random items to the player's PlayerInventory, records the chest in quest
    /// progress, saves, plays the pickup sound and shows "Chest opened!".
    /// If the player has no PlayerInventory the loot is only logged, never lost
    /// silently: the chest still counts as opened.
    /// </summary>
    public sealed class LootChest : MonoBehaviour
    {
        public bool IsOpened;

        private GameObject lid;
        private GameObject playerObject;
        private bool playerNear;
        private float reResolveTimer;

        private void Awake()
        {
            BuildChest();
        }

        private void Start()
        {
            ResolvePlayer();
        }

        private void Update()
        {
            if (IsOpened) return;
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
            if (!playerNear)
            {
                playerNear = Vector3.Distance(playerObject.transform.position, transform.position) <= 3f;
            }
            if (playerNear && Input.GetKeyDown(KeyCode.E))
            {
                Open();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsPlayerCollider(other)) playerNear = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsPlayerCollider(other)) playerNear = false;
        }

        /// <summary>Opens the chest once. Safe to call directly (e.g. from tests or UI).</summary>
        public void Open()
        {
            if (IsOpened) return;
            IsOpened = true;
            if (lid != null) lid.transform.Rotate(-80f, 0f, 0f);

            if (playerObject == null) ResolvePlayer();
            PlayerInventory inventory = null;
            if (playerObject != null) inventory = playerObject.GetComponent<PlayerInventory>();
            if (inventory == null) inventory = FindObjectOfType<PlayerInventory>();

            int count = Random.Range(2, 4);
            for (int i = 0; i < count; i++)
            {
                InventoryItem item = RollLoot();
                if (inventory != null)
                {
                    if (!inventory.AddItem(item))
                        Debug.Log("[KOBNEXA ROYAL] Chest loot did not fit: " + item.Name + " (inventory full).");
                }
                else
                {
                    Debug.Log("[KOBNEXA ROYAL] Chest loot (no PlayerInventory found, not granted): " + item.Name);
                }
            }

            if (RoyalGameState.QuestProgress != null) RoyalGameState.QuestProgress.ChestsOpened++;
            RoyalGameState.Save();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayPickup();
            ShowOpenedMessage();
        }

        private static InventoryItem RollLoot()
        {
            int roll = Random.Range(0, 4);
            switch (roll)
            {
                case 0:
                    return new InventoryItem { Kind = ItemKind.Ammo, Name = "Blaster Ammo", Amount = Random.Range(12, 31) };
                case 1:
                    return new InventoryItem { Kind = ItemKind.HealthPack, Name = "Health Pack", Amount = 1 };
                case 2:
                    return new InventoryItem { Kind = ItemKind.ShieldCell, Name = "Shield Cell", Amount = 1 };
                default:
                    if (Random.Range(0, 2) == 0)
                        return new InventoryItem { Kind = ItemKind.WeaponBlaster, Name = "Royal Blaster", Amount = 1 };
                    return new InventoryItem { Kind = ItemKind.WeaponMelee, Name = "Royal Blade", Amount = 1 };
            }
        }

        private void ShowOpenedMessage()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.Log("[KOBNEXA ROYAL] Chest opened!");
                return;
            }
            Text message = UIFactory.LabelIn(canvas.transform, "ChestOpenedMessage", "Chest opened!", 28,
                RoyalColors.RoyalGoldBright, TextAnchor.MiddleCenter, true,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-220f, -30f), new Vector2(220f, 30f));
            Object.Destroy(message.gameObject, 2f);
            Debug.Log("[KOBNEXA ROYAL] Chest opened!");
        }

        private void BuildChest()
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "ChestBox";
            box.transform.SetParent(transform, false);
            box.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            box.transform.localScale = new Vector3(1.4f, 0.8f, 1f);
            Renderer boxRenderer = box.GetComponent<Renderer>();
            if (boxRenderer != null) boxRenderer.material.color = new Color(0.45f, 0.28f, 0.12f, 1f);

            lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lid.name = "ChestLid";
            lid.transform.SetParent(transform, false);
            lid.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            lid.transform.localScale = new Vector3(1.5f, 0.25f, 1.1f);
            Renderer lidRenderer = lid.GetComponent<Renderer>();
            if (lidRenderer != null) lidRenderer.material.color = RoyalColors.RoyalGoldBright;

            // Trigger on this same GameObject so OnTriggerEnter reaches this
            // script (a trigger on a child without a Rigidbody would not).
            // The distance check in Update is the fallback if triggers miss.
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(3f, 2.5f, 3f);
            collider.center = new Vector3(0f, 1f, 0f);
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

        private bool IsPlayerCollider(Collider other)
        {
            if (other == null) return false;
            Transform t = other.transform;
            while (t != null)
            {
                if (playerObject != null && t.gameObject == playerObject) return true;
                if (t.GetComponent<ThirdPersonPlayerController>() != null) return true;
                if (t.name == "Player") return true;
                t = t.parent;
            }
            return false;
        }
    }
}
