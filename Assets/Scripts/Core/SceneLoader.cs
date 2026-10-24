namespace KobnexaRoyal
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Central scene-loading strategy. RoyalLobby is the hub; future gameplay
    /// scenes (Royal Battle, Open World) plug in here without touching lobby UI code.
    /// </summary>
    public sealed class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { return; }
            Instance = this;
        }

        public bool CanLoad(string sceneName)
        {
            return !string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName);
        }

        /// <returns>true if the load actually started.</returns>
        public bool LoadScene(string sceneName)
        {
            if (!CanLoad(sceneName))
            {
                Debug.LogWarning("[KOBNEXA ROYAL] Scene '" + sceneName + "' is not in Build Settings yet. " +
                    "Run Tools > KOBNEXA ROYAL > Rebuild All Scenes, or add it in File > Build Settings.");
                return false;
            }
            SceneManager.LoadSceneAsync(sceneName);
            return true;
        }
    }
}
