using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class DeputyController : MonoBehaviour, ISavable
{
    private const float LedgeCheckRadius = 0.15f;
    private const float JumpPower = 1.42f;
    private const int JumpNumJumps = 1;
    private const float JumpDuration = 0.34f;

    private CharacterAnimator _animator;
    private bool _isMoving;
    private PlayerController _player;
    private BattleParty _party;
    private float _moveSpeed;
    private Queue<Vector3> _positionQueue = new();
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _animator = GetComponent<CharacterAnimator>();
        _player = FindObjectOfType<PlayerController>();
        _party = _player.GetComponent<BattleParty>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _moveSpeed = _player.Character.MoveSpeed;
        _player.Character.OnMoveStart += OnPlayerMoveStart;
        _party.OnUpdated += UpdateDeputyBattler;

        UpdateDeputyBattler();
    }

    private void Update()
    {
        if (_party.Battlers.Count < 2)
        {
            return;
        }

        if (!_isMoving && _positionQueue.Count > 0)
        {
            Vector3 targetPosition = _positionQueue.Dequeue();
            _ = StartCoroutine(MoveToPosition(targetPosition));
        }
    }

    public void UpdateDeputyBattler()
    {
        if (_party.Battlers.Count < 2)
        {
            SetSpriteVisibility(false);
            return;
        }
        else
        {
            SetSpriteVisibility(true);
        }

        // Get the first battler who is not the commander.
        Battler deputy = _party.Battlers.First(static battler => !battler.IsCommander);

        _animator.SetSprites(
            deputy.Base.WalkDownSprites,
            deputy.Base.WalkUpSprites,
            deputy.Base.WalkRightSprites,
            deputy.Base.WalkLeftSprites);

        SetPosition();
    }

    public void OnPlayerMoveStart(Vector3 playerPosition)
    {
        if (_party.Battlers.Count < 2)
        {
            return;
        }

        _positionQueue.Enqueue(playerPosition);
    }

    public void SetPosition()
    {
        if (_party.Battlers.Count < 2)
        {
            SetSpriteVisibility(false);
            return;
        }
        else
        {
            SetSpriteVisibility(true);
        }

        transform.position = _player.transform.position;
        _animator.IsMoving = false;
        _isMoving = false;
    }

    public object CaptureState()
    {
        DeputySaveData saveData = new()
        {
            Position = new float[] { transform.position.x, transform.position.y },
            FacingDirection = _animator.FacingDirection,
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
        DeputySaveData saveData = (DeputySaveData)state;
        transform.position = new Vector3(saveData.Position[0], saveData.Position[1]);
        _animator.FacingDirection = saveData.FacingDirection;
    }

    private IEnumerator MoveToPosition(Vector3 targetPos)
    {
        _isMoving = true;
        _animator.IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            Vector2 moveVec = GetNextMoveVector(transform.position, targetPos);

            _animator.MoveX = moveVec.x;
            _animator.MoveY = moveVec.y;

            Vector3 nextPos = transform.position + (Vector3)moveVec;
            Ledge ledge = CheckForLedge(nextPos);

            if (ledge != null)
            {
                if (ledge.CanJump(moveVec))
                {
                    Vector3 jumpDest = transform.position + ((Vector3)moveVec * 2);
                    yield return Jump(jumpDest);
                    continue;
                }
            }

            while ((nextPos - transform.position).sqrMagnitude > Mathf.Epsilon)
            {
                transform.position = Vector3.MoveTowards(transform.position, nextPos, _moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = nextPos;
        }

        _animator.IsMoving = false;
        _isMoving = false;
    }

    private Vector2 GetNextMoveVector(Vector3 fromPosition, Vector3 toPosition)
    {
        Vector2 direction = toPosition - fromPosition;
        return Mathf.Abs(direction.x) > Mathf.Epsilon
            ? new Vector2(Mathf.Sign(direction.x), 0)
            : Mathf.Abs(direction.y) > Mathf.Epsilon ? new Vector2(0, Mathf.Sign(direction.y)) : Vector2.zero;
    }

    private Ledge CheckForLedge(Vector3 targetPos)
    {
        Collider2D collider = Physics2D.OverlapCircle(targetPos, LedgeCheckRadius, GameLayers.Instance.LedgeLayer);
        return collider != null ? collider.GetComponent<Ledge>() : null;
    }

    private IEnumerator Jump(Vector3 jumpDest)
    {
        _isMoving = true;
        _animator.IsJumping = true;
        yield return transform.DOJump(jumpDest, JumpPower, JumpNumJumps, JumpDuration).WaitForCompletion();
        _animator.IsJumping = false;
        _isMoving = false;
    }

    private void SetSpriteVisibility(bool visible)
    {
        _spriteRenderer.enabled = visible;
    }
}

[Serializable]
public class DeputySaveData
{
    public float[] Position;
    public FacingDirection FacingDirection;
}