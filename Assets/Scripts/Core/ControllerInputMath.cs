namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// Pure controller math + last-input tracking. No Unity input APIs here,
    /// so every function is covered by EditMode tests.
    /// </summary>
    public static class ControllerInputMath
    {
        public const float DefaultDeadzone = 0.2f;

        public enum LastInputDevice
        {
            KeyboardMouse,
            Gamepad
        }

        /// <summary>Radial deadzone with rescaling: values inside the deadzone
        /// read as 0, values at full deflection still read as 1.</summary>
        public static Vector2 ApplyDeadzone(Vector2 value, float deadzone)
        {
            float mag = value.magnitude;
            if (mag <= deadzone) return Vector2.zero;
            if (mag >= 1f) return value.normalized;
            float scaled = (mag - deadzone) / (1f - deadzone);
            return value.normalized * scaled;
        }

        public static float ApplyDeadzone(float value, float deadzone)
        {
            if (Mathf.Abs(value) <= deadzone) return 0f;
            float sign = value >= 0f ? 1f : -1f;
            float mag = Mathf.Abs(value);
            if (mag >= 1f) return sign;
            return sign * (mag - deadzone) / (1f - deadzone);
        }

        /// <summary>Button labels for documentation and UI hints. These are
        /// the Xbox-style names Unity's Input System uses (South = A/Cross,
        /// East = B/Circle, West = X/Square, North = Y/Triangle).</summary>
        public static class Buttons
        {
            public const string Jump = "South (A / Cross)";
            public const string Melee = "East (B / Circle)";
            public const string Reload = "West (X / Square)";
            public const string SwapWeapon = "North (Y / Triangle)";
            public const string Shoot = "Right Trigger (RT / R2)";
            public const string Sprint = "Left Bumper (LB / L1) or Left Trigger (LT / L2)";
            public const string Crouch = "Left Stick Click (L3)";
            public const string Pause = "Start (Menu)";
            public const string Back = "Back / View";
            public const string InventorySlots = "D-pad (slots 1-4)";
        }
    }

    /// <summary>
    /// Last-input switching: gamepad activity wins while a stick or button is
    /// used; any keyboard or mouse activity switches back. Pure state machine.
    /// </summary>
    public sealed class InputDeviceTracker
    {
        public ControllerInputMath.LastInputDevice Current { get; private set; }

        public InputDeviceTracker()
        {
            Current = ControllerInputMath.LastInputDevice.KeyboardMouse;
        }

        public void NoteGamepadActivity()
        {
            Current = ControllerInputMath.LastInputDevice.Gamepad;
        }

        public void NoteKeyboardMouseActivity()
        {
            Current = ControllerInputMath.LastInputDevice.KeyboardMouse;
        }

        public bool IsGamepad
        {
            get { return Current == ControllerInputMath.LastInputDevice.Gamepad; }
        }
    }
}
