namespace KobnexaRoyal
{
    using System;

    /// <summary>Pure, serializable player settings. Clamp() keeps every value in a safe range.</summary>
    [Serializable]
    public sealed class SettingsData
    {
        public float MasterVolume = 0.8f;
        public float MusicVolume = 0.6f;
        public float SfxVolume = 0.8f;
        public int QualityIndex = 2;
        public bool Fullscreen = true;
        public float MouseSensitivity = 1.0f;
        public bool InvertY = false;

        public static readonly string[] QualityNames = new string[]
        {
            "Low", "Medium", "High", "Ultra"
        };

        public void Clamp()
        {
            MasterVolume = Clamp01(MasterVolume);
            MusicVolume = Clamp01(MusicVolume);
            SfxVolume = Clamp01(SfxVolume);
            if (QualityIndex < 0) QualityIndex = 0;
            if (QualityIndex > QualityNames.Length - 1) QualityIndex = QualityNames.Length - 1;
            if (MouseSensitivity < 0.1f) MouseSensitivity = 0.1f;
            if (MouseSensitivity > 3.0f) MouseSensitivity = 3.0f;
        }

        private static float Clamp01(float v)
        {
            if (v < 0f) return 0f;
            if (v > 1f) return 1f;
            return v;
        }
    }
}
