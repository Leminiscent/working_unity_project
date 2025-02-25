using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioData> _sfxList;
    [SerializeField] private AudioSource _musicPlayer;
    [SerializeField] private AudioSource _sfxPlayer;
    [SerializeField] private float _fadeDuration = 0.75f;

    private AudioClip _currentMusic;
    private float _originalMusicVolume;
    private Dictionary<AudioID, AudioData> _sfxDictionary;

    public AudioSource MusicPlayer => _musicPlayer;
    public AudioSource SfxPlayer => _sfxPlayer;
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
        _originalMusicVolume = _musicPlayer.volume;
        _sfxDictionary = _sfxList.ToDictionary(static x => x.Id, static x => x);
    }

    public void PlaySFX(AudioID id, bool pauseMusic = false)
    {
        if (_sfxDictionary.TryGetValue(id, out AudioData audioData))
        {
            PlaySFX(audioData.Clip, pauseMusic);
        }
    }

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

    public void PlayMusic(AudioClip clip, bool loop = true, bool fade = false)
    {
        if (clip == null || clip == _currentMusic)
        {
            return;
        }

        _currentMusic = clip;
        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    private IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        if (fade)
        {
            yield return _musicPlayer.DOFade(0, _fadeDuration).WaitForCompletion();
        }

        _musicPlayer.clip = clip;
        _musicPlayer.loop = loop;
        _musicPlayer.Play();

        if (fade)
        {
            yield return _musicPlayer.DOFade(_originalMusicVolume, _fadeDuration).WaitForCompletion();
        }
    }

    private IEnumerator UnpauseMusic(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!_musicPlayer.isPlaying)
        {
            _musicPlayer.volume = 0;
            _musicPlayer.UnPause();
            _musicPlayer.DOFade(_originalMusicVolume, _fadeDuration);
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
    MonsterDefeat,
    AffinityGain,
    AffinityLoss,
    ExpGain,
    ItemObtained,
    MonsterObtained,
    SetStatus,
    Spotted,
    LevelUp
}

[System.Serializable]
public class AudioData
{
    public AudioID Id;
    public AudioClip Clip;
}