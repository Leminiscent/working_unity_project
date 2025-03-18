using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the playback of battle animations.
/// </summary>
public class MoveAnimationController : MonoBehaviour
{
    [SerializeField] private Image _image;

    private List<Sprite> _frames;
    private float _frameRate;

    /// <summary>
    /// Initializes the animation controller with the provided frames and frame rate.
    /// Begins playing the animation immediately if frames are available.
    /// </summary>
    /// <param name="frames">The list of sprites to animate.</param>
    /// <param name="frameRate">The duration (in seconds) each frame should be displayed.</param>
    public void Initialize(List<Sprite> frames, float frameRate)
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

    /// <summary>
    /// Plays the animation by cycling through the provided frames.
    /// Once complete, the GameObject is destroyed.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
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