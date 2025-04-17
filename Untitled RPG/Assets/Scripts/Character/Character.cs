using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class Character : MonoBehaviour
{
    [field: SerializeField, FormerlySerializedAs("_exclamation")] public GameObject Exclamation { get; private set; }

    private const float SNAP_OFFSET = 0.5f;
    private const float OVERLAP_CIRCLE_RADIUS = 0.15f;
    private const float BOX_CAST_SIZE = 0.2f;
    private const float LEDGE_JUMP_MULTIPLIER = 2f;
    private const float JUMP_POWER = 1.42f;
    private const int NUM_JUMPS = 1;
    private const float JUMP_DURATION = 0.34f;
    private static readonly HashSet<Vector2Int> _reservedTiles = new();
    private bool _hasReservedTile = false;
    private Vector2Int _reservedTile;

    public float MoveSpeed;
    public bool IsMoving { get; private set; }
    public float OffsetY { get; private set; } = 0.3f;
    public CharacterAnimator Animator { get; private set; }
    public event Action<Vector3> OnMoveStart;

    private void Awake()
    {
        Animator = GetComponent<CharacterAnimator>();
        SnapToTile(transform.position);
    }

    public void SnapToTile(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + SNAP_OFFSET;
        pos.y = Mathf.Floor(pos.y) + SNAP_OFFSET + OffsetY;
        transform.position = pos;
    }

    public IEnumerator MoveRoutine(Vector2 moveVector, Action OnMoveOver = null, bool checkCollisions = true)
    {
        // Update animator with movement direction
        Animator.MoveX = Mathf.Clamp(moveVector.x, -1f, 1f);
        Animator.MoveY = Mathf.Clamp(moveVector.y, -1f, 1f);

        Vector3 targetPos = transform.position + new Vector3(moveVector.x, moveVector.y);

        // Check for a ledge and handle jump if applicable
        Ledge ledge = DetectLedge(targetPos);
        if (ledge != null && ledge.CanJump(moveVector))
        {
            yield return JumpRoutine(moveVector, OnMoveOver);
            yield break;
        }

        // Check for obstacles if needed
        if (checkCollisions && !IsPathClear(targetPos))
        {
            yield break;
        }

        // Reserve the target tile to prevent simultaneous moves
        Vector2Int gridPos = new(
            Mathf.FloorToInt(targetPos.x),
            Mathf.FloorToInt(targetPos.y)
        );
        if (_reservedTiles.Contains(gridPos))
        {
            yield break;
        }
        _ = _reservedTiles.Add(gridPos);
        _hasReservedTile = true;
        _reservedTile = gridPos;

        StartMovement(transform.position);

        // Smoothly move towards the target position
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, MoveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        EndMovement();
        OnMoveOver?.Invoke();
    }

    public IEnumerator JumpRoutine(Vector2 moveDir, Action OnMoveOver = null)
    {
        // Compute jump destination
        Vector3 jumpDestination = transform.position + (new Vector3(moveDir.x, moveDir.y) * LEDGE_JUMP_MULTIPLIER);

        // Reserve landing tile
        Vector2Int landingGrid = new(
            Mathf.FloorToInt(jumpDestination.x),
            Mathf.FloorToInt(jumpDestination.y)
        );
        if (_reservedTiles.Contains(landingGrid))
        {
            yield break;
        }
        _ = _reservedTiles.Add(landingGrid);
        _hasReservedTile = true;
        _reservedTile = landingGrid;

        StartMovement(transform.position);
        Animator.IsJumping = true;

        yield return transform.DOJump(jumpDestination, JUMP_POWER, NUM_JUMPS, JUMP_DURATION)
                                .WaitForCompletion();

        Animator.IsJumping = false;
        transform.position = jumpDestination;

        EndMovement();
        OnMoveOver?.Invoke();
    }

    public void UpdateAnimator()
    {
        Animator.IsMoving = IsMoving;
    }

    public bool IsPathClear(Vector3 targetPos)
    {
        Vector3 diff = targetPos - transform.position;
        Vector3 direction = diff.normalized;
        int collisionLayer = GameLayers.Instance.SolidObjectsLayer |
                             GameLayers.Instance.InteractablesLayer |
                             GameLayers.Instance.PlayerLayer;

        // Adjust the distance for the BoxCast so it doesn't detect the character itself
        float distance = diff.magnitude - 1f;

        return !Physics2D.BoxCast(transform.position + direction,
                                  new Vector2(BOX_CAST_SIZE, BOX_CAST_SIZE),
                                  0f,
                                  direction,
                                  distance,
                                  collisionLayer);
    }

    public void LookTowards(Vector3 target)
    {
        float xDiff = Mathf.Floor(target.x) - Mathf.Floor(transform.position.x);
        float yDiff = Mathf.Floor(target.y) - Mathf.Floor(transform.position.y);

        // Only adjust direction if movement is axis-aligned
        if (xDiff == 0 || yDiff == 0)
        {
            Animator.MoveX = Mathf.Clamp(xDiff, -1f, 1f);
            Animator.MoveY = Mathf.Clamp(yDiff, -1f, 1f);
        }
    }

    private Ledge DetectLedge(Vector3 targetPos)
    {
        Collider2D collider = Physics2D.OverlapCircle(targetPos, OVERLAP_CIRCLE_RADIUS, GameLayers.Instance.LedgeLayer);
        return collider ? collider.GetComponent<Ledge>() : null;
    }

    private void StartMovement(Vector3 startPosition)
    {
        IsMoving = true;
        OnMoveStart?.Invoke(startPosition);
    }

    private void EndMovement()
    {
        IsMoving = false;
        // Release reservation
        if (_hasReservedTile)
        {
            _ = _reservedTiles.Remove(_reservedTile);
            _hasReservedTile = false;
        }
    }
}