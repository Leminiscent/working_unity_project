using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator
{
    private SpriteRenderer _renderer;
    private List<Sprite> _frames;
    private float _frameRate;
    private int _currentFrame;
    private float _timer;

    public List<Sprite> Frames => _frames;

    public SpriteAnimator(List<Sprite> frames, SpriteRenderer renderer, float frameRate = 0.16f)
    {
        _frames = frames;
        _renderer = renderer;
        _frameRate = frameRate;
    }

    public void Start()
    {
        _currentFrame = 1;
        _timer = 0f;
        _renderer.sprite = _frames[_currentFrame];
    }

    public void HandleUpdate()
    {
        _timer += Time.deltaTime;
        if (_timer > _frameRate)
        {
            _currentFrame = (_currentFrame + 1) % _frames.Count;
            _renderer.sprite = _frames[_currentFrame];
            _timer -= _frameRate;
        }
    }
}