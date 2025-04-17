using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeputyController : MonoBehaviour, ISavable
{
    private Character _character;
    private PlayerController _player;
    private BattleParty _party;
    private Queue<Vector3> _positionQueue = new();
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _player = FindObjectOfType<PlayerController>();
        _party = _player.GetComponent<BattleParty>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _player.Character.OnMoveStart += OnPlayerMoveStart;
        _party.OnUpdated += UpdateDeputyBattler;
        UpdateDeputyBattler();
    }

    private void OnDestroy()
    {
        _player.Character.OnMoveStart -= OnPlayerMoveStart;
        _party.OnUpdated -= UpdateDeputyBattler;
    }

    private void Update()
    {
        if (_party.Battlers.Count < 2)
        {
            return;
        }

        if (!_character.IsMoving && _positionQueue.Count > 0)
        {
            Vector3 targetPos = _positionQueue.Dequeue();
            _ = StartCoroutine(FollowRoutine(targetPos));
        }

        _character.UpdateAnimator();
    }

    public void UpdateDeputyBattler()
    {
        if (_party.Battlers.Count < 2)
        {
            SetSpriteVisibility(false);
            return;
        }

        SetSpriteVisibility(true);
        Battler deputy = _party.Battlers.First(static b => !b.IsCommander);

        // Set the new Deputy's sprites
        _character.Animator.SetSprites(
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

        SetSpriteVisibility(true);

        // Snap to the player’s tile
        _character.SnapToTile(_player.transform.position);
        _character.Animator.IsMoving = false;
    }

    public object CaptureState()
    {
        return new DeputySaveData
        {
            Position = new float[] { transform.position.x, transform.position.y },
            FacingDirection = _character.Animator.FacingDirection
        };
    }

    public void RestoreState(object state)
    {
        DeputySaveData saveData = (DeputySaveData)state;
        _character.SnapToTile(new Vector2(saveData.Position[0], saveData.Position[1]));
        _character.Animator.FacingDirection = saveData.FacingDirection;
    }

    private IEnumerator FollowRoutine(Vector3 targetPos)
    {
        // Step one tile at a time until we reach the dequeue’d position
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            Vector2Int currentGrid = new(
                Mathf.FloorToInt(transform.position.x),
                Mathf.FloorToInt(transform.position.y));
            Vector2Int targetGrid = new(
                Mathf.FloorToInt(targetPos.x),
                Mathf.FloorToInt(targetPos.y));
            Vector2Int diff = targetGrid - currentGrid;

            // Normalize to a single-axis step
            Vector2 moveVec = diff.x != 0
                ? new Vector2(Mathf.Sign(diff.x), 0)
                : new Vector2(0, Mathf.Sign(diff.y));

            yield return StartCoroutine(_character.MoveRoutine(moveVec));
        }
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