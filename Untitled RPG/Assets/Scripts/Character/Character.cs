using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private GameObject _exclamation;

    private CharacterAnimator _animator;

    public float MoveSpeed;
    public bool IsMoving { get; private set; }
    public float OffestY { get; private set; } = 0.3f;
    public GameObject Exclamation => _exclamation;
    public CharacterAnimator Animator => _animator;
    public event Action<Vector3> OnMoveStart;

    private void Awake()
    {
        _animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f + OffestY;
        transform.position = pos;
    }

    public IEnumerator Move(Vector2 moveVector, Action OnMoveOver = null, bool checkCollisions = true)
    {
        _animator.MoveX = Mathf.Clamp(moveVector.x, -1f, 1f);
        _animator.MoveY = Mathf.Clamp(moveVector.y, -1f, 1f);

        Vector3 targetPos = transform.position + new Vector3(moveVector.x, moveVector.y);
        Ledge ledge = CheckForLedge(targetPos);

        if (ledge != null)
        {
            if (ledge.CanJump(moveVector))
            {
                yield return Jump(moveVector, OnMoveOver);
                yield break;
            }
        }

        if (checkCollisions && !IsPathClear(targetPos))
        {
            yield break;
        }

        IsMoving = true;
        OnMoveStart?.Invoke(transform.position);

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, MoveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        IsMoving = false;
        OnMoveOver?.Invoke();
    }

    public IEnumerator Jump(Vector2 moveDir, Action OnMoveOver = null)
    {
        IsMoving = true;
        OnMoveStart?.Invoke(transform.position);

        _animator.IsJumping = true;

        Vector3 jumpDest = transform.position + (new Vector3(moveDir.x, moveDir.y) * 2);

        yield return transform.DOJump(jumpDest, 1.42f, 1, 0.34f).WaitForCompletion();

        _animator.IsJumping = false;
        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        _animator.IsMoving = IsMoving;
    }

    public bool IsPathClear(Vector3 targetPos)
    {
        Vector3 diff = targetPos - transform.position;
        Vector3 dir = diff.normalized;
        int collisionLayer = GameLayers.Instance.SolidObjectsLayer |
                             GameLayers.Instance.InteractablesLayer |
                             GameLayers.Instance.PlayerLayer;

        return !Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, collisionLayer);
    }

    private Ledge CheckForLedge(Vector3 targetPos)
    {
        Collider2D collider = Physics2D.OverlapCircle(targetPos, 0.15f, GameLayers.Instance.LedgeLayer);
        return collider != null ? collider.GetComponent<Ledge>() : null;
    }

    public void LookTowards(Vector3 target)
    {
        float xdiff = Mathf.Floor(target.x) - Mathf.Floor(transform.position.x);
        float ydiff = Mathf.Floor(target.y) - Mathf.Floor(transform.position.y);

        if (xdiff == 0 || ydiff == 0)
        {
            _animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            _animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
        }
    }
}
