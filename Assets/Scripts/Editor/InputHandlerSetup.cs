// KOBNEXA ROYAL - Input handler setup (Editor-only).
// KOBNEXA ROYAL uses BOTH input systems: the legacy Input Manager for the
// existing keyboard/mouse code and the Unity Input System package for the
// gamepad layer (ControllerInput). That only works when the project's Active
// Input Handler is "Both". This runs once per Editor session (guarded by a
// SessionState flag so the handler change - which triggers a domain reload -
// does not loop) and sets Both if it is not already set.
namespace KobnexaRoyal.Editor
{
    using UnityEditor;

    [InitializeOnLoad]
    public static class InputHandlerSetup
    {
        private const string SessionKey = "KobnexaRoyal_InputHandlerSet";

        static InputHandlerSetup()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);
            if (PlayerSettings.activeInputHandler != ActiveInputHandler.Both)
            {
                PlayerSettings.activeInputHandler = ActiveInputHandler.Both;
                UnityEngine.Debug.Log("[KOBNEXA ROYAL] Active Input Handler set to Both " +
                    "(legacy Input Manager + Input System package) for keyboard/mouse + gamepad support.");
            }
        }
    }
}
