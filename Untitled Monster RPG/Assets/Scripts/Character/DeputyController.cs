using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeputyController : MonoBehaviour
{
    private CharacterAnimator animator;
    private bool isMoving;
    private PlayerController player;
    private float moveSpeed;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>().GetComponent<PlayerController>();
        animator = GetComponent<CharacterAnimator>();
        SetPosition();
    }

    public void Follow(Vector3 movePosition)
    {
        if (isMoving)
            return;

        moveSpeed = player.Character.moveSpeed;
        Vector2 moveVector = movePosition - this.transform.position;
        StartCoroutine(Move(moveVector));
    }

    public void SetPosition()
    {
        this.transform.position = player.transform.position;
        this.animator.IsMoving = false;
        isMoving = false;
    }

    public IEnumerator Move(Vector2 moveVec)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        isMoving = true;
        animator.IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;

        isMoving = false;
        yield return StandStill();
    }

    private IEnumerator StandStill()
    {
        Vector3 myPosition = this.transform.position;
        yield return new WaitForFixedUpdate();

        if (myPosition == transform.position)
        {
            animator.IsMoving = false;
        }
    }
}