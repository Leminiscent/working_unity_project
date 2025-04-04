using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveAnimationController : MonoBehaviour
{
    [SerializeField] private Image _image;

    private List<Sprite> _frames;
    private float _frameRate;

    public void Init(List<Sprite> frames, float frameRate)
    {
        if (frames == null || frames.Count == 0)
        {
            Debug.LogWarning("No frames provided for the move animation. Destroying animation object.");
            Destroy(gameObject);
            return;
        }

        if (_image == null)
        {
            Debug.LogError("Image component is not assigned.");
            Destroy(gameObject);
            return;
        }

        _frames = frames;
        _frameRate = frameRate;

        _image.sprite = _frames[0];
        _ = StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        for (int i = 0; i < _frames.Count; i++)
        {
            _image.sprite = _frames[i];
            yield return new WaitForSeconds(_frameRate);
        }

        Destroy(gameObject);
    }
}