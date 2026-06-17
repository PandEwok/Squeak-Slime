using UnityEngine;
using System;

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
        [Range(0f, 1f)] public float volume;
    }
    [Header("Liste des sons")]
    [SerializeField] private SoundEffect[] databaseSfx;
    [SerializeField] private MusicTrack[] databaseMusic;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSfxSource;
    [SerializeField] private AudioSource musicSource;

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

        musicSource.clip = music.Value.clip;
        musicSource.volume = music.Value.volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}