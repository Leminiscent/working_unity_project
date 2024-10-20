using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    CharacterAnimator animator;
    public float moveSpeed;
    public bool IsMoving { get; private set; }
    public float offestY { get; private set; } = 0.3f;

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f + offestY;
        transform.position = pos;
    }

    public IEnumerator Move(Vector2 moveVector, Action OnMoveOver = null, bool checkCollisions = true)
    {
        animator.MoveX = Mathf.Clamp(moveVector.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVector.y, -1f, 1f);

        var targetPos = transform.position;

        targetPos.x += moveVector.x;
        targetPos.y += moveVector.y;

        var ledge = CheckForLedge(targetPos);

        if (ledge != null)
        {
            if (ledge.CanJump(this, moveVector))
            {
                yield break;
            }
        }

        if (checkCollisions && !IsPathClear(targetPos))
        {
            yield break;
        }

        IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;
        var collisionLayer = GameLayers.Instance.SolidObjectsLayer | GameLayers.Instance.InteractablesLayer | GameLayers.Instance.PlayerLayer;

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, collisionLayer) == true)
        {
            return false;
        }
        return true;
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.1f, GameLayers.Instance.SolidObjectsLayer | GameLayers.Instance.InteractablesLayer) != null)
        {
            return false;
        }
        return true;
    }

    Ledge CheckForLedge(Vector3 targetPos)
    {
        var collider = Physics2D.OverlapCircle(targetPos, 0.15f, GameLayers.Instance.LedgeLayer);

        return collider?.GetComponent<Ledge>();
    }

    public void LookTowards(Vector3 target)
    {
        var xdiff = Mathf.Floor(target.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(target.y) - Mathf.Floor(transform.position.y);

        if (xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
        }
    }

    public CharacterAnimator Animator => animator;
}
