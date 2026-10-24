namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// Cross-scene local state (static, survives scene loads). The lobby reads
    /// CurrentProfile/QuestProgress/Party; a match writes LastMatchResult and the
    /// lobby applies it on return. Persisted to PlayerPrefs as JSON (local only).
    /// </summary>
    public static class RoyalGameState
    {
        private const string ProfileKey = "KOBNEXA_ROYAL_PROFILE_V1";
        private const string QuestKey = "KOBNEXA_ROYAL_QUESTS_V1";

        public static PlayerProfileData CurrentProfile { get; private set; }
        public static QuestProgressData QuestProgress { get; private set; }
        public static PartyData Party { get; private set; }
        public static MatchResultData LastMatchResult { get; set; }
        public static bool LastMatchApplied { get; set; }

        static RoyalGameState()
        {
            Load();
        }

        public static void Load()
        {
            CurrentProfile = new PlayerProfileData();
            string pj = PlayerPrefs.GetString(ProfileKey, string.Empty);
            if (!string.IsNullOrEmpty(pj))
            {
                try { JsonUtility.FromJsonOverwrite(pj, CurrentProfile); } catch (System.Exception) { }
            }
            CurrentProfile.ClampProfile();
            QuestProgress = new QuestProgressData();
            string qj = PlayerPrefs.GetString(QuestKey, string.Empty);
            if (!string.IsNullOrEmpty(qj))
            {
                try { JsonUtility.FromJsonOverwrite(qj, QuestProgress); } catch (System.Exception) { }
            }
            if (Party == null) Party = new PartyData();
        }

        public static void Save()
        {
            PlayerPrefs.SetString(ProfileKey, JsonUtility.ToJson(CurrentProfile));
            PlayerPrefs.SetString(QuestKey, JsonUtility.ToJson(QuestProgress));
            PlayerPrefs.Save();
        }

        /// <summary>Applies a finished match to profile + quests exactly once per result.</summary>
        public static int ApplyMatchResult(MatchResultData result)
        {
            if (result == null) return 0;
            QuestProgress.ApplyMatch(result);
            CurrentProfile.TotalMatches++;
            CurrentProfile.TotalEliminations += result.Eliminations;
            CurrentProfile.TotalDamage += result.DamageDealt;
            if (result.Won) CurrentProfile.TotalWins++;
            int levels = CurrentProfile.AddXp(result.ComputeXp());
            Save();
            return levels;
        }
    }
}
