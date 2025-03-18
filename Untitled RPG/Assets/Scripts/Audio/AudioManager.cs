using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages audio playback within the game, including background music and sound effects.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioData> _sfxList;
    [SerializeField] private float _fadeDuration = 0.75f;

    private AudioClip _currentMusic;
    private float _originalMusicVolume;
    private Dictionary<AudioID, AudioData> _sfxDictionary;
    private int _pauseCount = 0; // Counter to track overlapping SFX that require pausing the music.
    private Tween _musicTween; // Tracks the active tween for music transitions.

    /// <summary>
    /// Gets the AudioSource used for playing background music.
    /// </summary>
    [field: SerializeField]
    public AudioSource MusicPlayer { get; private set; }

    /// <summary>
    /// Gets the AudioSource used for playing sound effects.
    /// </summary>
    [field: SerializeField]
    public AudioSource SfxPlayer { get; private set; }

    /// <summary>
    /// Gets the singleton instance of the AudioManager.
    /// </summary>
    public static AudioManager Instance { get; private set; }

    /// <summary>
    /// Initializes the AudioManager singleton instance.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Initializes the AudioManager by verifying assigned AudioSource components and initializing the sound effects dictionary.
    /// </summary>
    private void Start()
    {
        if (MusicPlayer == null)
        {
            Debug.LogError("MusicPlayer is not assigned in the inspector.");
        }

        if (SfxPlayer == null)
        {
            Debug.LogError("SfxPlayer is not assigned in the inspector.");
        }

        _originalMusicVolume = MusicPlayer.volume;
        InitializeSfxDictionary();
    }

    /// <summary>
    /// Initializes the SFX dictionary from the provided list while checking for duplicates and null entries.
    /// </summary>
    private void InitializeSfxDictionary()
    {
        _sfxDictionary = new Dictionary<AudioID, AudioData>();
        if (_sfxList == null)
        {
            Debug.LogWarning("SFX List is null. Please assign AudioData entries.");
            return;
        }

        foreach (AudioData audioData in _sfxList)
        {
            if (audioData == null)
            {
                Debug.LogWarning("Encountered a null AudioData entry in _sfxList.");
                continue;
            }
            if (_sfxDictionary.ContainsKey(audioData.Id))
            {
                Debug.LogWarning($"Duplicate AudioID found: {audioData.Id}. Only the first occurrence will be used.");
                continue;
            }
            _sfxDictionary.Add(audioData.Id, audioData);
        }
    }

    /// <summary>
    /// Plays a sound effect based on its AudioID. Optionally pauses the music during the SFX.
    /// </summary>
    /// <param name="id">The AudioID to play.</param>
    /// <param name="pauseMusic">If true, pauses the music for the duration of the SFX.</param>
    public void PlaySFX(AudioID id, bool pauseMusic = false)
    {
        if (_sfxDictionary.TryGetValue(id, out AudioData audioData))
        {
            PlaySFX(audioData.Clip, pauseMusic);
        }
        else
        {
            Debug.LogWarning($"AudioID {id} not found in the SFX dictionary.");
        }
    }

    /// <summary>
    /// Plays a sound effect using the specified AudioClip. Optionally pauses the music during the SFX.
    /// </summary>
    /// <param name="clip">The AudioClip to play.</param>
    /// <param name="pauseMusic">If true, pauses the music for the duration of the SFX.</param>
    public void PlaySFX(AudioClip clip, bool pauseMusic = false)
    {
        if (clip == null)
        {
            Debug.LogWarning("Attempted to play a null AudioClip.");
            return;
        }

        if (pauseMusic)
        {
            HandleMusicPause(clip.length);
        }

        SfxPlayer.PlayOneShot(clip);
    }

    /// <summary>
    /// Plays music with options for looping and fade transitions.
    /// </summary>
    /// <param name="clip">The AudioClip to play as music.</param>
    /// <param name="loop">Whether the music should loop.</param>
    /// <param name="fade">Whether to fade out the current music and fade in the new clip.</param>
    public void PlayMusic(AudioClip clip, bool loop = true, bool fade = false)
    {
        if (clip == null)
        {
            Debug.LogWarning("Attempted to play a null AudioClip as music.");
            return;
        }

        if (clip == _currentMusic)
        {
            // Already playing this clip, no need to replay.
            return;
        }

        _currentMusic = clip;
        _ = StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    /// <summary>
    /// Coroutine that handles the asynchronous playing of music with optional fade transitions.
    /// </summary>
    /// <param name="clip">The new AudioClip to play.</param>
    /// <param name="loop">Whether the clip should loop.</param>
    /// <param name="fade">Whether to fade between tracks.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        // If a tween is already running, kill it to avoid overlapping tweens.
        if (_musicTween != null && _musicTween.IsActive())
        {
            _musicTween.Kill();
        }

        if (fade)
        {
            // Fade out the current music.
            _musicTween = MusicPlayer.DOFade(0, _fadeDuration);
            yield return _musicTween.WaitForCompletion();
        }

        MusicPlayer.clip = clip;
        MusicPlayer.loop = loop;
        MusicPlayer.Play();

        if (fade)
        {
            // Fade in the new music.
            MusicPlayer.volume = 0;
            _musicTween = MusicPlayer.DOFade(_originalMusicVolume, _fadeDuration);
            yield return _musicTween.WaitForCompletion();
        }
    }

    /// <summary>
    /// Handles pausing the music when a SFX that requires pausing is played.
    /// Uses a counter to manage overlapping pause requests.
    /// </summary>
    /// <param name="duration">The duration of the SFX, used to determine when to unpause the music.</param>
    private void HandleMusicPause(float duration)
    {
        if (MusicPlayer.isPlaying && _pauseCount == 0)
        {
            MusicPlayer.Pause();
        }
        _pauseCount++;
        _ = StartCoroutine(UnpauseMusicAfterDelay(duration));
    }

    /// <summary>
    /// Coroutine that waits for the specified duration before decrementing the pause counter.
    /// When the counter reaches zero, the music is unpaused with a fade-in.
    /// </summary>
    /// <param name="delay">Delay in seconds.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator UnpauseMusicAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _pauseCount = Mathf.Max(0, _pauseCount - 1);
        if (_pauseCount == 0 && !MusicPlayer.isPlaying)
        {
            MusicPlayer.volume = 0;
            MusicPlayer.UnPause();
            _musicTween = MusicPlayer.DOFade(_originalMusicVolume, _fadeDuration);
        }
    }
}

/// <summary>
/// Enumeration for different Audio IDs used for sound effects.
/// </summary>
public enum AudioID
{
    UIShift,
    CureStatus,
    UISelect,
    UIReturn,
    MoveCast,
    Guard,
    Damage,
    Heal,
    StatusUp,
    StatusDown,
    UnitDefeat,
    AffinityGain,
    AffinityLoss,
    ExpGain,
    ItemObtained,
    BattlerObtained,
    SetStatus,
    Spotted,
    LevelUp
}

/// <summary>
/// Serializable class to map AudioIDs to AudioClips.
/// </summary>
[System.Serializable]
public class AudioData
{
    public AudioID Id;
    public AudioClip Clip;
}