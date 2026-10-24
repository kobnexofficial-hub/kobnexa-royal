namespace KobnexaRoyal.Tests
{
    using NUnit.Framework;

    /// <summary>
    /// EditMode tests for the lobby's pure logic. Run in Unity: Window > General >
    /// Test Runner > EditMode > Run All. These classes have no scene dependencies.
    /// </summary>
    public sealed class LobbyLogicTests
    {
        // -------- MenuNavigationStack --------

        [Test]
        public void Navigation_StartsAtHome()
        {
            MenuNavigationStack nav = new MenuNavigationStack();
            Assert.AreEqual(MenuNavigationStack.HomePanelId, nav.Current);
            Assert.IsFalse(nav.CanGoBack);
        }

        [Test]
        public void Navigation_GoToAndBack()
        {
            MenuNavigationStack nav = new MenuNavigationStack();
            nav.GoTo("Store");
            Assert.AreEqual("Store", nav.Current);
            Assert.IsTrue(nav.CanGoBack);
            nav.GoTo("Quests");
            Assert.AreEqual("Quests", nav.GoBack());
            Assert.AreEqual("Store", nav.Current);
            Assert.AreEqual(MenuNavigationStack.HomePanelId, nav.GoBack());
        }

        [Test]
        public void Navigation_BackAtHomeStaysHome()
        {
            MenuNavigationStack nav = new MenuNavigationStack();
            Assert.AreEqual(MenuNavigationStack.HomePanelId, nav.GoBack());
        }

        [Test]
        public void Navigation_GoHomeClearsHistory()
        {
            MenuNavigationStack nav = new MenuNavigationStack();
            nav.GoTo("Locker");
            nav.GoTo("Settings");
            nav.GoHome();
            Assert.AreEqual(MenuNavigationStack.HomePanelId, nav.Current);
            Assert.IsFalse(nav.CanGoBack);
        }

        [Test]
        public void Navigation_GoToSamePanelDoesNotPush()
        {
            MenuNavigationStack nav = new MenuNavigationStack();
            nav.GoTo("Career");
            nav.GoTo("Career");
            Assert.AreEqual(1, nav.Depth);
        }

        // -------- SettingsData.Clamp --------

        [Test]
        public void Settings_ClampKeepsVolumesInRange()
        {
            SettingsData d = new SettingsData { MasterVolume = 1.7f, MusicVolume = -0.4f, SfxVolume = 0.5f };
            d.Clamp();
            Assert.AreEqual(1f, d.MasterVolume, 0.0001f);
            Assert.AreEqual(0f, d.MusicVolume, 0.0001f);
            Assert.AreEqual(0.5f, d.SfxVolume, 0.0001f);
        }

        [Test]
        public void Settings_ClampQualityAndSensitivity()
        {
            SettingsData d = new SettingsData { QualityIndex = 99, MouseSensitivity = 42f };
            d.Clamp();
            Assert.AreEqual(SettingsData.QualityNames.Length - 1, d.QualityIndex);
            Assert.AreEqual(3.0f, d.MouseSensitivity, 0.0001f);

            SettingsData low = new SettingsData { QualityIndex = -3, MouseSensitivity = 0.01f };
            low.Clamp();
            Assert.AreEqual(0, low.QualityIndex);
            Assert.AreEqual(0.1f, low.MouseSensitivity, 0.0001f);
        }

        // -------- PlayerProfileData --------

        [Test]
        public void Profile_DefaultsMatchReference()
        {
            PlayerProfileData p = new PlayerProfileData();
            Assert.AreEqual("KobnexaEsteril", p.DisplayName);
            Assert.AreEqual(1, p.Level);
            Assert.AreEqual(1000, p.Crowns);
            Assert.AreEqual(0, p.RoyalGems);
            Assert.AreEqual(0, p.XpIntoLevel);
        }

        [Test]
        public void Profile_TrySpendCrowns()
        {
            PlayerProfileData p = new PlayerProfileData();
            Assert.IsTrue(p.TrySpendCrowns(600));
            Assert.AreEqual(400, p.Crowns);
            Assert.IsFalse(p.TrySpendCrowns(401));
            Assert.AreEqual(400, p.Crowns);
            Assert.IsFalse(p.TrySpendCrowns(-5));
        }

        [Test]
        public void Profile_AddXpLevelsUp()
        {
            PlayerProfileData p = new PlayerProfileData();
            int gained = p.AddXp(1000);
            Assert.AreEqual(1, gained);
            Assert.AreEqual(2, p.Level);
            Assert.AreEqual(0, p.XpIntoLevel);
        }

        [Test]
        public void Profile_AddXpMultipleLevelsAndProgress()
        {
            PlayerProfileData p = new PlayerProfileData();
            int gained = p.AddXp(2350);
            Assert.AreEqual(2, gained);
            Assert.AreEqual(3, p.Level);
            Assert.AreEqual(350, p.XpIntoLevel);
            Assert.AreEqual(0.35f, p.LevelProgress01, 0.0001f);
        }

        [Test]
        public void Profile_AddXpIgnoresNonPositive()
        {
            PlayerProfileData p = new PlayerProfileData();
            Assert.AreEqual(0, p.AddXp(0));
            Assert.AreEqual(0, p.AddXp(-100));
            Assert.AreEqual(1, p.Level);
        }
    }
}
