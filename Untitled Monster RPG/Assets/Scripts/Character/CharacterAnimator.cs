using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private List<Sprite> walkDownSprites;
    [SerializeField] private List<Sprite> walkUpSprites;
    [SerializeField] private List<Sprite> walkRightSprites;
    [SerializeField] private List<Sprite> walkLeftSprites;
    [SerializeField] private FacingDirection defaultDirection = FacingDirection.Down;

    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }
    public bool IsJumping { get; set; }

    private SpriteAnimator walkDownAnim;
    private SpriteAnimator walkUpAnim;
    private SpriteAnimator walkRightAnim;
    private SpriteAnimator walkLeftAnim;
    private SpriteAnimator currentAnim;
    private bool wasMoving;
    private SpriteRenderer spriteRenderer;

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
        SpriteAnimator prevAnim = currentAnim;

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

        wasMoving = IsMoving;
    }

    public void SetFacingDirection(FacingDirection dir)
    {
        MoveX = 0;
        MoveY = 0;

        if (dir == FacingDirection.Right)
        {
            MoveX = 1;
        }
        else if (dir == FacingDirection.Left)
        {
            MoveX = -1;
        }
        else if (dir == FacingDirection.Up)
        {
            MoveY = 1;
        }
        else if (dir == FacingDirection.Down)
        {
            MoveY = -1;
        }
    }

    private SpriteAnimator GetAnimForFacingDirection(FacingDirection dir)
    {
        return dir switch
        {
            FacingDirection.Up => walkUpAnim,
            FacingDirection.Down => walkDownAnim,
            FacingDirection.Left => walkLeftAnim,
            FacingDirection.Right => walkRightAnim,
            _ => walkDownAnim,
        };
    }

    public FacingDirection DefaultDirection => defaultDirection;
    public FacingDirection FacingDirection
    {
        get
        {
            if (currentAnim == walkRightAnim)
            {
                return FacingDirection.Right;
            }
            else if (currentAnim == walkLeftAnim)
            {
                return FacingDirection.Left;
            }
            else if (currentAnim == walkUpAnim)
            {
                return FacingDirection.Up;
            }
            else if (currentAnim == walkDownAnim)
            {
                return FacingDirection.Down;
            }
            else
            {
                return defaultDirection;
            }
        }
        set
        {
            SetFacingDirection(value);
            currentAnim = GetAnimForFacingDirection(value);
            currentAnim.Start();
        }
    }
}

public enum FacingDirection
{
    Up,
    Down,
    Left,
    Right
}