namespace KobnexaRoyal
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>
    /// Royal Battle match controller (singleton). Spawns the player and
    /// BotCount bots, tracks eliminations / damage / kill feed, runs the
    /// shrinking zone (CombatMath), and ends the match on victory (all bots
    /// dead) or defeat (player dead), writing the result to RoyalGameState
    /// and showing a runtime uGUI end panel.
    ///
    /// Zone logic lives here (not a separate RoyalZone.cs) by design.
    /// The player is found by ThirdPersonPlayerController first, then by
    /// tag/name "Player"; only if none exists is a capsule "Player" created.
    /// HUD values exposed for the dedicated HUD worker: AliveCount,
    /// ZoneRadius, KillFeed.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        public int BotCount = 12;
        public Transform PlayerSpawn;

        // Zone tuning (center is fixed at world x=0, z=0).
        public float ZoneStartRadius = 90f;
        public float ZoneEndRadius = 10f;
        public float ZoneShrinkDelay = 30f;
        public float ZoneShrinkDuration = 180f;

        public Transform PlayerTransform { get; private set; }
        public HealthComponent PlayerHealth { get; private set; }

        /// <summary>Bots alive + player (1 while the player is alive).</summary>
        public int AliveCount { get { return botsAlive + (playerAlive ? 1 : 0); } }
        public float ZoneRadius { get; private set; }
        public List<string> KillFeed { get { return killFeed; } }
        public float MatchTime { get; private set; }
        public int PlayerEliminations { get; private set; }
        public float PlayerDamage { get; private set; }
        public bool MatchEnded { get; private set; }

        private readonly List<string> killFeed = new List<string>();
        private readonly List<BotController> bots = new List<BotController>();
        private int botsAlive;
        private bool playerAlive;
        private int defeatPlacement;
        private float zoneDamageTimer;
        private Text hudText;
        private Text killFeedText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Object.Destroy(gameObject);
                return;
            }
            Instance = this;
            ZoneRadius = ZoneStartRadius;
        }

        private void Start()
        {
            SetupPlayer();
            SpawnBots();
            BuildHud();
        }

        private void Update()
        {
            if (MatchEnded) return;
            MatchTime += Time.deltaTime;
            ZoneRadius = CombatMath.ZoneRadiusAt(MatchTime, ZoneShrinkDelay, ZoneShrinkDuration, ZoneStartRadius, ZoneEndRadius);
            ApplyZoneDamage();
            RefreshHud();
        }

        // ---------------- player / bots ----------------

        private void SetupPlayer()
        {
            GameObject player = FindPlayerObject();
            if (player == null)
            {
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Player";
                Collider primitiveCollider = player.GetComponent<Collider>();
                if (primitiveCollider != null) Object.Destroy(primitiveCollider);
                CapsuleCollider collider = player.AddComponent<CapsuleCollider>();
                collider.center = Vector3.zero;
                collider.height = 2f;
                collider.radius = 0.5f;
            }
            Vector3 spawn = PlayerSpawn != null ? PlayerSpawn.position : new Vector3(0f, 1f, -30f);
            player.transform.position = spawn;
            PlayerTransform = player.transform;
            PlayerHealth = player.GetComponent<HealthComponent>();
            if (PlayerHealth == null) PlayerHealth = player.GetComponentInChildren<HealthComponent>();
            if (PlayerHealth == null) PlayerHealth = player.AddComponent<HealthComponent>();
            PlayerHealth.OnDeath += OnPlayerDeath;
            playerAlive = PlayerHealth.IsAlive;
        }

        private static GameObject FindPlayerObject()
        {
            ThirdPersonPlayerController controller = FindObjectOfType<ThirdPersonPlayerController>();
            if (controller != null) return controller.gameObject;
            try
            {
                GameObject tagged = GameObject.FindWithTag("Player");
                if (tagged != null) return tagged;
            }
            catch (System.Exception)
            {
                // Tag "Player" not defined in TagManager; fall through to name lookup.
            }
            return GameObject.Find("Player");
        }

        private void SpawnBots()
        {
            int count = Mathf.Max(0, BotCount);
            for (int i = 0; i < count; i++)
            {
                string botName = BotNames.Names[i % BotNames.Names.Length];
                if (i >= BotNames.Names.Length) botName += "_" + (i / BotNames.Names.Length + 1);
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float radius = Random.Range(40f, 80f);
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 1f, Mathf.Sin(angle) * radius);
                GameObject botGo = new GameObject("Bot_" + botName);
                botGo.transform.position = pos;
                BotController bot = botGo.AddComponent<BotController>();
                bot.BotName = botName;
                bot.SetPlayerTarget(PlayerTransform);
                bots.Add(bot);
            }
            botsAlive = bots.Count;
        }

        // ---------------- match tracking ----------------

        public void AddKillFeed(string entry)
        {
            if (string.IsNullOrEmpty(entry)) return;
            killFeed.Add(entry);
            while (killFeed.Count > 8) killFeed.RemoveAt(0);
            RefreshHud();
        }

        public void RegisterPlayerKill(string victimName)
        {
            PlayerEliminations++;
            AddKillFeed("Player eliminated " + victimName);
        }

        public void RegisterPlayerDamage(float amount)
        {
            if (amount > 0f) PlayerDamage += amount;
        }

        public void BotDied(BotController bot)
        {
            if (bot == null) return;
            bots.Remove(bot);
            botsAlive = Mathf.Max(0, botsAlive - 1);
            string attacker = string.Empty;
            HealthComponent health = bot.GetComponent<HealthComponent>();
            if (health != null) attacker = health.LastAttacker;
            if (attacker == "Player")
            {
                RegisterPlayerKill(bot.BotName);
            }
            else if (attacker == "Zone")
            {
                AddKillFeed(bot.BotName + " fell to the zone");
            }
            else
            {
                AddKillFeed(bot.BotName + " was eliminated");
            }
            if (!MatchEnded && botsAlive <= 0 && playerAlive) EndMatch(true);
        }

        public void PlayerDied()
        {
            if (MatchEnded) return;
            playerAlive = false;
            defeatPlacement = AliveCount;
            EndMatch(false);
        }

        private void OnPlayerDeath(string attacker)
        {
            PlayerDied();
        }

        // ---------------- zone ----------------

        private void ApplyZoneDamage()
        {
            zoneDamageTimer += Time.deltaTime;
            if (zoneDamageTimer < 1f) return;
            zoneDamageTimer = 0f;
            float dps = CombatMath.ZoneDamagePerSecond(MatchTime);
            Vector3 center = Vector3.zero;
            if (PlayerHealth != null && PlayerHealth.IsAlive &&
                !CombatMath.IsInsideZone(PlayerTransform.position, center, ZoneRadius))
            {
                PlayerHealth.TakeDamage(dps, "Zone");
            }
            // Copy: a bot dying here removes itself from bots via BotDied.
            BotController[] snapshot = bots.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                if (snapshot[i] == null) continue;
                HealthComponent health = snapshot[i].GetComponent<HealthComponent>();
                if (health == null || !health.IsAlive) continue;
                if (!CombatMath.IsInsideZone(snapshot[i].transform.position, center, ZoneRadius))
                {
                    health.TakeDamage(dps, "Zone");
                }
            }
        }

        // ---------------- end of match ----------------

        private void EndMatch(bool won)
        {
            if (MatchEnded) return;
            MatchEnded = true;
            int placement = won ? 1 : (defeatPlacement > 0 ? defeatPlacement : AliveCount);
            MatchResultData result = new MatchResultData();
            result.Eliminations = PlayerEliminations;
            result.DamageDealt = Mathf.RoundToInt(PlayerDamage);
            result.Placement = placement;
            result.SurvivedSeconds = MatchTime;
            result.Won = won;
            RoyalGameState.LastMatchResult = result;
            int levelsGained = RoyalGameState.ApplyMatchResult(result);
            RoyalGameState.LastMatchApplied = true;
            if (won && AudioManager.Instance != null) AudioManager.Instance.PlayVictory();
            RefreshHud();
            ShowEndPanel(won, result, levelsGained);
        }

        // ---------------- runtime uGUI (minimal HUD + end panel) ----------------

        private void BuildHud()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
            Canvas canvas = FindObjectOfType<Canvas>();
            Transform canvasTransform;
            if (canvas == null)
            {
                GameObject canvasGo = new GameObject("MatchCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
                canvasGo.AddComponent<GraphicRaycaster>();
                canvasTransform = canvasGo.transform;
            }
            else
            {
                canvasTransform = canvas.transform;
            }
            hudText = UIFactory.LabelIn(canvasTransform, "MatchHud", string.Empty, 22, RoyalColors.TextWhite,
                TextAnchor.UpperLeft, true, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(16f, -90f), new Vector2(420f, 0f));
            killFeedText = UIFactory.LabelIn(canvasTransform, "KillFeed", string.Empty, 17, RoyalColors.RoyalGoldBright,
                TextAnchor.UpperRight, false, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-420f, -220f), new Vector2(-16f, 0f));
            RefreshHud();
        }

        private void RefreshHud()
        {
            if (hudText != null)
            {
                int minutes = (int)(MatchTime / 60f);
                int seconds = (int)(MatchTime % 60f);
                string zoneState = MatchTime < ZoneShrinkDelay
                    ? "Zone shrinks in " + Mathf.CeilToInt(ZoneShrinkDelay - MatchTime) + "s"
                    : "Zone shrinking";
                hudText.text = "ALIVE: " + AliveCount + "\nZONE: " + Mathf.RoundToInt(ZoneRadius) + "m - " + zoneState +
                    "\nTIME: " + minutes + ":" + seconds.ToString("D2");
            }
            if (killFeedText != null) killFeedText.text = string.Join("\n", killFeed.ToArray());
        }

        private void ShowEndPanel(bool won, MatchResultData result, int levelsGained)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;
            GameObject panel = UIFactory.Panel(canvas.transform, "MatchEndPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-360f, -260f), new Vector2(360f, 260f), RoyalColors.NavyPanel, true);
            UIFactory.LabelIn(panel.transform, "EndTitle", won ? "VICTORY ROYALE" : "DEFEAT", 44,
                won ? RoyalColors.RoyalGoldBright : RoyalColors.TextWhite, TextAnchor.UpperCenter, true,
                Vector2.zero, Vector2.one, new Vector2(0f, 440f), new Vector2(0f, -16f));
            string stats = "Placement: #" + result.Placement +
                "\nEliminations: " + result.Eliminations +
                "\nDamage dealt: " + result.DamageDealt +
                "\nSurvived: " + Mathf.RoundToInt(result.SurvivedSeconds) + "s" +
                "\nXP earned: " + result.ComputeXp() +
                "\nLevels gained: " + levelsGained;
            UIFactory.LabelIn(panel.transform, "EndStats", stats, 22, RoyalColors.TextWhite,
                TextAnchor.UpperCenter, false, Vector2.zero, Vector2.one,
                new Vector2(0f, 100f), new Vector2(0f, -110f));
            Button lobbyButton = UIFactory.Button(panel.transform, "ReturnToLobby", "RETURN TO LOBBY",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-220f, 24f), new Vector2(220f, 84f), RoyalColors.RoyalGold, 24);
            lobbyButton.onClick.AddListener(delegate
            {
                if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene("RoyalLobby");
                else if (Application.CanStreamedLevelBeLoaded("RoyalLobby")) SceneManager.LoadScene("RoyalLobby");
            });
        }
    }
}
