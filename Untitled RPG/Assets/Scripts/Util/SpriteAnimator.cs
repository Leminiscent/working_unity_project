using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator
{
    public List<Sprite> Frames { get; private set; }

    private SpriteRenderer _renderer;
    private float _frameRate;
    private int _currentFrame;
    private float _timer;

    public SpriteAnimator(List<Sprite> frames, SpriteRenderer renderer, float frameRate = 0.16f)
    {
        if (frames == null || frames.Count == 0)
        {
            Debug.LogError("SpriteAnimator requires a non-null, non-empty frames list.");
            return;
        }
        if (renderer == null)
        {
            Debug.LogError("SpriteAnimator requires a valid SpriteRenderer.");
            return;
        }

        Frames = frames;
        _renderer = renderer;
        _frameRate = frameRate;
    }

    public void Start()
    {
        _currentFrame = 1;
        _timer = 0f;
        _renderer.sprite = Frames[_currentFrame];
    }

    public void HandleUpdate()
    {
        _timer += Time.deltaTime;
        while (_timer >= _frameRate)
        {
            _currentFrame = (_currentFrame + 1) % Frames.Count;
            _renderer.sprite = Frames[_currentFrame];
            _timer -= _frameRate;
        }
    }
}