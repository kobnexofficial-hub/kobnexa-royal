namespace KobnexaRoyal.Tests
{
    using NUnit.Framework;

    /// <summary>
    /// EditMode tests for the gameplay pure logic (combat, inventory, quests,
    /// match results, parties). Same style as LobbyLogicTests: no scene
    /// dependencies, run in Unity via Window > General > Test Runner >
    /// EditMode > Run All.
    /// </summary>
    public sealed class GameplayLogicTests
    {
        // -------- CombatMath.ApplyDamage --------

        [Test]
        public void Damage_ShieldAbsorbsFirst()
        {
            float health;
            float shield;
            CombatMath.ApplyDamage(100f, 50f, 30f, out health, out shield);
            Assert.AreEqual(100f, health, 0.0001f);
            Assert.AreEqual(20f, shield, 0.0001f);
        }

        [Test]
        public void Damage_OverflowHitsHealth()
        {
            float health;
            float shield;
            CombatMath.ApplyDamage(100f, 20f, 50f, out health, out shield);
            Assert.AreEqual(70f, health, 0.0001f);
            Assert.AreEqual(0f, shield, 0.0001f);
        }

        [Test]
        public void Damage_HealthFloorsAtZero()
        {
            float health;
            float shield;
            CombatMath.ApplyDamage(10f, 0f, 50f, out health, out shield);
            Assert.AreEqual(0f, health, 0.0001f);
            Assert.AreEqual(0f, shield, 0.0001f);
        }

        [Test]
        public void Damage_NegativeDamageIsIgnored()
        {
            float health;
            float shield;
            CombatMath.ApplyDamage(80f, 40f, -10f, out health, out shield);
            Assert.AreEqual(80f, health, 0.0001f);
            Assert.AreEqual(40f, shield, 0.0001f);
        }

        // -------- CombatMath.ZoneRadiusAt --------

        [Test]
        public void Zone_BeforeDelayReturnsStartRadius()
        {
            Assert.AreEqual(200f, CombatMath.ZoneRadiusAt(0f, 30f, 180f, 200f, 50f), 0.0001f);
            Assert.AreEqual(200f, CombatMath.ZoneRadiusAt(30f, 30f, 180f, 200f, 50f), 0.0001f);
        }

        [Test]
        public void Zone_AfterDurationReturnsEndRadius()
        {
            Assert.AreEqual(50f, CombatMath.ZoneRadiusAt(210f, 30f, 180f, 200f, 50f), 0.0001f);
            Assert.AreEqual(50f, CombatMath.ZoneRadiusAt(999f, 30f, 180f, 200f, 50f), 0.0001f);
        }

        [Test]
        public void Zone_MidpointInterpolates()
        {
            Assert.AreEqual(125f, CombatMath.ZoneRadiusAt(120f, 30f, 180f, 200f, 50f), 0.0001f);
        }

        // -------- InventoryData --------

        [Test]
        public void Inventory_AddAndSlot()
        {
            InventoryData data = new InventoryData();
            Assert.IsTrue(data.Add(new InventoryItem { Kind = ItemKind.WeaponBlaster, Name = "Royal Blaster", Amount = 1 }));
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual("Royal Blaster", data.Slot(0).Name);
            Assert.IsNull(data.Slot(1));
            Assert.IsNull(data.Slot(-1));
        }

        [Test]
        public void Inventory_CapacityIsEight()
        {
            InventoryData data = new InventoryData();
            for (int i = 0; i < InventoryData.Capacity; i++)
            {
                Assert.IsTrue(data.Add(new InventoryItem { Kind = ItemKind.WeaponBlaster, Name = "Blaster " + i, Amount = 1 }));
            }
            Assert.AreEqual(InventoryData.Capacity, data.Count);
            Assert.IsFalse(data.Add(new InventoryItem { Kind = ItemKind.WeaponBlaster, Name = "One Too Many", Amount = 1 }));
            Assert.AreEqual(InventoryData.Capacity, data.Count);
        }

        [Test]
        public void Inventory_AmmoStacksInsteadOfTakingSlots()
        {
            InventoryData data = new InventoryData();
            Assert.IsTrue(data.Add(new InventoryItem { Kind = ItemKind.Ammo, Name = "Blaster Ammo", Amount = 10 }));
            Assert.IsTrue(data.Add(new InventoryItem { Kind = ItemKind.Ammo, Name = "Blaster Ammo", Amount = 5 }));
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(15, data.AmmoCount());
            Assert.AreEqual(15, data.Slot(0).Amount);
        }

        [Test]
        public void Inventory_HealthPacksStack()
        {
            InventoryData data = new InventoryData();
            data.Add(new InventoryItem { Kind = ItemKind.HealthPack, Name = "Health Pack", Amount = 1 });
            data.Add(new InventoryItem { Kind = ItemKind.HealthPack, Name = "Health Pack", Amount = 2 });
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(3, data.Slot(0).Amount);
        }

        [Test]
        public void Inventory_SpendAmmo()
        {
            InventoryData data = new InventoryData();
            data.Add(new InventoryItem { Kind = ItemKind.Ammo, Name = "Blaster Ammo", Amount = 15 });
            Assert.IsTrue(data.SpendAmmo(5));
            Assert.AreEqual(10, data.AmmoCount());
            Assert.IsFalse(data.SpendAmmo(11));
            Assert.AreEqual(10, data.AmmoCount());
            Assert.IsTrue(data.SpendAmmo(10));
            Assert.AreEqual(0, data.AmmoCount());
            Assert.AreEqual(0, data.Count);
        }

        [Test]
        public void Inventory_RemoveAtAndNullAdd()
        {
            InventoryData data = new InventoryData();
            Assert.IsFalse(data.Add(null));
            data.Add(new InventoryItem { Kind = ItemKind.ShieldCell, Name = "Shield Cell", Amount = 1 });
            Assert.IsFalse(data.RemoveAt(5));
            Assert.IsTrue(data.RemoveAt(0));
            Assert.AreEqual(0, data.Count);
        }

        // -------- QuestProgressData --------

        [Test]
        public void Quests_TargetsMatchCatalog()
        {
            QuestProgressData q = new QuestProgressData();
            Assert.AreEqual(5, q.TargetFor("Eliminate 5 Players"));
            Assert.AreEqual(3, q.TargetFor("Open 3 Chests"));
            Assert.AreEqual(15, q.TargetFor("Survive for 15 Minutes"));
        }

        [Test]
        public void Quests_CompletionAfterSimulatedMatch()
        {
            QuestProgressData q = new QuestProgressData();
            Assert.IsFalse(q.IsComplete("Eliminate 5 Players"));
            Assert.IsFalse(q.IsComplete("Survive for 15 Minutes"));
            q.ApplyMatch(new MatchResultData { Eliminations = 5, SurvivedSeconds = 900f });
            Assert.AreEqual(5, q.ProgressFor("Eliminate 5 Players"));
            Assert.IsTrue(q.IsComplete("Eliminate 5 Players"));
            Assert.AreEqual(15, q.ProgressFor("Survive for 15 Minutes"));
            Assert.IsTrue(q.IsComplete("Survive for 15 Minutes"));
            Assert.IsFalse(q.IsComplete("Open 3 Chests"));
            q.ChestsOpened = 3;
            Assert.IsTrue(q.IsComplete("Open 3 Chests"));
        }

        [Test]
        public void Quests_SurviveUsesBestTime()
        {
            QuestProgressData q = new QuestProgressData();
            q.ApplyMatch(new MatchResultData { Eliminations = 0, SurvivedSeconds = 600f });
            Assert.AreEqual(10, q.ProgressFor("Survive for 15 Minutes"));
            Assert.IsFalse(q.IsComplete("Survive for 15 Minutes"));
            q.ApplyMatch(new MatchResultData { Eliminations = 0, SurvivedSeconds = 300f });
            Assert.AreEqual(10, q.ProgressFor("Survive for 15 Minutes"));
        }

        [Test]
        public void Quests_ClaimedFlags()
        {
            QuestProgressData q = new QuestProgressData();
            Assert.IsFalse(q.IsClaimed("Eliminate 5 Players"));
            Assert.IsFalse(q.IsClaimed("Open 3 Chests"));
            Assert.IsFalse(q.IsClaimed("Survive for 15 Minutes"));
            q.MarkClaimed("Open 3 Chests");
            Assert.IsTrue(q.IsClaimed("Open 3 Chests"));
            Assert.IsFalse(q.IsClaimed("Eliminate 5 Players"));
            q.MarkClaimed("Eliminate 5 Players");
            q.MarkClaimed("Survive for 15 Minutes");
            Assert.IsTrue(q.IsClaimed("Eliminate 5 Players"));
            Assert.IsTrue(q.IsClaimed("Survive for 15 Minutes"));
        }

        // -------- MatchResultData.ComputeXp --------

        [Test]
        public void MatchXp_EliminationsPlusWinBonus()
        {
            MatchResultData result = new MatchResultData { Eliminations = 3, SurvivedSeconds = 600f, Placement = 1, Won = true };
            Assert.AreEqual(900, result.ComputeXp());
        }

        [Test]
        public void MatchXp_TopThreeBonusWithoutWin()
        {
            MatchResultData result = new MatchResultData { Eliminations = 2, SurvivedSeconds = 300f, Placement = 2, Won = false };
            Assert.AreEqual(450, result.ComputeXp());
        }

        [Test]
        public void MatchXp_TopSixBonus()
        {
            MatchResultData result = new MatchResultData { Eliminations = 0, SurvivedSeconds = 60f, Placement = 5, Won = false };
            Assert.AreEqual(110, result.ComputeXp());
        }

        [Test]
        public void MatchXp_NoPlacementBonusOutsideTopSix()
        {
            MatchResultData result = new MatchResultData { Eliminations = 1, SurvivedSeconds = 0f, Placement = 12, Won = false };
            Assert.AreEqual(100, result.ComputeXp());
        }

        // -------- PartyData --------

        [Test]
        public void Party_StartsWithLocalMember()
        {
            PartyData party = new PartyData();
            Assert.AreEqual(1, party.Count);
            Assert.AreEqual("KobnexaEsteril", party.Member(0).Name);
            Assert.IsNull(party.Member(1));
        }

        [Test]
        public void Party_MaxSizeIsFour()
        {
            PartyData party = new PartyData();
            Assert.IsTrue(party.Add("RoyalGuard_01"));
            Assert.IsTrue(party.Add("QueenArc"));
            Assert.IsTrue(party.Add("KnightOf509"));
            Assert.AreEqual(PartyData.MaxSize, party.Count);
            Assert.IsFalse(party.Add("FifthPlayer"));
            Assert.AreEqual(PartyData.MaxSize, party.Count);
        }

        [Test]
        public void Party_DuplicateRejected()
        {
            PartyData party = new PartyData();
            Assert.IsFalse(party.Add("KobnexaEsteril"));
            Assert.IsTrue(party.Add("QueenArc"));
            Assert.IsFalse(party.Add("QueenArc"));
            Assert.AreEqual(2, party.Count);
        }

        [Test]
        public void Party_ReadyToggleAndAllReady()
        {
            PartyData party = new PartyData();
            party.Add("QueenArc");
            Assert.IsFalse(party.AllReady());
            party.ToggleReady("KobnexaEsteril");
            Assert.IsFalse(party.AllReady());
            party.ToggleReady("QueenArc");
            Assert.IsTrue(party.AllReady());
            party.ToggleReady("QueenArc");
            Assert.IsFalse(party.AllReady());
        }

        [Test]
        public void Party_RemoveOnlyNonLocal()
        {
            PartyData party = new PartyData();
            party.Add("QueenArc");
            Assert.IsFalse(party.Remove("KobnexaEsteril"));
            Assert.IsTrue(party.Remove("QueenArc"));
            Assert.IsFalse(party.Remove("QueenArc"));
            Assert.AreEqual(1, party.Count);
        }
    }
}
