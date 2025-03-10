using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveAnimationController : MonoBehaviour
{
    [SerializeField] private Image _image;
    private List<Sprite> _frames;
    private float _frameRate;

    public void Initialize(List<Sprite> frames, float frameRate)
    {
        _frames = frames;
        _frameRate = frameRate;

        if (_frames.Count > 0)
        {
            _image.sprite = _frames[0];
            StartCoroutine(PlayAnimation());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator PlayAnimation()
    {
        int currentFrame = 0;

        while (currentFrame < _frames.Count)
        {
            _image.sprite = _frames[currentFrame];
            yield return new WaitForSeconds(_frameRate);
            currentFrame++;
        }

        Destroy(gameObject);
    }
}
