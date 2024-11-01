using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DeputyController : MonoBehaviour, ISavable
{
    private CharacterAnimator _animator;
    private bool _isMoving;
    private PlayerController _player;
    private float _moveSpeed;
    private Queue<Vector3> _positionQueue = new();

    private void Awake()
    {
        _player = FindObjectOfType<PlayerController>();
        _animator = GetComponent<CharacterAnimator>();
    }

    private void Start()
    {
        _moveSpeed = _player.Character.MoveSpeed;
        _player.Character.OnMoveStart += OnPlayerMoveStart;
        SetPosition();
    }

    private void Update()
    {
        if (!_isMoving && _positionQueue.Count > 0)
        {
            Vector3 targetPosition = _positionQueue.Dequeue();
            StartCoroutine(MoveToPosition(targetPosition));
        }
    }

    private void OnPlayerMoveStart(Vector3 playerPosition)
    {
        _positionQueue.Enqueue(playerPosition);
    }

    public void SetPosition()
    {
        transform.position = _player.transform.position;
        _animator.IsMoving = false;
        _isMoving = false;
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
        Collider2D collider = Physics2D.OverlapCircle(targetPos, 0.15f, GameLayers.Instance.LedgeLayer);

        return collider?.GetComponent<Ledge>();
    }

    private IEnumerator Jump(Vector3 jumpDest)
    {
        _isMoving = true;
        _animator.IsJumping = true;

        yield return transform.DOJump(jumpDest, 1.42f, 1, 0.34f).WaitForCompletion();

        _animator.IsJumping = false;
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
}

[Serializable]
public class DeputySaveData
{
    public float[] Position;
    public FacingDirection FacingDirection;
}