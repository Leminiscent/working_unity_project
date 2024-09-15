using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] List<Sprite> sailSprites;
    [SerializeField] FacingDirections defaultDirection = FacingDirections.Down;

    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }
    public bool IsJumping { get; set; }
    public bool IsSailing { get; set; }

    SpriteAnimator walkDownAnim;
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkRightAnim;
    SpriteAnimator walkLeftAnim;
    SpriteAnimator currentAnim;
    bool wasMoving;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);
        SetFacingDirection(defaultDirection);
        currentAnim = walkDownAnim;
    }

    private void Update()
    {
        var prevAnim = currentAnim;

        if (!IsSailing)
        {
            if (MoveX == 1)
            {
                currentAnim = walkRightAnim;
            }
            else if (MoveX == -1)
            {
                currentAnim = walkLeftAnim;
            }
            else if (MoveY == 1)
            {
                currentAnim = walkUpAnim;
            }
            else if (MoveY == -1)
            {
                currentAnim = walkDownAnim;
            }

            if (currentAnim != prevAnim || IsMoving != wasMoving)
            {
                currentAnim.Start();
            }

            if (IsJumping)
            {
                spriteRenderer.sprite = currentAnim.Frames[currentAnim.Frames.Count - 1];
            }
            else if (IsMoving)
            {
                currentAnim.HandleUpdate();
            }
            else
            {
                spriteRenderer.sprite = currentAnim.Frames[0];
            }
        }
        else
        {
            if (MoveX == 1)
            {
                spriteRenderer.sprite = sailSprites[2];
            }
            else if (MoveX == -1)
            {
                spriteRenderer.sprite = sailSprites[3];
            }
            else if (MoveY == 1)
            {
                spriteRenderer.sprite = sailSprites[1];
            }
            else if (MoveY == -1)
            {
                spriteRenderer.sprite = sailSprites[0];
            }
        }
        
        wasMoving = IsMoving;
    }

    public void SetFacingDirection(FacingDirections dir)
    {
        if (dir == FacingDirections.Right)
        {
            MoveX = 1;
        }
        else if (dir == FacingDirections.Left)
        {
            MoveX = -1;
        }
        else if (dir == FacingDirections.Up)
        {
            MoveY = 1;
        }
        else if (dir == FacingDirections.Down)
        {
            MoveY = -1;
        }
    }

    public FacingDirections DefaultDirection => defaultDirection;
}

public enum FacingDirections
{
    Up,
    Down,
    Left,
    Right
}