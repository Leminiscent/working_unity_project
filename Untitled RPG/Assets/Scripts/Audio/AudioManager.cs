using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioData> _sfxList;
    [SerializeField] private float _fadeDuration = 0.75f;
    
    [field: SerializeField] public AudioSource MusicPlayer { get; private set; }
    [field: SerializeField] public AudioSource SfxPlayer { get; private set; }

    private AudioClip _currentMusic;
    private float _originalMusicVolume;
    private Dictionary<AudioID, AudioData> _sfxDictionary;
    private int _pauseCount = 0; // Counter to track overlapping SFX that require pausing the music.
    private Tween _musicTween; // Tracks the active tween for music transitions.

    public static AudioManager Instance { get; private set; }

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

    private void HandleMusicPause(float duration)
    {
        if (MusicPlayer.isPlaying && _pauseCount == 0)
        {
            MusicPlayer.Pause();
        }
        _pauseCount++;
        _ = StartCoroutine(UnpauseMusicAfterDelay(duration));
    }

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
    LevelUp,
    MoveLearned
}

[System.Serializable]
public class AudioData
{
    public AudioID Id;
    public AudioClip Clip;
}