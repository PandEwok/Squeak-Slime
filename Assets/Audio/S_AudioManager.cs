using UnityEngine;
using System;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Serializable]
    public struct SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [Serializable]
    public struct MusicTrack
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume; // The track's default mix volume

        [Header("Timing & Tweaks")]
        public float skipIntroSeconds;
        public float playbackDelaySeconds;
        public float fadeInDuration;
    }

    [Header("Liste des sons")]
    [SerializeField] private SoundEffect[] databaseSfx;
    [SerializeField] private MusicTrack[] databaseMusic;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSfxSource;
    [SerializeField] private AudioSource musicSource;

    // Direct Volume Storage (0.0 to 1.0)
    private float masterMusicVolume = 0.5f;
    private MusicTrack? currentActiveTrack;

    private Coroutine musicPlaybackCoroutine;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private SoundEffect? FindSound(string soundName)
    {
        foreach (var sound in databaseSfx)
        {
            if (sound.name == soundName) return sound;
        }
        Debug.LogWarning("AudioManager: Le son '" + soundName + "' n'existe pas.");
        return null;
    }

    private MusicTrack? FindMusic(string musicName)
    {
        foreach (var music in databaseMusic)
        {
            if (music.name == musicName) return music;
        }
        Debug.LogWarning("AudioManager: La musique '" + musicName + "' n'existe pas.");
        return null;
    }

    public void PlaySFX(string soundName)
    {
        SoundEffect? sound = FindSound(soundName);
        if (sound != null)
        {
            sfxSource.PlayOneShot(sound.Value.clip, sound.Value.volume);
        }
    }

    public void PlayLoopingSFX(string soundName)
    {
        SoundEffect? sound = FindSound(soundName);
        if (sound == null) return;
        if (loopSfxSource.clip == sound.Value.clip && loopSfxSource.isPlaying) return;

        loopSfxSource.clip = sound.Value.clip;
        loopSfxSource.volume = sound.Value.volume;
        loopSfxSource.loop = true;
        loopSfxSource.Play();
    }

    public void StopLoopingSFX()
    {
        loopSfxSource.Stop();
    }

    public void PlayMusic(string musicName)
    {
        MusicTrack? music = FindMusic(musicName);
        if (music == null) return;
        if (musicSource.clip == music.Value.clip && musicSource.isPlaying) return;

        ResetMusicRoutines();
        currentActiveTrack = music;
        musicPlaybackCoroutine = StartCoroutine(MusicPlaybackSequence(music.Value));
    }

    public void StopMusic()
    {
        ResetMusicRoutines();
        currentActiveTrack = null;
        musicSource.Stop();
    }

    // THE MISSING FUNCTION: This links to your MainMenuSettings.cs!
    public void SetMusicVolumeMaster(float volumeNormalized)
    {
        masterMusicVolume = Mathf.Clamp01(volumeNormalized);

        if (musicSource != null && musicSource.isPlaying && currentActiveTrack != null)
        {
            if (fadeCoroutine == null)
            {
                musicSource.volume = currentActiveTrack.Value.volume * masterMusicVolume;
            }
        }
    }

    private void ResetMusicRoutines()
    {
        if (musicPlaybackCoroutine != null) StopCoroutine(musicPlaybackCoroutine);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = null;
    }

    private IEnumerator MusicPlaybackSequence(MusicTrack music)
    {
        if (music.playbackDelaySeconds > 0f)
        {
            musicSource.Stop();
            yield return new WaitForSeconds(music.playbackDelaySeconds);
        }

        musicSource.clip = music.clip;
        musicSource.time = music.skipIntroSeconds;
        musicSource.loop = true;

        float targetVolumeScale = music.volume * masterMusicVolume;

        if (music.fadeInDuration > 0f)
        {
            musicSource.volume = 0f;
            musicSource.Play();
            fadeCoroutine = StartCoroutine(FadeInRoutine(targetVolumeScale, music.fadeInDuration));
        }
        else
        {
            musicSource.volume = targetVolumeScale;
            musicSource.Play();
        }
    }

    private IEnumerator FadeInRoutine(float targetVolume, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float liveTargetVolume = (currentActiveTrack != null ? currentActiveTrack.Value.volume : 1f) * masterMusicVolume;
            musicSource.volume = Mathf.Lerp(0f, liveTargetVolume, timer / duration);
            yield return null;
        }
        musicSource.volume = (currentActiveTrack != null ? currentActiveTrack.Value.volume : 1f) * masterMusicVolume;
        fadeCoroutine = null;
    }
}