namespace KobnexaRoyal
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Gamepad input layer for KOBNEXA ROYAL (PC gamepad, Xbox-style layout).
    ///
    /// Route: Unity Input System package (com.unity.inputsystem) when a
    /// Gamepad is present, with a legacy Input Manager joystick fallback
    /// (axes defined in ProjectSettings/InputManager.asset) when it is not.
    /// Keyboard/mouse support is never removed: last-input switching means
    /// the gamepad takes over when a stick or button is used, and keyboard /
    /// mouse take over again as soon as they are used. No gamepad is required
    /// to play; every property simply reads as idle when none is attached.
    ///
    /// Edge detection is frame-based: the first read in a new frame snapshots
    /// the previous frame's state, so Pressed properties are true for exactly
    /// one frame no matter how many systems read them.
    /// </summary>
    public static class ControllerInput
    {
        public const float Deadzone = ControllerInputMath.DefaultDeadzone;

        private struct Snapshot
        {
            public Vector2 Move;
            public Vector2 Camera;
            public Vector2 DPad;
            public bool Jump;
            public bool Melee;
            public bool Reload;
            public bool Swap;
            public bool ShootHeld;
            public bool SprintHeld;
            public bool CrouchHeld;
            public bool Start;
            public bool Back;
            public bool AnyActivity;
        }

        private static Snapshot prev;
        private static Snapshot curr;
        private static int lastFrame = -1;
        private static readonly InputDeviceTracker tracker = new InputDeviceTracker();
        private static bool motorsOn;
        private static float rumbleUntil;

        public static ControllerInputMath.LastInputDevice CurrentDevice
        {
            get { EnsureFrame(); return tracker.Current; }
        }

        public static bool IsGamepadActive
        {
            get { EnsureFrame(); return tracker.IsGamepad; }
        }

        public static float ControllerSensitivity
        {
            get
            {
                SettingsManager mgr = SettingsManager.Instance;
                if (mgr != null && mgr.Data != null) return mgr.Data.ControllerSensitivity;
                return 1f;
            }
        }

        public static bool VibrationEnabled
        {
            get
            {
                SettingsManager mgr = SettingsManager.Instance;
                if (mgr != null && mgr.Data != null) return mgr.Data.VibrationEnabled;
                return true;
            }
        }

        // ---- continuous input (already deadzoned) ----
        public static Vector2 Move { get { EnsureFrame(); return curr.Move; } }
        public static Vector2 Camera { get { EnsureFrame(); return curr.Camera; } }
        public static Vector2 DPad { get { EnsureFrame(); return curr.DPad; } }
        public static bool ShootHeld { get { EnsureFrame(); return curr.ShootHeld; } }
        public static bool SprintHeld { get { EnsureFrame(); return curr.SprintHeld; } }
        public static bool CrouchHeld { get { EnsureFrame(); return curr.CrouchHeld; } }

        // ---- one-frame edges ----
        public static bool JumpPressed { get { EnsureFrame(); return curr.Jump && !prev.Jump; } }
        public static bool MeleePressed { get { EnsureFrame(); return curr.Melee && !prev.Melee; } }
        public static bool ReloadPressed { get { EnsureFrame(); return curr.Reload && !prev.Reload; } }
        public static bool SwapPressed { get { EnsureFrame(); return curr.Swap && !prev.Swap; } }
        public static bool StartPressed { get { EnsureFrame(); return curr.Start && !prev.Start; } }
        public static bool BackPressed { get { EnsureFrame(); return curr.Back && !prev.Back; } }
        public static bool DPadUpPressed { get { EnsureFrame(); return curr.DPad.y > 0.5f && !(prev.DPad.y > 0.5f); } }
        public static bool DPadDownPressed { get { EnsureFrame(); return curr.DPad.y < -0.5f && !(prev.DPad.y < -0.5f); } }
        public static bool DPadLeftPressed { get { EnsureFrame(); return curr.DPad.x < -0.5f && !(prev.DPad.x < -0.5f); } }
        public static bool DPadRightPressed { get { EnsureFrame(); return curr.DPad.x > 0.5f && !(prev.DPad.x > 0.5f); } }

        /// <summary>Short rumble pulse. No-op without a gamepad or when the
        /// vibration setting is off. Safe to call every shot/hit.</summary>
        public static void Rumble(float lowFrequency, float highFrequency, float seconds)
        {
            if (!VibrationEnabled) return;
            Gamepad pad = Gamepad.current;
            if (pad == null) return;
            try
            {
                pad.SetMotorSpeeds(lowFrequency, highFrequency);
                motorsOn = true;
                rumbleUntil = Time.realtimeSinceStartup + Mathf.Max(0.05f, seconds);
            }
            catch (System.Exception)
            {
                motorsOn = false;
            }
        }

        public static void StopRumble()
        {
            Gamepad pad = Gamepad.current;
            if (pad != null && motorsOn)
            {
                try { pad.SetMotorSpeeds(0f, 0f); } catch (System.Exception) { }
            }
            motorsOn = false;
        }

        private static void EnsureFrame()
        {
            int frame = Time.frameCount;
            if (frame == lastFrame) return;
            lastFrame = frame;
            prev = curr;
            curr = ReadSnapshot();
            if (motorsOn && Time.realtimeSinceStartup >= rumbleUntil) StopRumble();

            if (curr.AnyActivity)
            {
                tracker.NoteGamepadActivity();
            }
            else if (Input.anyKeyDown || Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f
                || Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                tracker.NoteKeyboardMouseActivity();
            }
        }

        private static Snapshot ReadSnapshot()
        {
            Snapshot s = new Snapshot();
            Gamepad pad = Gamepad.current;
            if (pad != null)
            {
                s.Move = ControllerInputMath.ApplyDeadzone(pad.leftStick.ReadValue(), Deadzone);
                s.Camera = ControllerInputMath.ApplyDeadzone(pad.rightStick.ReadValue(), Deadzone);
                s.DPad = pad.dpad.ReadValue();
                s.Jump = pad.buttonSouth.isPressed;
                s.Melee = pad.buttonEast.isPressed;
                s.Reload = pad.buttonWest.isPressed;
                s.Swap = pad.buttonNorth.isPressed;
                s.ShootHeld = pad.rightTrigger.ReadValue() > 0.5f;
                s.SprintHeld = pad.leftShoulder.isPressed || pad.leftTrigger.ReadValue() > 0.5f;
                s.CrouchHeld = pad.leftStickButton.isPressed;
                s.Start = pad.startButton.isPressed;
                s.Back = pad.selectButton.isPressed;
            }
            else
            {
                // Legacy Input Manager fallback (axes in InputManager.asset).
                float lx = SafeAxis("JoyLeftX");
                float ly = SafeAxis("JoyLeftY");
                float rx = SafeAxis("JoyRightX");
                float ry = SafeAxis("JoyRightY");
                float trigger = SafeAxis("JoyTrigger");
                s.Move = ControllerInputMath.ApplyDeadzone(new Vector2(lx, -ly), Deadzone);
                s.Camera = ControllerInputMath.ApplyDeadzone(new Vector2(rx, -ry), Deadzone);
                s.DPad = new Vector2(SafeAxis("JoyDPadX"), SafeAxis("JoyDPadY"));
                s.Jump = Input.GetKey(KeyCode.JoystickButton0);
                s.Melee = Input.GetKey(KeyCode.JoystickButton1);
                s.Reload = Input.GetKey(KeyCode.JoystickButton2);
                s.Swap = Input.GetKey(KeyCode.JoystickButton3);
                s.ShootHeld = trigger > 0.5f;
                s.SprintHeld = Input.GetKey(KeyCode.JoystickButton4) || trigger < -0.5f;
                s.CrouchHeld = Input.GetKey(KeyCode.JoystickButton8);
                s.Start = Input.GetKey(KeyCode.JoystickButton7);
                s.Back = Input.GetKey(KeyCode.JoystickButton6);
            }

            s.AnyActivity = s.Move != Vector2.zero || s.Camera != Vector2.zero || s.DPad != Vector2.zero
                || s.Jump || s.Melee || s.Reload || s.Swap || s.ShootHeld || s.SprintHeld
                || s.CrouchHeld || s.Start || s.Back;
            return s;
        }

        private static float SafeAxis(string name)
        {
            try { return Input.GetAxis(name); }
            catch (System.Exception) { return 0f; }
        }
    }
}
