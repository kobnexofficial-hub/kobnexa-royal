namespace KobnexaRoyal
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// Runtime uGUI gameplay HUD, built in code (no scene prefab needed).
    /// Shows health + shield bars, ammo, clickable inventory slots, alive
    /// count, zone radius, a top-center match timer, and kill feed lines.
    /// Every read is null-safe: when a system (player, HealthComponent,
    /// RoyalBlaster, PlayerInventory, MatchManager) is absent the HUD shows
    /// "—" or an empty slot instead of throwing.
    /// </summary>
    public sealed class GameplayHUD : MonoBehaviour
    {
        private GameObject playerObject;
        private HealthComponent health;
        private RoyalBlaster blaster;
        private PlayerInventory inventory;
        private PlayerInventory subscribedInventory;

        private Transform canvasTransform;
        private Transform slotsRoot;
        private UIFactory.ProgressBar healthBar;
        private UIFactory.ProgressBar shieldBar;
        private Text healthText;
        private Text shieldText;
        private Text ammoText;
        private Text aliveText;
        private Text topCenterText;
        private Text killFeedText;
        private Text inventoryLabel;
        private readonly List<Button> slotButtons = new List<Button>();
        private GameObject pauseRoot;
        private bool isPaused;
        private string lastInventorySignature;
        private string lastTopCenter;
        private string lastAlive;
        private string lastAmmo;
        private string lastKillFeed;
        private string lastHealth;
        private string lastShield;
        private float reResolveTimer;

        private void Start()
        {
            EnsureEventSystem();
            EnsureCanvas();
            BuildHud();
            BuildPausePanel();
            ResolvePlayer();
            RefreshInventory(true);
        }

        private void Update()
        {
            // Pause: Start/Menu on a gamepad, Back/View on a gamepad, or P.
            if (ControllerInput.StartPressed || ControllerInput.BackPressed || Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
            if (playerObject == null)
            {
                reResolveTimer += Time.deltaTime;
                if (reResolveTimer >= 1f)
                {
                    reResolveTimer = 0f;
                    ResolvePlayer();
                }
            }
            else
            {
                RefreshPlayerComponents();
            }
            RefreshValues();
            RefreshInventory(false);
        }

        private void OnDestroy()
        {
            if (subscribedInventory != null) subscribedInventory.OnInventoryChanged -= RefreshInventoryFromEvent;
            Time.timeScale = 1f;
        }

        private void BuildPausePanel()
        {
            pauseRoot = UIFactory.Panel(canvasTransform, "PausePanel", Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, new Color(0.01f, 0.02f, 0.05f, 0.78f), false);
            GameObject box = UIFactory.Panel(pauseRoot.transform, "PauseBox",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-360f, -190f), new Vector2(360f, 190f), RoyalColors.NavyPanel, true);
            UIFactory.LabelIn(box.transform, "PauseTitle", "PAUSED", 36,
                RoyalColors.RoyalGoldBright, TextAnchor.UpperCenter, true,
                Vector2.zero, Vector2.one, new Vector2(24f, -70f), new Vector2(-24f, 0f));
            UIFactory.LabelIn(box.transform, "PauseHint", "Start / P resumes. Controller: D-pad moves, A confirms.",
                17, RoyalColors.TextMuted, TextAnchor.UpperCenter, false,
                Vector2.zero, new Vector2(1f, 0.72f), new Vector2(32f, 70f), new Vector2(-32f, 0f));
            Button resume = UIFactory.Button(box.transform, "PauseResume", "RESUME",
                new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.48f),
                Vector2.zero, Vector2.zero, RoyalColors.RoyalGold, 21);
            UIFactory.SetButtonText(resume, "RESUME", new Color(0.08f, 0.06f, 0.01f, 1f));
            resume.onClick.AddListener(TogglePause);
            Button quit = UIFactory.Button(box.transform, "PauseQuitToLobby", "QUIT TO LOBBY",
                new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.26f),
                Vector2.zero, Vector2.zero, RoyalColors.NavyPanelLight, 21);
            quit.onClick.AddListener(QuitToLobby);
            pauseRoot.SetActive(false);
        }

        private void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            if (pauseRoot != null) pauseRoot.SetActive(isPaused);
            if (isPaused && EventSystem.current != null)
            {
                Button resume = pauseRoot != null
                    ? pauseRoot.GetComponentInChildren<Button>(true) : null;
                if (resume != null) resume.Select();
            }
        }

        private void QuitToLobby()
        {
            Time.timeScale = 1f;
            isPaused = false;
            SceneLoader loader = SceneLoader.Instance;
            if (loader != null) loader.LoadScene("RoyalLobby");
            else UnityEngine.SceneManagement.SceneManager.LoadScene("RoyalLobby");
        }

        // ---------------- construction ----------------

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        private void EnsureCanvas()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                canvasTransform = canvas.transform;
                return;
            }
            GameObject canvasGo = new GameObject("GameplayHUDCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasTransform = canvasGo.transform;
        }

        private void BuildHud()
        {
            // Top-center: match timer + zone radius.
            topCenterText = UIFactory.LabelIn(canvasTransform, "HudTopCenter", "—", 26,
                RoyalColors.RoyalGoldBright, TextAnchor.MiddleCenter, true,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-450f, -70f), new Vector2(450f, 0f));

            // Alive count (top-left, below MatchManager's own minimal HUD text).
            aliveText = UIFactory.LabelIn(canvasTransform, "HudAlive", "ALIVE: —", 22,
                RoyalColors.TextWhite, TextAnchor.MiddleLeft, true,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(16f, -140f), new Vector2(420f, -100f));

            // Kill feed (top-right, below MatchManager's own kill feed block).
            killFeedText = UIFactory.LabelIn(canvasTransform, "HudKillFeed", string.Empty, 17,
                RoyalColors.RoyalGoldBright, TextAnchor.UpperRight, false,
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-520f, -480f), new Vector2(-20f, -230f));

            // Health + shield bars (bottom-left) with numeric labels.
            healthBar = UIFactory.Bar(canvasTransform, "HudHealthBar",
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(20f, 112f), new Vector2(320f, 136f), RoyalColors.Success, 1f);
            healthText = UIFactory.LabelIn(canvasTransform, "HudHealthText", "HP —", 20,
                RoyalColors.TextWhite, TextAnchor.MiddleLeft, false,
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(330f, 112f), new Vector2(520f, 136f));
            shieldBar = UIFactory.Bar(canvasTransform, "HudShieldBar",
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(20f, 142f), new Vector2(320f, 162f), RoyalColors.RoyalBlue, 0f);
            shieldText = UIFactory.LabelIn(canvasTransform, "HudShieldText", "SHIELD —", 20,
                RoyalColors.TextWhite, TextAnchor.MiddleLeft, false,
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(330f, 142f), new Vector2(520f, 162f));

            // Ammo (bottom-right).
            ammoText = UIFactory.LabelIn(canvasTransform, "HudAmmo", "AMMO —", 24,
                RoyalColors.RoyalGoldBright, TextAnchor.MiddleRight, true,
                new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-340f, 20f), new Vector2(-20f, 60f));

            // Inventory label + slot row (bottom-left, above the bars).
            inventoryLabel = UIFactory.LabelIn(canvasTransform, "HudInventoryLabel",
                "INVENTORY (click a slot to use it)", 17,
                RoyalColors.TextMuted, TextAnchor.MiddleLeft, false,
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(20f, 76f), new Vector2(460f, 102f));
            slotsRoot = UIFactory.Rect(canvasTransform, "HudInventorySlots",
                new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, Vector2.zero);
        }

        // ---------------- player wiring ----------------

        private void ResolvePlayer()
        {
            ThirdPersonPlayerController controller = FindObjectOfType<ThirdPersonPlayerController>();
            if (controller != null) playerObject = controller.gameObject;
            else if (MatchManager.Instance != null && MatchManager.Instance.PlayerTransform != null)
                playerObject = MatchManager.Instance.PlayerTransform.gameObject;
            else playerObject = GameObject.Find("Player");
            if (playerObject != null) RefreshPlayerComponents();
        }

        private void RefreshPlayerComponents()
        {
            if (playerObject == null) return;
            if (health == null) health = playerObject.GetComponent<HealthComponent>();
            if (health == null) health = playerObject.GetComponentInChildren<HealthComponent>();
            if (blaster == null) blaster = playerObject.GetComponent<RoyalBlaster>();
            if (blaster == null) blaster = playerObject.GetComponentInChildren<RoyalBlaster>();
            if (inventory == null) inventory = playerObject.GetComponent<PlayerInventory>();
            if (inventory == null) inventory = FindObjectOfType<PlayerInventory>();
            if (inventory != null && subscribedInventory != inventory)
            {
                if (subscribedInventory != null) subscribedInventory.OnInventoryChanged -= RefreshInventoryFromEvent;
                inventory.OnInventoryChanged += RefreshInventoryFromEvent;
                subscribedInventory = inventory;
            }
        }

        // ---------------- per-frame values ----------------

        private void RefreshValues()
        {
            if (health != null)
            {
                float maxHealth = Mathf.Max(1f, health.MaxHealth);
                float maxShield = Mathf.Max(1f, health.MaxShield);
                if (healthBar != null) healthBar.Set(health.Health / maxHealth);
                if (shieldBar != null) shieldBar.Set(health.Shield / maxShield);
                SetText(ref lastHealth, healthText, "HP " + Mathf.RoundToInt(health.Health) + "/" + Mathf.RoundToInt(health.MaxHealth));
                SetText(ref lastShield, shieldText, "SHIELD " + Mathf.RoundToInt(health.Shield) + "/" + Mathf.RoundToInt(health.MaxShield));
            }
            else
            {
                if (healthBar != null) healthBar.Set(0f);
                if (shieldBar != null) shieldBar.Set(0f);
                SetText(ref lastHealth, healthText, "HP —");
                SetText(ref lastShield, shieldText, "SHIELD —");
            }

            if (blaster != null)
                SetText(ref lastAmmo, ammoText, "AMMO " + blaster.AmmoInMag + " / " + blaster.ReserveAmmo);
            else
                SetText(ref lastAmmo, ammoText, "AMMO —");

            MatchManager match = MatchManager.Instance;
            if (match != null)
            {
                int minutes = (int)(match.MatchTime / 60f);
                int seconds = (int)(match.MatchTime % 60f);
                SetText(ref lastTopCenter, topCenterText,
                    minutes + ":" + seconds.ToString("D2") + "  •  ZONE " + Mathf.RoundToInt(match.ZoneRadius) + "m");
                SetText(ref lastAlive, aliveText, "ALIVE: " + match.AliveCount);
                SetText(ref lastKillFeed, killFeedText, BuildKillFeedText(match));
            }
            else
            {
                int total = (int)Time.timeSinceLevelLoad;
                SetText(ref lastTopCenter, topCenterText, (total / 60) + ":" + (total % 60).ToString("D2") + "  •  ZONE —");
                SetText(ref lastAlive, aliveText, "ALIVE: —");
                SetText(ref lastKillFeed, killFeedText, string.Empty);
            }
        }

        private static string BuildKillFeedText(MatchManager match)
        {
            if (match == null || match.KillFeed == null || match.KillFeed.Count == 0) return string.Empty;
            List<string> lines = match.KillFeed;
            int start = Mathf.Max(0, lines.Count - 5);
            string text = string.Empty;
            for (int i = start; i < lines.Count; i++)
            {
                if (i > start) text += "\n";
                text += lines[i];
            }
            return text;
        }

        private static void SetText(ref string last, Text label, string value)
        {
            if (label == null) return;
            if (last == value) return;
            last = value;
            label.text = value;
        }

        // ---------------- inventory slots ----------------

        private void RefreshInventoryFromEvent()
        {
            RefreshInventory(true);
        }

        private void RefreshInventory(bool force)
        {
            string signature = BuildInventorySignature();
            if (!force && signature == lastInventorySignature) return;
            lastInventorySignature = signature;

            for (int i = 0; i < slotButtons.Count; i++)
            {
                if (slotButtons[i] != null) Object.Destroy(slotButtons[i].gameObject);
            }
            slotButtons.Clear();

            InventoryData data = inventory != null ? inventory.Data : null;
            if (inventoryLabel != null)
                inventoryLabel.text = data == null ? "INVENTORY UNAVAILABLE" : "INVENTORY (click a slot to use it)";

            for (int i = 0; i < InventoryData.Capacity; i++)
            {
                InventoryItem item = data != null ? data.Slot(i) : null;
                float x0 = 20f + i * 112f;
                Button button;
                if (item != null)
                {
                    string label = item.Name + (item.Amount > 1 ? " x" + item.Amount : string.Empty);
                    button = UIFactory.Button(slotsRoot, "HudSlot_" + i, label,
                        new Vector2(0f, 0f), new Vector2(0f, 0f),
                        new Vector2(x0, 16f), new Vector2(x0 + 104f, 72f),
                        RoyalColors.NavyPanelLight, 15);
                    int captured = i;
                    button.onClick.AddListener(() => UseSlot(captured));
                }
                else
                {
                    button = UIFactory.Button(slotsRoot, "HudSlot_" + i, "Empty",
                        new Vector2(0f, 0f), new Vector2(0f, 0f),
                        new Vector2(x0, 16f), new Vector2(x0 + 104f, 72f),
                        RoyalColors.NavyPanel, 15);
                    button.interactable = false;
                }
                slotButtons.Add(button);
            }
        }

        private string BuildInventorySignature()
        {
            if (inventory == null || inventory.Data == null) return "none";
            InventoryData data = inventory.Data;
            string signature = "count=" + data.Count;
            for (int i = 0; i < data.Count; i++)
            {
                InventoryItem item = data.Slot(i);
                if (item == null) continue;
                signature += "|" + item.Kind + ":" + item.Name + "x" + item.Amount;
            }
            return signature;
        }

        private void UseSlot(int index)
        {
            if (inventory == null) return;
            if (health == null && playerObject != null) health = playerObject.GetComponent<HealthComponent>();
            inventory.UseItem(index, health);
            RefreshInventory(true);
        }
    }
}
