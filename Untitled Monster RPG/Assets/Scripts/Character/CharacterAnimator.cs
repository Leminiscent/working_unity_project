using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private List<Sprite> _walkDownSprites;
    [SerializeField] private List<Sprite> _walkUpSprites;
    [SerializeField] private List<Sprite> _walkRightSprites;
    [SerializeField] private List<Sprite> _walkLeftSprites;
    [SerializeField] private FacingDirection _defaultDirection = FacingDirection.Down;

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
    public FacingDirection DefaultDirection => _defaultDirection;
    public FacingDirection FacingDirection
    {
        get
        {
            return _currentAnim == _walkRightAnim
                ? FacingDirection.Right
                : _currentAnim == _walkLeftAnim
                    ? FacingDirection.Left
                    : _currentAnim == _walkUpAnim ? FacingDirection.Up : _currentAnim == _walkDownAnim ? FacingDirection.Down : _defaultDirection;
        }
        set
        {
            SetFacingDirection(value);
            _currentAnim = GetAnimForFacingDirection(value);
            _currentAnim?.Start();
        }
    }

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _walkDownAnim = new SpriteAnimator(_walkDownSprites, _spriteRenderer);
        _walkUpAnim = new SpriteAnimator(_walkUpSprites, _spriteRenderer);
        _walkRightAnim = new SpriteAnimator(_walkRightSprites, _spriteRenderer);
        _walkLeftAnim = new SpriteAnimator(_walkLeftSprites, _spriteRenderer);
        SetFacingDirection(_defaultDirection);
        _currentAnim = _walkDownAnim;
    }

    private void Update()
    {
        SpriteAnimator prevAnim = _currentAnim;

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

        if (_currentAnim != prevAnim || IsMoving != _wasMoving)
        {
            _currentAnim.Start();
        }

        if (IsJumping)
        {
            _spriteRenderer.sprite = _currentAnim.Frames[_currentAnim.Frames.Count - 1];
        }
        else if (IsMoving)
        {
            _currentAnim.HandleUpdate();
        }
        else
        {
            _spriteRenderer.sprite = _currentAnim.Frames[0];
        }

        _wasMoving = IsMoving;
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
            FacingDirection.Up => _walkUpAnim,
            FacingDirection.Down => _walkDownAnim,
            FacingDirection.Left => _walkLeftAnim,
            FacingDirection.Right => _walkRightAnim,
            _ => _walkDownAnim,
        };
    }
}

public enum FacingDirection
{
    Up,
    Down,
    Left,
    Right
}