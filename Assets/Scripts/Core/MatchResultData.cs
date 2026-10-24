namespace KobnexaRoyal
{
    using System;

    /// <summary>Pure result of one Royal Battle match. XP rule (documented, local only):
    /// 100 XP per elimination + 10 XP per survived minute + 500 XP for a Victory Royale
    /// + 100 XP per placement tier survived (top 6 = +100, top 3 = +200).</summary>
    [Serializable]
    public sealed class MatchResultData
    {
        public int Eliminations;
        public int DamageDealt;
        public int Placement;
        public float SurvivedSeconds;
        public bool Won;

        public int ComputeXp()
        {
            int xp = Eliminations * 100 + (int)(SurvivedSeconds / 60f) * 10;
            if (Won) xp += 500;
            else if (Placement > 0 && Placement <= 3) xp += 200;
            else if (Placement > 0 && Placement <= 6) xp += 100;
            return xp;
        }
    }
}
