namespace KobnexaRoyal
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Startup flow. Bootstrap scene -> RoyalLobby. When RoyalLobby is opened
    /// directly, bootstrap simply makes sure the core managers exist and settings apply.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        public string LobbySceneName = "RoyalLobby";

        private void Awake()
        {
            Application.targetFrameRate = 120;
            EnsureManager<SettingsManager>();
            EnsureManager<AudioManager>();
            EnsureManager<SceneLoader>();
        }

        private void Start()
        {
            string active = SceneManager.GetActiveScene().name;
            Debug.Log("[KOBNEXA ROYAL] Booted in scene: " + active);
            if (active == "Bootstrap")
            {
                SceneLoader loader = FindObjectOfType<SceneLoader>();
                if (loader != null) loader.LoadScene(LobbySceneName);
            }
        }

        private void EnsureManager<T>() where T : Component
        {
            if (FindObjectOfType<T>() == null)
            {
                gameObject.AddComponent<T>();
            }
        }
    }
}
