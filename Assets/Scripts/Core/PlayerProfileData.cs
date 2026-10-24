namespace KobnexaRoyal
{
    using System;

    /// <summary>
    /// Pure local prototype profile. This is NOT an online account:
    /// it lives on the device only and exists so the lobby has real state to display.
    /// </summary>
    [Serializable]
    public sealed class PlayerProfileData
    {
        public string DisplayName = "KobnexaEsteril";
        public string Title = "Crown Sovereign";
        public int Level = 1;
        public int XpIntoLevel = 0;
        public const int XpPerLevel = 1000;
        public int Crowns = 1000;
        public int RoyalGems = 0;
        public int TotalEliminations = 0;
        public int TotalMatches = 0;
        public int TotalWins = 0;
        public int TotalDamage = 0;

        public int AddXp(int amount)
        {
            if (amount <= 0) return 0;
            XpIntoLevel += amount;
            int levelsGained = 0;
            while (XpIntoLevel >= XpPerLevel)
            {
                XpIntoLevel -= XpPerLevel;
                Level++;
                levelsGained++;
            }
            return levelsGained;
        }

        public float LevelProgress01
        {
            get { return (float)XpIntoLevel / (float)XpPerLevel; }
        }

        public bool TrySpendCrowns(int amount)
        {
            if (amount < 0) return false;
            if (Crowns < amount) return false;
            Crowns -= amount;
            return true;
        }

        public void ClampProfile()
        {
            if (Level < 1) Level = 1;
            if (XpIntoLevel < 0) XpIntoLevel = 0;
            if (Crowns < 0) Crowns = 0;
            if (RoyalGems < 0) RoyalGems = 0;
            if (TotalEliminations < 0) TotalEliminations = 0;
            if (TotalMatches < 0) TotalMatches = 0;
            if (TotalWins < 0) TotalWins = 0;
            if (TotalDamage < 0) TotalDamage = 0;
        }

        public void AddCrowns(int amount)
        {
            if (amount > 0) Crowns += amount;
        }
    }
}
