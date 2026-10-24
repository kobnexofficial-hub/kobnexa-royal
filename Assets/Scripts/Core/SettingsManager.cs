namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>Loads, applies and persists SettingsData via PlayerPrefs (local only).</summary>
    public sealed class SettingsManager : MonoBehaviour
    {
        private const string PrefsKey = "KOBNEXA_ROYAL_SETTINGS_V1";
        public SettingsData Data { get; private set; }

        public static SettingsManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { return; }
            Instance = this;
            Load();
            Apply();
        }

        public void Load()
        {
            Data = new SettingsData();
            string json = PlayerPrefs.GetString(PrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                try { JsonUtility.FromJsonOverwrite(json, Data); }
                catch (System.Exception e) { Debug.LogWarning("[KOBNEXA ROYAL] Settings load failed, using defaults: " + e.Message); }
            }
            Data.Clamp();
        }

        public void Save()
        {
            if (Data == null) Data = new SettingsData();
            Data.Clamp();
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

        public void ResetToDefaults()
        {
            Data = new SettingsData();
            Apply();
            Save();
        }

        public void Apply()
        {
            if (Data == null) return;
            Data.Clamp();
            AudioListener.volume = Data.MasterVolume;
            int quality = Mathf.Clamp(Data.QualityIndex, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(quality, true);
            Screen.fullScreen = Data.Fullscreen;
            if (AudioManager.Instance != null) AudioManager.Instance.RefreshVolumes();
        }
    }
}
