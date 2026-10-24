namespace KobnexaRoyal
{
    using System;

    /// <summary>Pure, persistent progress for the 3 reference daily quests.
    /// Quest names must match PrototypeCatalog.Quests() exactly.</summary>
    [Serializable]
    public sealed class QuestProgressData
    {
        public int Eliminations;
        public int ChestsOpened;
        public float SurvivedSecondsBest;
        public bool ClaimedEliminate;
        public bool ClaimedChests;
        public bool ClaimedSurvive;

        public int TargetFor(string questName)
        {
            if (questName == "Eliminate 5 Players") return 5;
            if (questName == "Open 3 Chests") return 3;
            if (questName == "Survive for 15 Minutes") return 15;
            return 1;
        }

        public int ProgressFor(string questName)
        {
            if (questName == "Eliminate 5 Players") return Eliminations;
            if (questName == "Open 3 Chests") return ChestsOpened;
            if (questName == "Survive for 15 Minutes") return (int)(SurvivedSecondsBest / 60f);
            return 0;
        }

        public bool IsComplete(string questName)
        {
            return ProgressFor(questName) >= TargetFor(questName);
        }

        public bool IsClaimed(string questName)
        {
            if (questName == "Eliminate 5 Players") return ClaimedEliminate;
            if (questName == "Open 3 Chests") return ClaimedChests;
            if (questName == "Survive for 15 Minutes") return ClaimedSurvive;
            return false;
        }

        public void MarkClaimed(string questName)
        {
            if (questName == "Eliminate 5 Players") ClaimedEliminate = true;
            else if (questName == "Open 3 Chests") ClaimedChests = true;
            else if (questName == "Survive for 15 Minutes") ClaimedSurvive = true;
        }

        public void ApplyMatch(MatchResultData result)
        {
            if (result == null) return;
            Eliminations += result.Eliminations;
            if (result.SurvivedSeconds > SurvivedSecondsBest) SurvivedSecondsBest = result.SurvivedSeconds;
        }
    }
}
