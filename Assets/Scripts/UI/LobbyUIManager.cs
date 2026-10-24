namespace KobnexaRoyal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// Builds and drives the complete Royal Lobby UI at runtime, matching the
    /// approved reference layout. Presentation + navigation only: game data comes
    /// from PrototypeCatalog / PlayerProfileData, settings from SettingsManager.
    /// Anything not backed by a real service is honestly labeled a prototype.
    /// </summary>
    public sealed class LobbyUIManager : MonoBehaviour
    {
        private const string PlayId = "Play";
        private const string LockerId = "Locker";
        private const string StoreId = "Store";
        private const string BattlePassId = "BattlePass";
        private const string QuestsId = "Quests";
        private const string CareerId = "Career";
        private const string FriendsId = "Friends";
        private const string SettingsId = "Settings";

        private readonly MenuNavigationStack nav = new MenuNavigationStack();
        private readonly Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Button> navButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, string> navLabels = new Dictionary<string, string>();

        private PlayerProfileData profile;
        private List<PrototypeCatalog.StoreItem> storeItems;
        private List<PrototypeCatalog.Quest> quests;
        private List<PrototypeCatalog.LockerItem> lockerItems;
        private int selectedModeIndex = 0;
        private static readonly string[] ModeNames = { "ROYAL BATTLE - Solo", "ROYAL BATTLE - Duo (prototype)", "ROYAL BATTLE - Squad (prototype)" };

        private GameObject homeRoot;
        private GameObject dialogRoot;
        private Text statusText;
        private Text coinsText;
        private Text gemsText;
        private Text profileLevelText;
        private Text profileXpText;
        private UIFactory.ProgressBar profileXpBar;
        private Text heroNameText;
        private Text selectedOutfitText;
        private Text countdownText;
        private Text dialogMessageText;
        private Action dialogConfirm;

        // Settings controls
        private Slider masterSlider, musicSlider, sfxSlider, sensitivitySlider;
        private Dropdown qualityDropdown;
        private Toggle fullscreenToggle, invertYToggle;
        private Text sensitivityValueText;

        private LobbyCharacterShowcase showcase;

        // Friends / party roster UI (rebuilt when the party changes).
        private Text partyRosterText;
        private RectTransform friendsContent;
        private readonly List<GameObject> partyRosterRows = new List<GameObject>();

        private void Awake()
        {
            RoyalGameState.Load();
            profile = RoyalGameState.CurrentProfile;
            storeItems = PrototypeCatalog.StoreItems();
            quests = PrototypeCatalog.Quests();
            lockerItems = PrototypeCatalog.LockerItems();
            SyncQuestsFromProgress();
        }

        /// <summary>Copies persistent quest progress/claimed flags into the lobby's quest list.</summary>
        private void SyncQuestsFromProgress()
        {
            if (quests == null || RoyalGameState.QuestProgress == null) return;
            for (int i = 0; i < quests.Count; i++)
            {
                quests[i].Progress = RoyalGameState.QuestProgress.ProgressFor(quests[i].Name);
                quests[i].Claimed = RoyalGameState.QuestProgress.IsClaimed(quests[i].Name);
            }
        }

        /// <summary>
        /// Applies a finished match exactly once (only when it has not been
        /// applied yet) and returns the summary line whenever a last match
        /// result exists, or null when there is none. The summary is shown
        /// even for an already-applied result without re-applying it, so no
        /// XP is ever granted twice: MatchManager.EndMatch already applies
        /// its own result and marks it applied before returning to the lobby.
        /// </summary>
        private string ApplyPendingMatchResult()
        {
            MatchResultData result = RoyalGameState.LastMatchResult;
            if (result == null) return null;
            if (!RoyalGameState.LastMatchApplied)
            {
                RoyalGameState.ApplyMatchResult(result);
                RoyalGameState.LastMatchApplied = true;
                SyncQuestsFromProgress();
            }
            string summary = (result.Won ? "Victory Royale! " : "") + "Last match: " + result.Eliminations
                + " eliminations, placement #" + result.Placement + ", +" + result.ComputeXp() + " XP.";
            return summary;
        }

        private void Start()
        {
            showcase = FindObjectOfType<LobbyCharacterShowcase>();
            EnsureEventSystem();
            string matchSummary = ApplyPendingMatchResult();
            BuildUi();
            ShowPanel(MenuNavigationStack.HomePanelId, true);
            RefreshProfileUi();
            if (matchSummary != null) SetStatus(matchSummary);
            else SetStatus("Welcome to KOBNEXA ROYAL - Mission 01 Royal Lobby prototype.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (dialogRoot != null && dialogRoot.activeSelf) CloseDialog(false);
                else GoBack();
            }
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        // ---------------- construction ----------------

        private void BuildUi()
        {
            GameObject canvasGo = new GameObject("LobbyCanvas");
            canvasGo.transform.SetParent(transform, false);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            homeRoot = UIFactory.FullRect(canvasGo.transform, "Home").gameObject;
            BuildTopBar(homeRoot.transform);
            BuildHomeCards(homeRoot.transform);

            BuildPlayPanel(canvasGo.transform);
            BuildLockerPanel(canvasGo.transform);
            BuildStorePanel(canvasGo.transform);
            BuildBattlePassPanel(canvasGo.transform);
            BuildQuestsPanel(canvasGo.transform);
            BuildCareerPanel(canvasGo.transform);
            BuildFriendsPanel(canvasGo.transform);
            BuildSettingsPanel(canvasGo.transform);
            BuildDialog(canvasGo.transform);
        }

        private void BuildTopBar(Transform parent)
        {
            GameObject bar = UIFactory.Panel(parent, "TopBar", new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -96f), Vector2.zero, new Color(0.02f, 0.03f, 0.08f, 0.88f), false);
            Transform bt = bar.transform;

            UIFactory.LabelIn(bt, "Logo", "KOBNEXA ROYAL", 40, RoyalColors.RoyalGoldBright, TextAnchor.MiddleLeft, true, new Vector2(0f, 0f), new Vector2(0.24f, 1f), new Vector2(24f, 0f), Vector2.zero);
            UIFactory.LabelIn(bt, "LogoSub", "THE ROYAL ERA - SEASON 1", 13, RoyalColors.TextMuted, TextAnchor.UpperLeft, false, new Vector2(0f, 0f), new Vector2(0.24f, 0.45f), new Vector2(26f, 0f), Vector2.zero);

            // Center nav, exactly the reference order.
            GameObject navBar = UIFactory.Panel(bt, "NavBar", new Vector2(0.255f, 0.16f), new Vector2(0.795f, 0.84f), Vector2.zero, Vector2.zero, new Color(0.03f, 0.05f, 0.12f, 0.9f), false);
            string[] ids = { PlayId, LockerId, StoreId, BattlePassId, QuestsId, CareerId, FriendsId, SettingsId };
            string[] labels = { "PLAY", "LOCKER", "STORE", "BATTLE PASS", "QUESTS", "CAREER", "FRIENDS", "SETTINGS" };
            float step = 1f / ids.Length;
            for (int i = 0; i < ids.Length; i++)
            {
                string id = ids[i];
                Color normal = id == PlayId ? RoyalColors.RoyalGold : new Color(0.05f, 0.08f, 0.18f, 1f);
                Button b = UIFactory.Button(navBar.transform, "Nav_" + id, labels[i], new Vector2(step * i + 0.004f, 0.08f), new Vector2(step * (i + 1) - 0.004f, 0.92f), Vector2.zero, Vector2.zero, normal, 19);
                if (id == PlayId) UIFactory.SetButtonText(b, labels[i], new Color(0.08f, 0.06f, 0.01f, 1f));
                b.onClick.AddListener(() => OpenSection(id));
                navButtons[id] = b;
                navLabels[id] = labels[i];
            }

            coinsText = UIFactory.LabelIn(bt, "Coins", string.Empty, 24, RoyalColors.RoyalGoldBright, TextAnchor.MiddleRight, true, new Vector2(0.815f, 0f), new Vector2(0.885f, 1f), Vector2.zero, Vector2.zero);
            gemsText = UIFactory.LabelIn(bt, "Gems", string.Empty, 24, new Color(0.75f, 0.55f, 1f), TextAnchor.MiddleRight, true, new Vector2(0.887f, 0f), new Vector2(0.935f, 1f), Vector2.zero, Vector2.zero);
            GameObject avatar = UIFactory.Panel(bt, "Avatar", new Vector2(0.945f, 0.08f), new Vector2(0.985f, 0.92f), Vector2.zero, Vector2.zero, RoyalColors.NavyPanelLight, true);
            UIFactory.Label(avatar.transform, "AvatarInitial", "K", 30, RoyalColors.RoyalGoldBright, TextAnchor.MiddleCenter, true);
        }

        private void BuildHomeCards(Transform parent)
        {
            // Left: profile card.
            GameObject profileCard = UIFactory.Panel(parent, "ProfileCard", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -218f), new Vector2(388f, -112f), RoyalColors.NavyPanel, true);
            Transform pt = profileCard.transform;
            GameObject crownBox = UIFactory.Panel(pt, "CrownBox", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(10f, 12f), new Vector2(78f, -12f), RoyalColors.RoyalGold, false);
            UIFactory.Label(crownBox.transform, "CrownText", "K", 34, new Color(0.08f, 0.06f, 0.01f), TextAnchor.MiddleCenter, true);
            UIFactory.LabelIn(pt, "ProfileName", profile.DisplayName, 24, RoyalColors.TextWhite, TextAnchor.UpperLeft, true, Vector2.zero, Vector2.one, new Vector2(94f, -6f), Vector2.zero);
            profileLevelText = UIFactory.LabelIn(pt, "ProfileLevel", string.Empty, 19, RoyalColors.TextMuted, TextAnchor.UpperLeft, false, Vector2.zero, new Vector2(1f, 0.72f), new Vector2(94f, 0f), Vector2.zero);
            profileXpBar = UIFactory.Bar(pt, "ProfileXpBar", Vector2.zero, Vector2.one, new Vector2(94f, 12f), new Vector2(-14f, 30f), RoyalColors.RoyalBlue, 0f);
            profileXpText = UIFactory.LabelIn(pt, "ProfileXpText", string.Empty, 15, RoyalColors.TextWhite, TextAnchor.MiddleRight, false, Vector2.zero, Vector2.one, new Vector2(0f, 10f), new Vector2(-14f, 34f));

            // Left: season card.
            GameObject season = UIFactory.Panel(parent, "SeasonCard", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -478f), new Vector2(388f, -234f), new Color(0.04f, 0.10f, 0.30f, 0.97f), true);
            Transform st = season.transform;
            UIFactory.LabelIn(st, "SeasonTitle", "SEASON 1", 40, RoyalColors.TextWhite, TextAnchor.LowerCenter, true, new Vector2(0f, 0.38f), Vector2.one, Vector2.zero, Vector2.zero);
            UIFactory.LabelIn(st, "SeasonSub", "THE ROYAL ERA", 21, RoyalColors.RoyalGoldBright, TextAnchor.UpperCenter, true, new Vector2(0f, 0.30f), new Vector2(1f, 0.48f), Vector2.zero, Vector2.zero);
            Button passBtn = UIFactory.Button(st, "ViewBattlePass", "VIEW BATTLE PASS >", new Vector2(0.06f, 0.07f), new Vector2(0.94f, 0.26f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.08f, 1f), 19);
            passBtn.onClick.AddListener(() => OpenSection(BattlePassId));

            // Left: play card.
            GameObject playCard = UIFactory.Panel(parent, "PlayCard", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 116f), new Vector2(388f, 468f), RoyalColors.NavyPanel, true);
            Transform pct = playCard.transform;
            UIFactory.LabelIn(pct, "PlayHeader", "PLAY", 26, RoyalColors.TextWhite, TextAnchor.UpperLeft, true, Vector2.zero, Vector2.one, new Vector2(16f, -8f), Vector2.zero);
            GameObject modeCard = UIFactory.Panel(pct, "ModeCard", new Vector2(0.045f, 0.46f), new Vector2(0.955f, 0.84f), Vector2.zero, Vector2.zero, new Color(0.06f, 0.13f, 0.35f, 1f), true);
            UIFactory.LabelIn(modeCard.transform, "ModeName", "ROYAL BATTLE", 27, RoyalColors.TextWhite, TextAnchor.LowerCenter, true, new Vector2(0f, 0.18f), Vector2.one, Vector2.zero, Vector2.zero);
            UIFactory.LabelIn(modeCard.transform, "ModeSub", "Battle Royale - Solo", 18, RoyalColors.TextMuted, TextAnchor.UpperCenter, false, Vector2.zero, new Vector2(1f, 0.30f), Vector2.zero, Vector2.zero);
            Button changeMode = UIFactory.Button(pct, "ChangeMode", "CHANGE MODE", new Vector2(0.045f, 0.315f), new Vector2(0.955f, 0.435f), Vector2.zero, Vector2.zero, new Color(0.03f, 0.05f, 0.13f, 1f), 19);
            changeMode.onClick.AddListener(() => OpenSection(PlayId));
            Button bigPlay = UIFactory.Button(pct, "BigPlay", "PLAY", new Vector2(0.045f, 0.045f), new Vector2(0.955f, 0.285f), Vector2.zero, Vector2.zero, RoyalColors.RoyalGold, 38);
            UIFactory.SetButtonText(bigPlay, "PLAY", new Color(0.08f, 0.06f, 0.01f, 1f));
            bigPlay.onClick.AddListener(RequestPlay);

            // Center: hero nameplate + party slots.
            heroNameText = UIFactory.LabelIn(parent, "HeroName", profile.DisplayName, 27, RoyalColors.TextWhite, TextAnchor.LowerCenter, true, new Vector2(0.5f, 0.70f), new Vector2(0.5f, 0.70f), new Vector2(-220f, 0f), new Vector2(220f, 38f));
            UIFactory.LabelIn(parent, "HeroStatus", "Ready", 22, RoyalColors.Success, TextAnchor.UpperCenter, true, new Vector2(0.5f, 0.70f), new Vector2(0.5f, 0.70f), new Vector2(-220f, -30f), new Vector2(220f, 2f));
            Button plusL = UIFactory.Button(parent, "PartySlotLeft", "+", new Vector2(0.365f, 0.40f), new Vector2(0.395f, 0.40f), new Vector2(-27f, -27f), new Vector2(27f, 27f), new Color(0.02f, 0.03f, 0.08f, 0.92f), 34);
            plusL.onClick.AddListener(() => OpenSection(FriendsId));
            Button plusR = UIFactory.Button(parent, "PartySlotRight", "+", new Vector2(0.605f, 0.40f), new Vector2(0.635f, 0.40f), new Vector2(-27f, -27f), new Vector2(27f, 27f), new Color(0.02f, 0.03f, 0.08f, 0.92f), 34);
            plusR.onClick.AddListener(() => OpenSection(FriendsId));

            countdownText = UIFactory.LabelIn(parent, "Countdown", string.Empty, 90, RoyalColors.RoyalGoldBright, TextAnchor.MiddleCenter, true, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(-200f, -70f), new Vector2(200f, 70f));

            // Right: daily quests card.
            GameObject questCard = UIFactory.Panel(parent, "DailyQuestsCard", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-388f, -430f), new Vector2(-20f, -112f), RoyalColors.NavyPanel, true);
            Transform qt = questCard.transform;
            UIFactory.LabelIn(qt, "DailyHeader", "DAILY QUESTS", 23, RoyalColors.RoyalGoldBright, TextAnchor.UpperLeft, true, Vector2.zero, Vector2.one, new Vector2(16f, -8f), Vector2.zero);
            for (int i = 0; i < quests.Count; i++)
            {
                PrototypeCatalog.Quest q = quests[i];
                float rowTop = -54f - i * 74f;
                GameObject row = UIFactory.Panel(qt, "DailyQuestRow_" + i, new Vector2(0f, 1f), Vector2.one, new Vector2(12f, rowTop - 66f), new Vector2(-12f, rowTop), new Color(0.03f, 0.05f, 0.13f, 0.9f), false);
                Transform rowT = row.transform;
                UIFactory.LabelIn(rowT, "QuestName", q.Name, 19, RoyalColors.TextWhite, TextAnchor.UpperLeft, false, Vector2.zero, Vector2.one, new Vector2(10f, -4f), Vector2.zero);
                UIFactory.LabelIn(rowT, "QuestReward", "1K XP", 17, new Color(0.75f, 0.55f, 1f), TextAnchor.UpperRight, true, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10f, -4f));
                UIFactory.Bar(rowT, "QuestBar", Vector2.zero, Vector2.one, new Vector2(10f, 8f), new Vector2(-96f, 22f), RoyalColors.RoyalBlue, q.Target > 0 ? (float)q.Progress / q.Target : 0f);
                UIFactory.LabelIn(rowT, "QuestProgress", q.Progress + " / " + q.Target, 16, RoyalColors.TextMuted, TextAnchor.MiddleRight, false, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10f, 2f));
            }
            Button allQuests = UIFactory.Button(qt, "ViewAllQuests", "VIEW ALL QUESTS >", new Vector2(0.05f, 0.035f), new Vector2(0.95f, 0.145f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.08f, 1f), 18);
            allQuests.onClick.AddListener(() => OpenSection(QuestsId));

            // Right: store promo card.
            GameObject storeCard = UIFactory.Panel(parent, "StorePromoCard", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-388f, 268f), new Vector2(-20f, 430f), new Color(0.16f, 0.06f, 0.35f, 0.97f), true);
            Transform sct = storeCard.transform;
            UIFactory.LabelIn(sct, "StorePromoTitle", "KOBNEXA ROYAL STORE", 24, RoyalColors.RoyalGoldBright, TextAnchor.Center, true, new Vector2(0f, 0.42f), Vector2.one, Vector2.zero, Vector2.zero);
            Button storeBtn = UIFactory.Button(sct, "GetItems", "GET EXCLUSIVE ITEMS!", new Vector2(0.07f, 0.10f), new Vector2(0.93f, 0.36f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.08f, 1f), 18);
            storeBtn.onClick.AddListener(() => OpenSection(StoreId));

            // Right: next reward card.
            GameObject reward = UIFactory.Panel(parent, "NextRewardCard", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-388f, 116f), new Vector2(-20f, 252f), RoyalColors.NavyPanel, true);
            Transform rt2 = reward.transform;
            UIFactory.LabelIn(rt2, "NextRewardTitle", "NEXT REWARD", 17, RoyalColors.TextMuted, TextAnchor.UpperLeft, true, Vector2.zero, Vector2.one, new Vector2(16f, -8f), Vector2.zero);
            UIFactory.LabelIn(rt2, "NextRewardLevel", "LVL 2", 27, RoyalColors.RoyalGoldBright, TextAnchor.UpperLeft, true, Vector2.zero, new Vector2(1f, 0.68f), new Vector2(16f, 0f), Vector2.zero);
            UIFactory.Bar(rt2, "NextRewardBar", Vector2.zero, Vector2.one, new Vector2(16f, 14f), new Vector2(-120f, 30f), RoyalColors.RoyalBlue, 0f);
            UIFactory.LabelIn(rt2, "NextRewardXp", "0 / 10,000 XP", 16, RoyalColors.TextWhite, TextAnchor.MiddleRight, false, Vector2.zero, Vector2.one, new Vector2(0f, 10f), new Vector2(-14f, 36f));

            // Bottom-left session info + bottom status + bottom-right actions.
            UIFactory.LabelIn(parent, "SessionInfo", "Game Mode: Royal Battle\nRegion: Auto", 17, RoyalColors.TextWhite, TextAnchor.LowerLeft, false, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 14f), new Vector2(320f, 74f));
            statusText = UIFactory.LabelIn(parent, "Status", string.Empty, 18, RoyalColors.RoyalGoldBright, TextAnchor.MiddleCenter, false, new Vector2(0.30f, 0f), new Vector2(0.70f, 0f), new Vector2(0f, 12f), new Vector2(0f, 48f));
            Button chatBtn = UIFactory.Button(parent, "ChatButton", "Chat (Hold)", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-560f, 16f), new Vector2(-386f, 62f), new Color(0.02f, 0.03f, 0.08f, 0.92f), 18);
            chatBtn.onClick.AddListener(() => SetStatus("Chat is a prototype placeholder - text and voice chat arrive with the multiplayer milestone."));
            Button emoteBtn = UIFactory.Button(parent, "EmoteButton", "Emote (Hold)", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-376f, 16f), new Vector2(-202f, 62f), new Color(0.02f, 0.03f, 0.08f, 0.92f), 18);
            emoteBtn.onClick.AddListener(PlayEmote);
            Button backBtn = UIFactory.Button(parent, "BackButton", "Back", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-192f, 16f), new Vector2(-20f, 62f), new Color(0.02f, 0.03f, 0.08f, 0.92f), 18);
            backBtn.onClick.AddListener(GoBack);
        }

        // ---------------- section overlay panels ----------------

        private GameObject SectionPanel(Transform canvasRoot, string id, string title)
        {
            GameObject panel = UIFactory.Panel(canvasRoot, "Panel_" + id, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.02f, 0.04f, 0.10f, 0.97f), false);
            UIFactory.LabelIn(panel.transform, "Title", title, 36, RoyalColors.RoyalGoldBright, TextAnchor.UpperLeft, true, new Vector2(0f, 1f), Vector2.one, new Vector2(48f, -78f), new Vector2(700f, -16f));
            Button back = UIFactory.Button(panel.transform, "SectionBack", "< BACK", new Vector2(1f, 1f), Vector2.one, new Vector2(-240f, -78f), new Vector2(-48f, -22f), new Color(0.05f, 0.08f, 0.18f, 1f), 20);
            back.onClick.AddListener(GoBack);
            panel.SetActive(false);
            panels[id] = panel;
            return panel;
        }

        private void BuildPlayPanel(Transform canvasRoot)
        {
            GameObject panel = SectionPanel(canvasRoot, PlayId, "PLAY - CHOOSE YOUR MODE");
            Transform t = panel.transform;
            UIFactory.LabelIn(t, "Intro", "ROYAL BATTLE is the primary mode. This build is a LOCAL PROTOTYPE: there is no online matchmaking yet, and starting a match shows the prototype flow instead of connecting to a server.", 20, RoyalColors.TextMuted, TextAnchor.UpperLeft, false, new Vector2(0f, 1f), Vector2.one, new Vector2(48f, -160f), new Vector2(-48f, -96f));
            for (int i = 0; i < ModeNames.Length; i++)
            {
                int idx = i;
                Button modeBtn = UIFactory.Button(t, "Mode_" + i, ModeNames[i], new Vector2(0.06f, 0f), new Vector2(0.62f, 0f), new Vector2(0f, 330f - i * 84f), new Vector2(0f, 398f - i * 84f), i == 0 ? RoyalColors.RoyalGold : RoyalColors.NavyPanelLight, 24);
                if (i == 0) UIFactory.SetButtonText(modeBtn, ModeNames[i], new Color(0.08f, 0.06f, 0.01f, 1f));
                modeBtn.onClick.AddListener(() => SelectMode(idx));
            }
            Button start = UIFactory.Button(t, "StartPrototype", "START LOCAL PROTOTYPE", new Vector2(0.06f, 0f), new Vector2(0.62f, 0f), new Vector2(0f, 60f), new Vector2(0f, 140f), RoyalColors.RoyalGold, 26);
            UIFactory.SetButtonText(start, "START LOCAL PROTOTYPE", new Color(0.08f, 0.06f, 0.01f, 1f));
            start.onClick.AddListener(RequestPlay);
            UIFactory.LabelIn(t, "ArchitectureNote", "Architecture note: RoyalBattle loads through SceneLoader - pressing PLAY loads the RoyalBattle scene when it is in Build Settings; if it is not, the lobby countdown fallback runs instead.", 18, RoyalColors.TextMuted, TextAnchor.UpperLeft, false, new Vector2(0.66f, 0f), Vector2.one, new Vector2(0f, 60f), new Vector2(-48f, 220f));
        }

        private void BuildLockerPanel(Transform canvasRoot)
        {
            GameObject panel = SectionPanel(canvasRoot, LockerId, "LOCKER - CHARACTER AND COSMETICS (PROTOTYPE)");
            Transform t = panel.transform;
            selectedOutfitText = UIFactory.LabelIn(t, "SelectedOutfit", "Selected outfit: Crown Sovereign (Gold)", 22, RoyalColors.TextWhite, TextAnchor.UpperLeft, true, new Vector2(0f, 1f), Vector2.one, new Vector2(48f, -150f), new Vector2(-48f, -104f));
            RectTransform content = UIFactory.ScrollList(t, "LockerList", new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.80f), Vector2.zero, Vector2.zero, 10f);
            for (int i = 0; i < lockerItems.Count; i++)
            {
                PrototypeCatalog.LockerItem item = lockerItems[i];
                GameObject row = UIFactory.Row(content, "LockerRow_" + i, 72f, RoyalColors.NavyPanel);
                UIFactory.LabelIn(row.transform, "Name", item.Name + "   [" + item.Slot + "]", 22, RoyalColors.TextWhite, TextAnchor.MiddleLeft, false, Vector2.zero, Vector2.one, new Vector2(16f, 0f), new Vector2(-220f, 0f));
                Button equip = UIFactory.Button(row.transform, "Equip", "EQUIP / PREVIEW", new Vector2(1f, 0.18f), new Vector2(1f, 0.82f), new Vector2(-204f, 0f), new Vector2(-12f, 0f), RoyalColors.RoyalGold, 18);
                UIFactory.SetButtonText(equip, "EQUIP / PREVIEW", new Color(0.08f, 0.06f, 0.01f, 1f));
                PrototypeCatalog.LockerItem captured = item;
                equip.onClick.AddListener(() => EquipLockerItem(captured));
            }
        }

        private void BuildStorePanel(Transform canvasRoot)
        {
            GameObject panel = SectionPanel(canvasRoot, StoreId, "STORE - PROTOTYPE ITEMS (NO REAL PURCHASES)");
            Transform t = panel.transform;
            UIFactory.LabelIn(t, "StoreNote", "Prototype store: prices are paid with local prototype Crowns only. Nothing here charges real money.", 20, RoyalColors.TextMuted, TextAnchor.UpperLeft, false, new Vector2(0f, 1f), Vector2.one, new Vector2(48f, -150f), new Vector2(-48f, -104f));
            RectTransform content = UIFactory.ScrollList(t, "StoreList", new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.80f), Vector2.zero, Vector2.zero, 10f);
            for (int i = 0; i < storeItems.Count; i++)
            {
                PrototypeCatalog.StoreItem item = storeItems[i];
                GameObject row = UIFactory.Row(content, "StoreRow_" + i, 76f, RoyalColors.NavyPanel);
                UIFactory.LabelIn(row.transform, "Name", item.Name + "   [" + item.Rarity + "]", 22, RoyalColors.TextWhite, TextAnchor.MiddleLeft, false, Vector2.zero, Vector2.one, new Vector2(16f, 0f), new Vector2(-240f, 0f));
                Button buy = UIFactory.Button(row.transform, "Buy", item.CrownPrice + " CROWNS", new Vector2(1f, 0.16f), new Vector2(1f, 0.84f), new Vector2(-224f, 0f), new Vector2(-12f, 0f), RoyalColors.RoyalGold, 18);
                UIFactory.SetButtonText(buy, item.CrownPrice + " CROWNS", new Color(0.08f, 0.06f, 0.01f, 1f));
                PrototypeCatalog.StoreItem captured = item;
                Button capturedBtn = buy;
                buy.onClick.AddListener(() => BuyStoreItem(captured, capturedBtn));
            }
        }

        private void BuildBattlePassPanel(Transform canvasRoot)
        {
            GameObject panel = SectionPanel(canvasRoot, BattlePassId, "BATTLE PASS - SEASON 1: THE ROYAL ERA (PROTOTYPE)");
            Transform t = panel.transform;
            UIFactory.LabelIn(t, "PassLevel", "Season Level 1 - prototype progression, not yet saved between sessions.", 20, RoyalColors.TextMuted, TextAnchor.UpperLeft, false, new Vector2(0f, 1f), Vector2.one, new Vector2(48f, -150f), new Vector2(-48f, -104f));
            RectTransform content = UIFactory.ScrollList(t, "PassList", new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.80f), Vector2.zero, Vector2.zero, 10f);
            List<PrototypeCatalog.PassTier> tiers = PrototypeCatalog.BattlePassTiers();
            for (int i = 0; i < tiers.Count; i++)
            {
                PrototypeCatalog.PassTier tier = tiers[i];
                bool unlocked = tier.Tier <= profile.Level;
                GameObject row = UIFactory.Row(content, "PassRow_" + i, 64f, unlocked ? new Color(0.10f, 0.16f, 0.34f, 1f) : RoyalColors.NavyPanel);
                UIFactory.LabelIn(row.transform, "Tier", "TIER " + tier.Tier + "  -  " + tier.Reward + (unlocked ? "   [UNLOCKED]" : "   [LOCKED]"), 21, unlocked ? RoyalColors.RoyalGoldBright : RoyalColors.TextMuted, TextAnchor.MiddleLeft, false, Vector2.zero, Vector2.one, new Vector2(16f, 0f), Vector2.zero);
            }
        }

        private void BuildQuestsPanel(Transform canvasRoot)
        {
            GameObject panel = SectionPanel(canvasRoot, QuestsId, "QUESTS - DAILY QUESTS (PROTOTYPE)");
            Transform t = panel.transform;
            RectTransform content = UIFactory.ScrollList(t, "QuestList", new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.86f), Vector2.zero, Vector2.zero, 10f);
            for (int i = 0; i < quests.Count; i++)
            {
                PrototypeCatalog.Quest q = quests[i];
                GameObject row = UIFactory.Row(content, "QuestRow_" + i, 92f, RoyalColors.NavyPanel);
                UIFactory.LabelIn(row.transform, "Name", q.Name, 23, RoyalColors.TextWhite, TextAnchor.UpperLeft, true, Vector2.zero, Vector2.one, new Vector2(16f, -6f), Vector2.zero);
                UIFactory.LabelIn(row.transform, "Meta", "Progress " + q.Progress + " / " + q.Target + "   -   Reward " + q.XpReward + " XP" + (q.Claimed ? "   [CLAIMED]" : string.Empty), 18, RoyalColors.TextMuted, TextAnchor.LowerLeft, false, Vector2.zero, new Vector2(1f, 0.45f), new Vector2(16f, 6f), Vector2.zero);
                Button claim = UIFactory.Button(row.transform, "Claim", q.Claimed ? "CLAIMED" : (q.Progress >= q.Target ? "CLAIM XP" : "IN PROGRESS"), new Vector2(1f, 0.22f), new Vector2(1f, 0.78f), new Vector2(-204f, 0f), new Vector2(-12f, 0f), RoyalColors.RoyalGold, 17);
                UIFactory.SetButtonText(claim, q.Claimed ? "CLAIMED" : (q.Progress >= q.Target ? "CLAIM XP" : "IN PROGRESS"), new Color(0.08f, 0.06f, 0.01f, 1f));
                claim.interactable = !q.Claimed && q.Progress >= q.Target;
                PrototypeCatalog.Quest captured = q;
                Button capturedBtn = claim;
                claim.onClick.AddListener(() => ClaimQuest(captured, capturedBtn));
            }
            UIFactory.LabelIn(t, "QuestNote", "These daily quests complete during real matches once gameplay arrives in Mission 02. Until then their progress stays at 0 - the lobby does not fake completions.", 17, RoyalColors.TextMuted, TextAnchor.UpperLeft, false, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(48f, 8f), new Vector2(-48f, 48f));
        }

        private void BuildCareerPanel(Transform canvasRoot)
        {
            GameObject panel = SectionPanel(canvasRoot, CareerId, "CAREER - PLAYER PROFILE (PROTOTYPE)");
            Transform t = panel.transform;
            UIFactory.LabelIn(t, "CareerName", profile.DisplayName + " - " + profile.Title, 26, RoyalColors.TextWhite, TextAnchor.UpperLeft, true, new Vector2(0f, 1f), Vector2.one, new Vector2(48f, -150f), new Vector2(-48f, -104f));
            RectTransform content = UIFactory.ScrollList(t, "CareerList", new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.78f), Vector2.zero, Vector2.zero, 10f);
            List<PrototypeCatalog.CareerStat> stats = PrototypeCatalog.CareerStats();
            for (int i = 0; i < stats.Count; i++)
            {
                GameObject row = UIFactory.Row(content, "CareerRow_" + i, 60f, RoyalColors.NavyPanel);
                UIFactory.LabelIn(row.transform, "Stat", stats[i].Label + ":   " + stats[i].Value, 21, RoyalColors.TextWhite, TextAnchor.MiddleLeft, false, Vector2.zero, Vector2.one, new Vector2(16f, 0f), Vector2.zero);
            }
            string[] profileStatLabels = { "Total Matches", "Total Wins", "Total Eliminations", "Total Damage" };
            int[] profileStatValues = { profile.TotalMatches, profile.TotalWins, profile.TotalEliminations, profile.TotalDamage };
            for (int i = 0; i < profileStatLabels.Length; i++)
            {
                GameObject row = UIFactory.Row(content, "CareerProfileRow_" + i, 60f, RoyalColors.NavyPanel);
                UIFactory.LabelIn(row.transform, "Stat", profileStatLabels[i] + ":   " + profileStatValues[i], 21, RoyalColors.TextWhite, TextAnchor.MiddleLeft, false, Vector2.zero, Vector2.one, new Vector2(16f, 0f), Vector2.zero);
            }
        }

        private void BuildFriendsPanel(Transform canvasRoot)
        {
            GameObject panel = SectionPanel(canvasRoot, FriendsId, "FRIENDS - PARTY FOUNDATION (PROTOTYPE)");
            Transform t = panel.transform;
            UIFactory.LabelIn(t, "FriendsNote", "No online service is connected yet. This list is the UI foundation for the future party and friends system.", 20, RoyalColors.TextMuted, TextAnchor.UpperLeft, false, new Vector2(0f, 1f), Vector2.one, new Vector2(48f, -150f), new Vector2(-48f, -104f));
            partyRosterText = UIFactory.LabelIn(t, "PartyRoster", string.Empty, 18, RoyalColors.TextWhite, TextAnchor.UpperLeft, false, new Vector2(0f, 1f), Vector2.one, new Vector2(48f, -226f), new Vector2(-48f, -158f));
            RectTransform content = UIFactory.ScrollList(t, "FriendsList", new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.78f), Vector2.zero, Vector2.zero, 10f);
            friendsContent = content;
            List<PrototypeCatalog.Friend> friends = PrototypeCatalog.Friends();
            for (int i = 0; i < friends.Count; i++)
            {
                PrototypeCatalog.Friend f = friends[i];
                GameObject row = UIFactory.Row(content, "FriendRow_" + i, 68f, RoyalColors.NavyPanel);
                UIFactory.LabelIn(row.transform, "Name", f.Name + "   -   " + f.Status, 21, RoyalColors.TextWhite, TextAnchor.MiddleLeft, false, Vector2.zero, Vector2.one, new Vector2(16f, 0f), new Vector2(-200f, 0f));
                Button invite = UIFactory.Button(row.transform, "Invite", "INVITE", new Vector2(1f, 0.18f), new Vector2(1f, 0.82f), new Vector2(-184f, 0f), new Vector2(-12f, 0f), RoyalColors.RoyalGold, 18);
                UIFactory.SetButtonText(invite, "INVITE", new Color(0.08f, 0.06f, 0.01f, 1f));
                PrototypeCatalog.Friend captured = f;
                invite.onClick.AddListener(() => InviteFriend(captured.Name));
            }
            RefreshPartyRoster();
            UIFactory.LabelIn(t, "PartyLocalNote", "Parties are local only - online matchmaking is not connected.", 18, RoyalColors.RoyalGoldBright, TextAnchor.MiddleLeft, false, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(48f, 8f), new Vector2(-48f, 52f));
        }

        private void InviteFriend(string friendName)
        {
            if (RoyalGameState.Party == null)
            {
                SetStatus("Party is not available right now.");
                return;
            }
            bool added = RoyalGameState.Party.Add(friendName);
            if (added) SetStatus("Added " + friendName + " to your party (" + RoyalGameState.Party.Count + "/" + PartyData.MaxSize + ").");
            else SetStatus("Could not add " + friendName + " - the party is full or they are already in it.");
            RefreshPartyRoster();
        }

        private string BuildPartyRosterText()
        {
            PartyData party = RoyalGameState.Party;
            if (party == null) return "Party: unavailable.";
            string text = "Party (" + party.Count + "/" + PartyData.MaxSize + "): ";
            for (int i = 0; i < party.Count; i++)
            {
                PartyMember member = party.Member(i);
                if (member == null) continue;
                if (i > 0) text += "  •  ";
                text += member.Name + " [" + (member.Ready ? "Ready" : "Not Ready") + "]";
            }
            return text;
        }

        private void RefreshPartyRoster()
        {
            if (partyRosterText != null) partyRosterText.text = BuildPartyRosterText();
            if (friendsContent == null) return;
            for (int i = 0; i < partyRosterRows.Count; i++)
            {
                if (partyRosterRows[i] != null) Object.Destroy(partyRosterRows[i]);
            }
            partyRosterRows.Clear();
            GameObject header = UIFactory.Row(friendsContent, "PartyRosterHeader", 40f, new Color(0.10f, 0.16f, 0.34f, 1f));
            UIFactory.LabelIn(header.transform, "Header", "YOUR PARTY (local only)", 19, RoyalColors.RoyalGoldBright, TextAnchor.MiddleLeft, true, Vector2.zero, Vector2.one, new Vector2(16f, 0f), Vector2.zero);
            partyRosterRows.Add(header);
            PartyData party = RoyalGameState.Party;
            if (party == null) return;
            for (int i = 0; i < party.Count; i++)
            {
                PartyMember member = party.Member(i);
                if (member == null) continue;
                GameObject row = UIFactory.Row(friendsContent, "PartyRow_" + i, 60f, RoyalColors.NavyPanel);
                UIFactory.LabelIn(row.transform, "Member", member.Name + "  -  " + (member.Ready ? "READY" : "NOT READY"), 20, RoyalColors.TextWhite, TextAnchor.MiddleLeft, false, Vector2.zero, Vector2.one, new Vector2(16f, 0f), new Vector2(-200f, 0f));
                Button ready = UIFactory.Button(row.transform, "Ready", member.Ready ? "READY" : "SET READY", new Vector2(1f, 0.18f), new Vector2(1f, 0.82f), new Vector2(-184f, 0f), new Vector2(-12f, 0f), RoyalColors.RoyalGold, 17);
                UIFactory.SetButtonText(ready, member.Ready ? "READY" : "SET READY", new Color(0.08f, 0.06f, 0.01f, 1f));
                string capturedName = member.Name;
                ready.onClick.AddListener(() => TogglePartyReady(capturedName));
                partyRosterRows.Add(row);
            }
        }

        private void TogglePartyReady(string memberName)
        {
            if (RoyalGameState.Party == null) return;
            RoyalGameState.Party.ToggleReady(memberName);
            RefreshPartyRoster();
            bool ready = false;
            for (int i = 0; i < RoyalGameState.Party.Count; i++)
            {
                PartyMember member = RoyalGameState.Party.Member(i);
                if (member != null && member.Name == memberName) ready = member.Ready;
            }
            SetStatus(memberName + " is now " + (ready ? "Ready" : "Not Ready") + ". Party all ready: " + (RoyalGameState.Party.AllReady() ? "yes" : "no") + ".");
        }

        private void BuildSettingsPanel(Transform canvasRoot)
        {
            GameObject panel = SectionPanel(canvasRoot, SettingsId, "SETTINGS");
            Transform t = panel.transform;
            SettingsData data = CurrentSettings();

            float[] ys = { -190f, -262f, -334f, -430f, -502f };
            string[] names = { "Master Volume", "Music Volume", "SFX Volume", "Graphics Quality", "Mouse Sensitivity" };
            for (int i = 0; i < names.Length; i++)
            {
                UIFactory.LabelIn(t, "SettingLabel_" + i, names[i], 22, RoyalColors.TextWhite, TextAnchor.MiddleLeft, false, new Vector2(0f, 1f), Vector2.one, new Vector2(64f, ys[i]), new Vector2(360f, ys[i] + 52f));
            }
            masterSlider = UIFactory.Slider(t, "MasterSlider", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, ys[0] + 8f), new Vector2(880f, ys[0] + 44f), data.MasterVolume);
            musicSlider = UIFactory.Slider(t, "MusicSlider", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, ys[1] + 8f), new Vector2(880f, ys[1] + 44f), data.MusicVolume);
            sfxSlider = UIFactory.Slider(t, "SfxSlider", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, ys[2] + 8f), new Vector2(880f, ys[2] + 44f), data.SfxVolume);
            qualityDropdown = UIFactory.Dropdown(t, "QualityDropdown", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, ys[3] + 4f), new Vector2(700f, ys[3] + 48f), SettingsData.QualityNames, data.QualityIndex);
            sensitivitySlider = UIFactory.Slider(t, "SensitivitySlider", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, ys[4] + 8f), new Vector2(880f, ys[4] + 44f), (data.MouseSensitivity - 0.1f) / 2.9f);
            sensitivityValueText = UIFactory.LabelIn(t, "SensitivityValue", data.MouseSensitivity.ToString("0.0"), 22, RoyalColors.RoyalGoldBright, TextAnchor.MiddleLeft, true, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(896f, ys[4]), new Vector2(980f, ys[4] + 52f));
            sensitivitySlider.onValueChanged.AddListener(v => { if (sensitivityValueText != null) sensitivityValueText.text = (0.1f + v * 2.9f).ToString("0.0"); });

            fullscreenToggle = UIFactory.Toggle(t, "FullscreenToggle", "Fullscreen", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(64f, -610f), new Vector2(360f, -562f), data.Fullscreen);
            invertYToggle = UIFactory.Toggle(t, "InvertYToggle", "Invert Y (look)", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(400f, -610f), new Vector2(760f, -562f), data.InvertY);

            Button save = UIFactory.Button(t, "SaveSettings", "APPLY + SAVE", new Vector2(0f, 0f), Vector2.zero, new Vector2(64f, 48f), new Vector2(360f, 116f), RoyalColors.RoyalGold, 22);
            UIFactory.SetButtonText(save, "APPLY + SAVE", new Color(0.08f, 0.06f, 0.01f, 1f));
            save.onClick.AddListener(SaveSettingsFromUi);
            Button reset = UIFactory.Button(t, "ResetSettings", "RESET DEFAULTS", new Vector2(0f, 0f), Vector2.zero, new Vector2(380f, 48f), new Vector2(676f, 116f), RoyalColors.NavyPanelLight, 22);
            reset.onClick.AddListener(ResetSettingsUi);
        }

        private void BuildDialog(Transform canvasRoot)
        {
            dialogRoot = UIFactory.Panel(canvasRoot, "ConfirmationDialog", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.01f, 0.02f, 0.05f, 0.78f), false);
            GameObject box = UIFactory.Panel(dialogRoot.transform, "DialogBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-430f, -190f), new Vector2(430f, 190f), RoyalColors.NavyPanel, true);
            UIFactory.LabelIn(box.transform, "DialogTitle", "CONFIRM", 28, RoyalColors.RoyalGoldBright, TextAnchor.UpperCenter, true, Vector2.zero, Vector2.one, new Vector2(24f, -64f), new Vector2(-24f, 0f));
            dialogRoot.name = "ConfirmationDialog";
            dialogMessageText = UIFactory.LabelIn(box.transform, "DialogMessage", string.Empty, 20, RoyalColors.TextWhite, TextAnchor.UpperCenter, false, Vector2.zero, new Vector2(1f, 0.72f), new Vector2(32f, 70f), new Vector2(-32f, 0f));
            Button confirm = UIFactory.Button(box.transform, "DialogConfirm", "CONFIRM", new Vector2(0.08f, 0.08f), new Vector2(0.47f, 0.26f), Vector2.zero, Vector2.zero, RoyalColors.RoyalGold, 21);
            UIFactory.SetButtonText(confirm, "CONFIRM", new Color(0.08f, 0.06f, 0.01f, 1f));
            confirm.onClick.AddListener(() => CloseDialog(true));
            Button cancel = UIFactory.Button(box.transform, "DialogCancel", "CANCEL", new Vector2(0.53f, 0.08f), new Vector2(0.92f, 0.26f), Vector2.zero, Vector2.zero, RoyalColors.NavyPanelLight, 21);
            cancel.onClick.AddListener(() => CloseDialog(false));
            dialogRoot.SetActive(false);
        }

        // ---------------- behavior ----------------

        private SettingsData CurrentSettings()
        {
            if (SettingsManager.Instance != null && SettingsManager.Instance.Data != null) return SettingsManager.Instance.Data;
            return new SettingsData();
        }

        private void SaveSettingsFromUi()
        {
            SettingsManager mgr = SettingsManager.Instance;
            if (mgr == null)
            {
                SetStatus("Settings manager not found in scene - add GameBootstrap to the lobby root.");
                return;
            }
            SettingsData d = mgr.Data;
            d.MasterVolume = masterSlider.value;
            d.MusicVolume = musicSlider.value;
            d.SfxVolume = sfxSlider.value;
            d.QualityIndex = qualityDropdown.value;
            d.Fullscreen = fullscreenToggle.isOn;
            d.InvertY = invertYToggle.isOn;
            d.MouseSensitivity = 0.1f + sensitivitySlider.value * 2.9f;
            d.Clamp();
            mgr.Apply();
            mgr.Save();
            SetStatus("Settings applied and saved locally.");
            if (AudioManager.Instance != null) AudioManager.Instance.PlayConfirm();
        }

        private void ResetSettingsUi()
        {
            SettingsManager mgr = SettingsManager.Instance;
            if (mgr == null) { SetStatus("Settings manager not found in scene."); return; }
            mgr.ResetToDefaults();
            SettingsData d = mgr.Data;
            masterSlider.value = d.MasterVolume;
            musicSlider.value = d.MusicVolume;
            sfxSlider.value = d.SfxVolume;
            qualityDropdown.value = d.QualityIndex;
            qualityDropdown.RefreshShownValue();
            fullscreenToggle.isOn = d.Fullscreen;
            invertYToggle.isOn = d.InvertY;
            sensitivitySlider.value = (d.MouseSensitivity - 0.1f) / 2.9f;
            SetStatus("Settings reset to defaults.");
        }

        private void OpenSection(string id)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
            if (id == LockerId) MarkQuestProgress("Royal Inspection");
            if (id == StoreId) MarkQuestProgress("Treasury Visit");
            ShowPanel(nav.GoTo(id), false);
            RefreshNavHighlight();
        }

        private void MarkQuestProgress(string questName)
        {
            for (int i = 0; i < quests.Count; i++)
            {
                if (quests[i].Name == questName && quests[i].Progress < quests[i].Target) quests[i].Progress = quests[i].Target;
            }
        }

        private void GoBack()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayBack();
            if (nav.Current == MenuNavigationStack.HomePanelId) { SetStatus("You are already in the Royal Lobby."); return; }
            ShowPanel(nav.GoBack(), false);
            RefreshNavHighlight();
        }

        private void ShowPanel(string id, bool instant)
        {
            if (homeRoot != null) homeRoot.SetActive(id == MenuNavigationStack.HomePanelId);
            foreach (KeyValuePair<string, GameObject> kv in panels)
            {
                bool active = kv.Key == id;
                kv.Value.SetActive(active);
                if (active && !instant)
                {
                    CanvasGroup cg = kv.Value.GetComponent<CanvasGroup>();
                    if (cg == null) cg = kv.Value.AddComponent<CanvasGroup>();
                    StopAllCoroutines();
                    StartCoroutine(FadeIn(cg));
                }
            }
        }

        private IEnumerator FadeIn(CanvasGroup cg)
        {
            cg.alpha = 0f;
            float t = 0f;
            while (t < 0.18f)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Clamp01(t / 0.18f);
                yield return null;
            }
            cg.alpha = 1f;
        }

        private void RefreshNavHighlight()
        {
            foreach (KeyValuePair<string, Button> kv in navButtons)
            {
                Image img = kv.Value.GetComponent<Image>();
                if (img == null) continue;
                bool isCurrent = kv.Key == nav.Current || (kv.Key == PlayId && nav.Current == MenuNavigationStack.HomePanelId);
                img.color = isCurrent ? RoyalColors.RoyalGold : new Color(0.05f, 0.08f, 0.18f, 1f);
                string label;
                if (navLabels.TryGetValue(kv.Key, out label))
                    UIFactory.SetButtonText(kv.Value, label, isCurrent ? new Color(0.08f, 0.06f, 0.01f, 1f) : RoyalColors.TextWhite);
            }
        }

        private void RequestPlay()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
            ShowDialog("START ROYAL BATTLE?",
                "Mode: " + ModeNames[selectedModeIndex] + "\n\nPress CONFIRM to load RoyalBattle through SceneLoader. If the RoyalBattle scene is not in Build Settings yet, the lobby countdown fallback runs instead. No online matchmaking service exists yet, so no server will be contacted.",
                () =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayConfirm();
                    if (SceneLoader.Instance != null && SceneLoader.Instance.LoadScene("RoyalBattle"))
                    {
                        SetStatus("Loading Royal Battle...");
                        return;
                    }
                    StartCoroutine(PrototypeStartSequence());
                });
        }

        private IEnumerator PrototypeStartSequence()
        {
            ShowPanel(nav.GoHome(), true);
            RefreshNavHighlight();
            for (int i = 3; i >= 1; i--)
            {
                if (countdownText != null) countdownText.text = i.ToString();
                yield return new WaitForSeconds(0.7f);
            }
            if (countdownText != null) countdownText.text = string.Empty;
            SetStatus("Prototype start complete. The Royal Battle gameplay scene arrives in Mission 02 - the lobby stayed fully functional, no fake matchmaking was performed.");
        }

        private void SelectMode(int idx)
        {
            selectedModeIndex = Mathf.Clamp(idx, 0, ModeNames.Length - 1);
            SetStatus("Selected mode: " + ModeNames[selectedModeIndex] + ". Press START LOCAL PROTOTYPE when ready.");
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }

        private void EquipLockerItem(PrototypeCatalog.LockerItem item)
        {
            if (selectedOutfitText != null) selectedOutfitText.text = "Selected outfit: " + item.Name + "  [" + item.Slot + "]";
            if (showcase == null) showcase = FindObjectOfType<LobbyCharacterShowcase>();
            if (showcase != null) showcase.SetAccentScheme(item.AccentScheme);
            SetStatus("Equipped / previewing: " + item.Name + " - the hero showcase accent updated live.");
            if (AudioManager.Instance != null) AudioManager.Instance.PlayConfirm();
        }

        private void BuyStoreItem(PrototypeCatalog.StoreItem item, Button buyButton)
        {
            if (item.Owned)
            {
                SetStatus(item.Name + " is already owned (prototype).");
                return;
            }
            ShowDialog("BUY " + item.Name.ToUpperInvariant() + "?",
                "Price: " + item.CrownPrice + " prototype Crowns.\nYour balance: " + profile.Crowns + " Crowns.\n\nPrototype store only - no real money is involved.",
                () =>
                {
                    if (profile.TrySpendCrowns(item.CrownPrice))
                    {
                        item.Owned = true;
                        UIFactory.SetButtonText(buyButton, "OWNED", RoyalColors.TextWhite);
                        buyButton.interactable = false;
                        RefreshProfileUi();
                        SetStatus("Purchased " + item.Name + " with prototype Crowns. New balance: " + profile.Crowns + ".");
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayConfirm();
                    }
                    else
                    {
                        SetStatus("Not enough prototype Crowns for " + item.Name + " (needs " + item.CrownPrice + ", you have " + profile.Crowns + ").");
                    }
                });
        }

        private void ClaimQuest(PrototypeCatalog.Quest quest, Button claimButton)
        {
            if (quest.Claimed || quest.Progress < quest.Target) return;
            quest.Claimed = true;
            int levels = profile.AddXp(quest.XpReward);
            if (RoyalGameState.QuestProgress != null) RoyalGameState.QuestProgress.MarkClaimed(quest.Name);
            RoyalGameState.Save();
            claimButton.interactable = false;
            UIFactory.SetButtonText(claimButton, "CLAIMED", RoyalColors.TextWhite);
            RefreshProfileUi();
            SetStatus("Quest claimed: +" + quest.XpReward + " XP." + (levels > 0 ? " Level up! You are now Level " + profile.Level + "." : string.Empty));
            if (AudioManager.Instance != null) AudioManager.Instance.PlayConfirm();
        }

        private void PlayEmote()
        {
            if (showcase == null) showcase = FindObjectOfType<LobbyCharacterShowcase>();
            if (showcase != null) showcase.PlayEmote();
            SetStatus("Emote: Royal Spin (prototype).");
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }

        private void ShowDialog(string title, string message, Action onConfirm)
        {
            if (dialogRoot == null) { if (onConfirm != null) onConfirm(); return; }
            Transform titleTr = dialogRoot.transform.Find("DialogBox/DialogTitle");
            if (titleTr != null) { Text tt = titleTr.GetComponent<Text>(); if (tt != null) tt.text = title; }
            if (dialogMessageText != null) dialogMessageText.text = message;
            dialogConfirm = onConfirm;
            dialogRoot.SetActive(true);
            dialogRoot.transform.SetAsLastSibling();
        }

        private void CloseDialog(bool confirmed)
        {
            if (dialogRoot != null) dialogRoot.SetActive(false);
            Action cb = dialogConfirm;
            dialogConfirm = null;
            if (confirmed && cb != null) cb();
            else if (AudioManager.Instance != null) AudioManager.Instance.PlayBack();
        }

        private void RefreshProfileUi()
        {
            if (coinsText != null) coinsText.text = "Coins " + profile.Crowns.ToString("N0");
            if (gemsText != null) gemsText.text = "Gems " + profile.RoyalGems.ToString("N0");
            if (profileLevelText != null) profileLevelText.text = "Level " + profile.Level;
            if (profileXpBar != null) profileXpBar.Set(profile.LevelProgress01);
            if (profileXpText != null) profileXpText.text = profile.XpIntoLevel.ToString("N0") + " / " + PlayerProfileData.XpPerLevel.ToString("N0") + " XP";
            if (heroNameText != null) heroNameText.text = profile.DisplayName;
        }

        private void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
            Debug.Log("[KOBNEXA ROYAL] " + message);
        }
    }
}
