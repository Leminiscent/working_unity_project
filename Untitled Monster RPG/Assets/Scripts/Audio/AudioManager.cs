using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] List<AudioData> sfxList;
    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource sfxPlayer;
    [SerializeField] float fadeDuration = 0.75f;
    AudioClip currentMusic;
    float originalMusicVolume;
    Dictionary<AudioID, AudioData> sfxDictionary;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        originalMusicVolume = musicPlayer.volume;
        sfxDictionary = sfxList.ToDictionary(x => x.id, x => x);
    }

    public void PlaySFX(AudioClip clip, bool pauseMusic = false)
    {
        if (clip == null) return;
        if (pauseMusic)
        {
            musicPlayer.Pause();
            StartCoroutine(UnpauseMusic(clip.length));
        }
        sfxPlayer.PlayOneShot(clip);
    }

    public void PlaySFX(AudioID id, bool pauseMusic = false)
    {
        if (!sfxDictionary.ContainsKey(id)) return;

        var audioData = sfxDictionary[id];

        PlaySFX(audioData.clip, pauseMusic);
    }

    public void PlayMusic(AudioClip clip, bool loop = true, bool fade = false)
    {
        if (clip == null || clip == currentMusic) return;
        currentMusic = clip;
        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        if (fade)
        {
            yield return musicPlayer.DOFade(0, fadeDuration).WaitForCompletion();
        }

        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();

        if (fade)
        {
            yield return musicPlayer.DOFade(originalMusicVolume, fadeDuration).WaitForCompletion();
        }
    }

    IEnumerator UnpauseMusic(float delay)
    {
        yield return new WaitForSeconds(delay);
        musicPlayer.volume = 0;
        musicPlayer.UnPause();
        musicPlayer.DOFade(originalMusicVolume, fadeDuration);
    }
}

public enum AudioID { UISelect, Hit, Defeat, ExpGain, ItemObtained, MonsterObtained }

[System.Serializable]
public class AudioData
{
    public AudioID id;
    public AudioClip clip;
}