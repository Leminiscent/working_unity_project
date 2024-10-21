using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeputyController : MonoBehaviour
{
    private CharacterAnimator animator;
    private bool isMoving;
    private PlayerController player;
    private float moveSpeed;
    private Queue<Vector3> positionQueue = new();

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        animator = GetComponent<CharacterAnimator>();
        SetPosition();
        moveSpeed = player.Character.moveSpeed;
        player.Character.OnMoveStart += OnPlayerMoveStart;
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.Character.OnMoveStart -= OnPlayerMoveStart;
        }
    }

    private void Update()
    {
        if (!isMoving && positionQueue.Count > 0)
        {
            Vector3 targetPosition = positionQueue.Dequeue();
            StartCoroutine(MoveToPosition(targetPosition));
        }
    }

    private void OnPlayerMoveStart(Vector3 playerPosition)
    {
        positionQueue.Enqueue(playerPosition);
    }

    public void SetPosition()
    {
        transform.position = player.transform.position;
        animator.IsMoving = false;
        isMoving = false;
    }

    private IEnumerator MoveToPosition(Vector3 targetPos)
    {
        isMoving = true;
        animator.IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            Vector2 moveVec = GetNextMoveVector(transform.position, targetPos);

            animator.MoveX = moveVec.x;
            animator.MoveY = moveVec.y;

            Vector3 nextPos = transform.position + (Vector3)moveVec;

            while ((nextPos - transform.position).sqrMagnitude > Mathf.Epsilon)
            {
                transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = nextPos;
        }

        animator.IsMoving = false;
        isMoving = false;
    }

    private Vector2 GetNextMoveVector(Vector3 fromPosition, Vector3 toPosition)
    {
        Vector2 direction = toPosition - fromPosition;

        if (Mathf.Abs(direction.x) > Mathf.Epsilon)
        {
            return new Vector2(Mathf.Sign(direction.x), 0);
        }
        else if (Mathf.Abs(direction.y) > Mathf.Epsilon)
        {
            return new Vector2(0, Mathf.Sign(direction.y));
        }
        else
        {
            return Vector2.zero;
        }
    }
}