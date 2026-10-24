namespace KobnexaRoyal.Tests
{
    using NUnit.Framework;
    using UnityEngine;

    /// <summary>
    /// EditMode tests for the controller layer's pure parts. Run in Unity:
    /// Window > General > Test Runner > EditMode > Run All. No gamepad or
    /// scene is needed: these test deadzone math, last-input switching,
    /// weapon-mode toggling, mapping labels, and settings clamping.
    /// </summary>
    public sealed class ControllerInputTests
    {
        // -------- deadzone math --------

        [Test]
        public void Deadzone_VectorInsideDeadzoneReadsZero()
        {
            Assert.AreEqual(Vector2.zero,
                ControllerInputMath.ApplyDeadzone(new Vector2(0.1f, 0f), 0.2f));
        }

        [Test]
        public void Deadzone_VectorAtFullDeflectionStaysFull()
        {
            Vector2 result = ControllerInputMath.ApplyDeadzone(new Vector2(1f, 0f), 0.2f);
            Assert.AreEqual(1f, result.x, 0.001f);
            Assert.AreEqual(0f, result.y, 0.001f);
        }

        [Test]
        public void Deadzone_VectorRescalesPartialDeflection()
        {
            // Magnitude 0.6 with deadzone 0.2 rescales to (0.6-0.2)/(1-0.2) = 0.5.
            Vector2 result = ControllerInputMath.ApplyDeadzone(new Vector2(0.6f, 0f), 0.2f);
            Assert.AreEqual(0.5f, result.x, 0.001f);
        }

        [Test]
        public void Deadzone_FloatNegativeSideIsSymmetric()
        {
            Assert.AreEqual(0f, ControllerInputMath.ApplyDeadzone(-0.1f, 0.2f), 0.001f);
            Assert.AreEqual(-0.5f, ControllerInputMath.ApplyDeadzone(-0.6f, 0.2f), 0.001f);
            Assert.AreEqual(-1f, ControllerInputMath.ApplyDeadzone(-1f, 0.2f), 0.001f);
        }

        [Test]
        public void Deadzone_DefaultIsReasonable()
        {
            Assert.Greater(ControllerInputMath.DefaultDeadzone, 0f);
            Assert.Less(ControllerInputMath.DefaultDeadzone, 0.5f);
        }

        // -------- last-input switching --------

        [Test]
        public void Tracker_StartsOnKeyboardMouse()
        {
            InputDeviceTracker tracker = new InputDeviceTracker();
            Assert.AreEqual(ControllerInputMath.LastInputDevice.KeyboardMouse, tracker.Current);
            Assert.IsFalse(tracker.IsGamepad);
        }

        [Test]
        public void Tracker_SwitchesToGamepadAndBack()
        {
            InputDeviceTracker tracker = new InputDeviceTracker();
            tracker.NoteGamepadActivity();
            Assert.IsTrue(tracker.IsGamepad);
            tracker.NoteKeyboardMouseActivity();
            Assert.AreEqual(ControllerInputMath.LastInputDevice.KeyboardMouse, tracker.Current);
            Assert.IsFalse(tracker.IsGamepad);
        }

        // -------- weapon mode --------

        [Test]
        public void WeaponMode_TogglesBetweenBlasterAndMelee()
        {
            WeaponMode.SetBlaster();
            Assert.IsTrue(WeaponMode.IsBlaster);
            WeaponMode.Toggle();
            Assert.IsTrue(WeaponMode.IsMelee);
            Assert.IsFalse(WeaponMode.IsBlaster);
            WeaponMode.Toggle();
            Assert.IsTrue(WeaponMode.IsBlaster);
        }

        // -------- mapping labels --------

        [Test]
        public void Mapping_ButtonsAreDocumented()
        {
            Assert.IsNotEmpty(ControllerInputMath.Buttons.Jump);
            Assert.IsNotEmpty(ControllerInputMath.Buttons.Melee);
            Assert.IsNotEmpty(ControllerInputMath.Buttons.Reload);
            Assert.IsNotEmpty(ControllerInputMath.Buttons.SwapWeapon);
            Assert.IsNotEmpty(ControllerInputMath.Buttons.Shoot);
            Assert.IsNotEmpty(ControllerInputMath.Buttons.Sprint);
            Assert.IsNotEmpty(ControllerInputMath.Buttons.Pause);
            Assert.IsNotEmpty(ControllerInputMath.Buttons.InventorySlots);
        }

        // -------- settings --------

        [Test]
        public void Settings_ControllerSensitivityIsClamped()
        {
            SettingsData data = new SettingsData();
            data.ControllerSensitivity = 99f;
            data.Clamp();
            Assert.AreEqual(3.0f, data.ControllerSensitivity, 0.001f);
            data.ControllerSensitivity = -5f;
            data.Clamp();
            Assert.AreEqual(0.1f, data.ControllerSensitivity, 0.001f);
        }

        [Test]
        public void Settings_VibrationDefaultsToOn()
        {
            SettingsData data = new SettingsData();
            Assert.IsTrue(data.VibrationEnabled);
            Assert.AreEqual(1.0f, data.ControllerSensitivity, 0.001f);
        }
    }
}
