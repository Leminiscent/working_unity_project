using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Manages all game audio
public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioData> _sfxList;        // List of all sound effects
    [SerializeField] private AudioSource _musicPlayer;        // AudioSource for background music
    [SerializeField] private AudioSource _sfxPlayer;          // AudioSource for sound effects
    [SerializeField] private float _fadeDuration = 0.75f;     // Fade in/out duration

    private AudioClip _currentMusic;                          // Currently playing music track
    private float _originalMusicVolume;                       // Original music volume
    private Dictionary<AudioID, AudioData> _sfxDictionary;    // Map of IDs to AudioData

    public static AudioManager Instance { get; private set; } // Singleton instance

    private void Awake()
    {
        // Enforce singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Save music volume and build SFX dictionary
        _originalMusicVolume = _musicPlayer.volume;
        _sfxDictionary = _sfxList.ToDictionary(static x => x.Id, static x => x);
    }

    // Plays a specified clip as SFX and optionally pauses music
    public void PlaySFX(AudioClip clip, bool pauseMusic = false)
    {
        if (clip == null)
        {
            return;
        }

        if (pauseMusic)
        {
            _musicPlayer.Pause();
            StartCoroutine(UnpauseMusic(clip.length));
        }

        _sfxPlayer.PlayOneShot(clip);
    }

    // Plays SFX by ID
    public void PlaySFX(AudioID id, bool pauseMusic = false)
    {
        if (_sfxDictionary.TryGetValue(id, out AudioData audioData))
        {
            PlaySFX(audioData.Clip, pauseMusic);
        }
    }

    // Plays new music track (with fade option)
    public void PlayMusic(AudioClip clip, bool loop = true, bool fade = false)
    {
        if (clip == null || clip == _currentMusic)
        {
            return;
        }

        _currentMusic = clip;
        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    // Handles music playback coroutine with optional fade in/out
    private IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        // Fade out current music if needed
        if (fade)
        {
            yield return _musicPlayer.DOFade(0, _fadeDuration).WaitForCompletion();
        }

        // Set new clip and play
        _musicPlayer.clip = clip;
        _musicPlayer.loop = loop;
        _musicPlayer.Play();

        // Fade in new music if needed
        if (fade)
        {
            yield return _musicPlayer.DOFade(_originalMusicVolume, _fadeDuration).WaitForCompletion();
        }
    }

    // Unpauses music after SFX completes
    private IEnumerator UnpauseMusic(float delay)
    {
        yield return new WaitForSeconds(delay);
        _musicPlayer.volume = 0;
        _musicPlayer.UnPause();
        _musicPlayer.DOFade(_originalMusicVolume, _fadeDuration);
    }
}

// Identifiers for different audio clips
public enum AudioID
{
    UISelect,
    Hit,
    Defeat,
    ExpGain,
    ItemObtained,
    MonsterObtained
}

// Holds an ID-Clip pair
[System.Serializable]
public class AudioData
{
    public AudioID Id;
    public AudioClip Clip;
}