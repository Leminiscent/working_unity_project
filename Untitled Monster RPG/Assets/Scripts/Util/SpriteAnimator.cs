using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator
{
    private SpriteRenderer renderer;
    private List<Sprite> frames;
    private float frameRate;
    private int currentFrame;
    private float timer;

    public SpriteAnimator(List<Sprite> frames, SpriteRenderer renderer, float frameRate = 0.16f)
    {
        this.frames = frames;
        this.renderer = renderer;
        this.frameRate = frameRate;
    }

    public void Start()
    {
        currentFrame = 1;
        timer = 0f;
        renderer.sprite = frames[currentFrame];
    }

    public void HandleUpdate()
    {
        timer += Time.deltaTime;
        if (timer > frameRate)
        {
            currentFrame = (currentFrame + 1) % frames.Count;
            renderer.sprite = frames[currentFrame];
            timer -= frameRate;
        }
    }

    public List<Sprite> Frames => frames;
}