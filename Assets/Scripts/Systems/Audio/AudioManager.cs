using System;
using Systems.Settings;
using UnityEngine;
using UnityEngine.Audio;

namespace Systems.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mixer")]
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMixerGroup _musicGroup;
        [SerializeField] private AudioMixerGroup _sfxGroup;
        [SerializeField] private string _musicVolumeParameter = "MusicVolume";
        [SerializeField] private string _sfxVolumeParameter = "SfxVolume";

        [Header("Source Trim")]
        [SerializeField, Range(0f, 1f)] private float _musicTrim = 1f;
        [SerializeField, Range(0f, 1f)] private float _sfxTrim = 1f;

        private const float MinDecibels = -80f;
        private const float SilenceThreshold = 0.0001f;

        private AudioSource _musicSource;
        private AudioSource _sfxSource;
        private bool _missingParameterWarningLogged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                throw new InvalidOperationException($"{name}: only one AudioManager is allowed.");

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_mixer == null || _musicGroup == null || _sfxGroup == null)
                throw new MissingReferenceException(
                    $"{name}: mixer, music group and sfx group must all be assigned.");

            _musicSource = CreateSource("Music", _musicGroup, loop: true);
            _sfxSource = CreateSource("SFX", _sfxGroup, loop: false);

            ApplyVolumes();
        }

        private void OnEnable()
        {
            GameSettings.MusicVolumeChanged += ApplyMusicVolume;
            GameSettings.SfxVolumeChanged += ApplySfxVolume;
        }

        private void OnDisable()
        {
            GameSettings.MusicVolumeChanged -= ApplyMusicVolume;
            GameSettings.SfxVolumeChanged -= ApplySfxVolume;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null || _musicSource.clip == clip)
                return;

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.Stop();
            _musicSource.clip = null;
        }

        /// Fires a one-shot on the SFX layer. Several can overlap on the shared source.
        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null)
                return;

            _sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }
        
        public void ApplyVolumes()
        {
            ApplyMusicVolume(GameSettings.MusicVolume);
            ApplySfxVolume(GameSettings.SfxVolume);
        }

        private AudioSource CreateSource(string sourceName, AudioMixerGroup group, bool loop)
        {
            GameObject sourceObject = new(sourceName);
            sourceObject.transform.SetParent(transform);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = group;

            return source;
        }

        private void ApplyMusicVolume(float value)
        {
            SetMixerVolume(_musicVolumeParameter, value * _musicTrim);
        }

        private void ApplySfxVolume(float value)
        {
            SetMixerVolume(_sfxVolumeParameter, value * _sfxTrim);
        }

        private void SetMixerVolume(string parameter, float value)
        {
            if (_mixer.SetFloat(parameter, ToDecibels(value)))
                return;

            if (_missingParameterWarningLogged)
                return;

            _missingParameterWarningLogged = true;
            UnityEngine.Debug.LogWarning(
                $"{name}: mixer parameter '{parameter}' is not exposed, so this volume cannot be controlled. " +
                "Expose it on the matching group and check the parameter name.");
        }

        private static float ToDecibels(float value)
        {
            return value <= SilenceThreshold ? MinDecibels : Mathf.Log10(value) * 20f;
        }
    }
}
