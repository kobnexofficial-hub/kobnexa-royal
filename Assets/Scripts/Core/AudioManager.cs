namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// Lobby audio with safe procedural fallbacks: if no audio assets are assigned,
    /// it synthesizes a soft royal pad loop and simple UI tones at runtime so the
    /// lobby is never silent and never crashes on a missing clip.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        public AudioClip MusicClip;
        public AudioClip ClickClip;
        public AudioClip BackClip;
        public AudioClip ConfirmClip;
        public AudioClip ShotClip;
        public AudioClip PickupClip;
        public AudioClip VictoryClip;
        public AudioClip HitClip;
        public AudioClip MeleeClip;

        private AudioSource musicSource;
        private AudioSource sfxSource;

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { return; }
            Instance = this;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;

            if (MusicClip == null) MusicClip = CreatePadLoop();
            if (ClickClip == null) ClickClip = CreateTone(880f, 0.07f, 0.35f);
            if (BackClip == null) BackClip = CreateTone(520f, 0.08f, 0.35f);
            if (ConfirmClip == null) ConfirmClip = CreateTone(990f, 0.12f, 0.4f);
            if (ShotClip == null) ShotClip = CreateTone(180f, 0.15f, 0.5f);
            if (PickupClip == null) PickupClip = CreateTone(1320f, 0.12f, 0.4f);
            if (VictoryClip == null) VictoryClip = CreateTone(784f, 0.5f, 0.4f);
            if (HitClip == null) HitClip = CreateTone(440f, 0.08f, 0.4f);
            if (MeleeClip == null) MeleeClip = CreateTone(300f, 0.12f, 0.4f);

            RefreshVolumes();
            PlayMusic();
        }

        public void RefreshVolumes()
        {
            float music = 0.6f;
            float sfx = 0.8f;
            if (SettingsManager.Instance != null && SettingsManager.Instance.Data != null)
            {
                music = SettingsManager.Instance.Data.MusicVolume;
                sfx = SettingsManager.Instance.Data.SfxVolume;
            }
            if (musicSource != null) musicSource.volume = music;
            if (sfxSource != null) sfxSource.volume = sfx;
        }

        public void PlayMusic()
        {
            if (musicSource == null || MusicClip == null) return;
            if (musicSource.clip != MusicClip) musicSource.clip = MusicClip;
            if (!musicSource.isPlaying) musicSource.Play();
        }

        public void PlayClick() { PlaySfx(ClickClip); }
        public void PlayBack() { PlaySfx(BackClip); }
        public void PlayConfirm() { PlaySfx(ConfirmClip); }
        public void PlayShot() { PlaySfx(ShotClip); }
        public void PlayPickup() { PlaySfx(PickupClip); }
        public void PlayVictory() { PlaySfx(VictoryClip); }
        public void PlayHit() { PlaySfx(HitClip); }
        public void PlayMelee() { PlaySfx(MeleeClip); }

        private void PlaySfx(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip);
        }

        private static AudioClip CreateTone(float frequency, float seconds, float volume)
        {
            int sampleRate = 44100;
            int samples = Mathf.Max(256, (int)(sampleRate * seconds));
            AudioClip clip = AudioClip.Create("RoyalTone_" + (int)frequency, samples, 1, sampleRate, false);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - ((float)i / samples);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreatePadLoop()
        {
            int sampleRate = 44100;
            float seconds = 8f;
            int samples = (int)(sampleRate * seconds);
            AudioClip clip = AudioClip.Create("RoyalPadLoop", samples, 1, sampleRate, false);
            float[] data = new float[samples];
            float[] freqs = new float[] { 130.81f, 196.00f, 261.63f, 329.63f };
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float tremolo = 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * 0.125f * t);
                float sum = 0f;
                for (int f = 0; f < freqs.Length; f++)
                {
                    sum += Mathf.Sin(2f * Mathf.PI * freqs[f] * t);
                }
                data[i] = (sum / freqs.Length) * 0.18f * tremolo;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
