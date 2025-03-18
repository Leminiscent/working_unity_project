using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private List<Sprite> _walkDownSprites;
    [SerializeField] private List<Sprite> _walkUpSprites;
    [SerializeField] private List<Sprite> _walkRightSprites;
    [SerializeField] private List<Sprite> _walkLeftSprites;
    [field: SerializeField, FormerlySerializedAs("_defaultDirection")] public FacingDirection DefaultDirection { get; private set; } = FacingDirection.Down;

    private SpriteAnimator _walkDownAnim;
    private SpriteAnimator _walkUpAnim;
    private SpriteAnimator _walkRightAnim;
    private SpriteAnimator _walkLeftAnim;
    private SpriteAnimator _currentAnim;
    private bool _wasMoving;
    private SpriteRenderer _spriteRenderer;

    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }
    public bool IsJumping { get; set; }
    public FacingDirection FacingDirection
    {
        get => _currentAnim == _walkRightAnim ? FacingDirection.Right :
                   _currentAnim == _walkLeftAnim ? FacingDirection.Left :
                   _currentAnim == _walkUpAnim ? FacingDirection.Up :
                   _currentAnim == _walkDownAnim ? FacingDirection.Down : DefaultDirection;
        set
        {
            SetFacingDirection(value);
            _currentAnim = GetAnimForFacingDirection(value);
            _currentAnim.Start();
        }
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _walkDownAnim = new SpriteAnimator(_walkDownSprites, _spriteRenderer);
        _walkUpAnim = new SpriteAnimator(_walkUpSprites, _spriteRenderer);
        _walkRightAnim = new SpriteAnimator(_walkRightSprites, _spriteRenderer);
        _walkLeftAnim = new SpriteAnimator(_walkLeftSprites, _spriteRenderer);
        SetFacingDirection(DefaultDirection);
        _currentAnim = _walkDownAnim;
    }

    private void Update()
    {
        UpdateAnimation();
    }

    public List<Sprite> GetAllSprites()
    {
        return new List<Sprite>(_walkDownSprites.Concat(_walkUpSprites)
                                                .Concat(_walkRightSprites)
                                                .Concat(_walkLeftSprites));
    }

    public void SetSprites(List<Sprite> walkDown, List<Sprite> walkUp, List<Sprite> walkRight, List<Sprite> walkLeft)
    {
        _walkDownSprites = walkDown;
        _walkUpSprites = walkUp;
        _walkRightSprites = walkRight;
        _walkLeftSprites = walkLeft;
        _walkDownAnim = new SpriteAnimator(_walkDownSprites, _spriteRenderer);
        _walkUpAnim = new SpriteAnimator(_walkUpSprites, _spriteRenderer);
        _walkRightAnim = new SpriteAnimator(_walkRightSprites, _spriteRenderer);
        _walkLeftAnim = new SpriteAnimator(_walkLeftSprites, _spriteRenderer);
        _currentAnim = GetAnimForFacingDirection(FacingDirection);
    }

    private SpriteAnimator GetAnimForFacingDirection(FacingDirection dir)
    {
        return dir switch
        {
            FacingDirection.Up => _walkUpAnim,
            FacingDirection.Down => _walkDownAnim,
            FacingDirection.Left => _walkLeftAnim,
            FacingDirection.Right => _walkRightAnim,
            _ => _walkDownAnim,
        };
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

    private void UpdateAnimation()
    {
        SpriteAnimator previousAnim = _currentAnim;

        // Determine new animation based on movement input values.
        if (MoveX == 1)
        {
            _currentAnim = _walkRightAnim;
        }
        else if (MoveX == -1)
        {
            _currentAnim = _walkLeftAnim;
        }
        else if (MoveY == 1)
        {
            _currentAnim = _walkUpAnim;
        }
        else if (MoveY == -1)
        {
            _currentAnim = _walkDownAnim;
        }

        // Restart the animation if there is a change in animation or movement state.
        if (_currentAnim != previousAnim || IsMoving != _wasMoving)
        {
            _currentAnim.Start();
        }

        // Update the sprite based on the current state.
        if (IsJumping)
        {
            // While jumping, display the last frame of the current animation.
            _spriteRenderer.sprite = _currentAnim.Frames[^1];
        }
        else if (IsMoving)
        {
            _currentAnim.HandleUpdate();
        }
        else
        {
            // When idle, display the first frame of the current animation.
            _spriteRenderer.sprite = _currentAnim.Frames[0];
        }

        _wasMoving = IsMoving;
    }
}

public enum FacingDirection
{
    Up,
    Down,
    Left,
    Right
}