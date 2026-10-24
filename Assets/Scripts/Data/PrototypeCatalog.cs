namespace KobnexaRoyal
{
    using System.Collections.Generic;

    /// <summary>
    /// Clearly-labeled PROTOTYPE content for the lobby. No online store, no real
    /// purchases, no matchmaking: this data exists so every menu has real,
    /// navigable content during Mission 01. Replace with real services later.
    /// </summary>
    public static class PrototypeCatalog
    {
        public sealed class StoreItem { public string Name; public string Rarity; public int CrownPrice; public bool Owned; }
        public sealed class Quest { public string Name; public string Objective; public int Progress; public int Target; public int XpReward; public bool Claimed; }
        public sealed class PassTier { public int Tier; public string Reward; public bool Unlocked; }
        public sealed class LockerItem { public string Name; public string Slot; public int AccentScheme; }
        public sealed class Friend { public string Name; public string Status; }
        public sealed class CareerStat { public string Label; public string Value; }

        public static List<StoreItem> StoreItems()
        {
            return new List<StoreItem>
            {
                new StoreItem { Name = "Crown Sovereign Armor", Rarity = "Legendary", CrownPrice = 2000 },
                new StoreItem { Name = "Royal Navy Cloak", Rarity = "Epic", CrownPrice = 1200 },
                new StoreItem { Name = "Gilded Blade Wrap", Rarity = "Rare", CrownPrice = 600 },
                new StoreItem { Name = "Blue Arc Emote - Royal Salute", Rarity = "Rare", CrownPrice = 500 },
                new StoreItem { Name = "Throne Room Glider", Rarity = "Epic", CrownPrice = 1500 },
                new StoreItem { Name = "Pillar Guard Pickaxe", Rarity = "Uncommon", CrownPrice = 300 },
            };
        }

        public static List<Quest> Quests()
        {
            // Daily Quests exactly as shown in the approved lobby reference image.
            return new List<Quest>
            {
                new Quest { Name = "Eliminate 5 Players", Objective = "Eliminate 5 Players", Progress = 0, Target = 5, XpReward = 1000 },
                new Quest { Name = "Open 3 Chests", Objective = "Open 3 Chests", Progress = 0, Target = 3, XpReward = 1000 },
                new Quest { Name = "Survive for 15 Minutes", Objective = "Survive for 15 Minutes", Progress = 0, Target = 15, XpReward = 1000 },
            };
        }

        public static List<PassTier> BattlePassTiers()
        {
            List<PassTier> tiers = new List<PassTier>();
            string[] rewards = { "100 Crowns", "Royal Banner Icon", "500 XP Boost (prototype)", "Navy Guard Gloves", "Gilded Emote - Point", "Blue Arc Trail", "Royal Helm Variant", "1000 Crowns", "Throne Spray (prototype)", "Crown Sovereign Variant" };
            for (int i = 0; i < rewards.Length; i++)
            {
                tiers.Add(new PassTier { Tier = i + 1, Reward = rewards[i], Unlocked = i < 3 });
            }
            return tiers;
        }

        public static List<LockerItem> LockerItems()
        {
            return new List<LockerItem>
            {
                new LockerItem { Name = "Crown Sovereign (Gold)", Slot = "Outfit", AccentScheme = 0 },
                new LockerItem { Name = "Midnight Royal (Blue)", Slot = "Outfit", AccentScheme = 1 },
                new LockerItem { Name = "Deep Navy Guard", Slot = "Outfit", AccentScheme = 2 },
                new LockerItem { Name = "Royal Cape", Slot = "Back Bling", AccentScheme = 0 },
            };
        }

        public static List<Friend> Friends()
        {
            return new List<Friend>
            {
                new Friend { Name = "RoyalGuard_01", Status = "Online - In Lobby (prototype)" },
                new Friend { Name = "QueenArc", Status = "Online - In Menus (prototype)" },
                new Friend { Name = "KnightOf509", Status = "Offline (prototype)" },
            };
        }

        public static List<CareerStat> CareerStats()
        {
            return new List<CareerStat>
            {
                new CareerStat { Label = "Matches Played", Value = "0 (no gameplay yet)" },
                new CareerStat { Label = "Victory Royales", Value = "0" },
                new CareerStat { Label = "Eliminations", Value = "0" },
                new CareerStat { Label = "Time in Lobby", Value = "Prototype session" },
                new CareerStat { Label = "Account Level", Value = "1" },
            };
        }
    }
}
