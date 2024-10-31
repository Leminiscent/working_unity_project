using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    private Image _image;
    
    public static Fader Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _image = GetComponent<Image>();
    }

    public IEnumerator FadeIn(float time)
    {
        yield return _image.DOFade(1f, time).WaitForCompletion();
    }

    public IEnumerator FadeOut(float time)
    {
        yield return _image.DOFade(0f, time).WaitForCompletion();
    }
}
