using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour, ISavable
{
    [field: SerializeField, FormerlySerializedAs("_name")] public string Name { get; private set; }

    private Battler _playerBattler;
    private Vector2 _input;
    private IPlayerTriggerable _currentlyInTrigger;
    private bool _isInteracting;

    public static PlayerController Instance { get; private set; }
    public Character Character { get; private set; }
    public DeputyController Deputy { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        Character = GetComponent<Character>();
        Deputy = FindObjectOfType<DeputyController>();
    }

    public void HandleUpdate()
    {
        if (!Character.IsMoving)
        {
            // Capture input values.
            _input.x = Input.GetAxisRaw("Horizontal");
            _input.y = Input.GetAxisRaw("Vertical");

            // Prioritize horizontal movement over vertical when both inputs are present.
            if (_input.x != 0)
            {
                _input.y = 0;
            }

            // Begin movement routine if any input is detected.
            if (_input != Vector2.zero)
            {
                _ = StartCoroutine(Character.MoveRoutine(_input, OnMoveOver));
            }
        }

        Character.UpdateAnimator();

        // Handle interaction input.
        if (Input.GetButtonDown("Action") && !_isInteracting)
        {
            _isInteracting = true;
            _ = StartCoroutine(Interact());
        }
    }

    public void SetPlayerBattler(Battler battler)
    {
        _playerBattler = battler ?? throw new ArgumentNullException(nameof(battler));
        _playerBattler.IsCommander = true;
        Name = battler.Base.Name;

        BattleParty party = GetComponent<BattleParty>();
        if (!party.Battlers.Contains(battler))
        {
            party.AddMember(battler);
        }

        Character.Animator.SetSprites(battler.Base.WalkDownSprites,
                                       battler.Base.WalkUpSprites,
                                       battler.Base.WalkRightSprites,
                                       battler.Base.WalkLeftSprites);
    }

    public object CaptureState()
    {
        List<Battler> party = GetComponent<BattleParty>().Battlers;
        int playerIndex = party.FindIndex(static b => b.IsCommander);

        PlayerSaveData saveData = new()
        {
            Position = new float[] { transform.position.x, transform.position.y },
            FacingDirection = Character.Animator.FacingDirection,
            Battlers = party.Select(static b => b.GetSaveData()).ToList(),
            PlayerBattlerIndex = playerIndex
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        PlayerSaveData saveData = (PlayerSaveData)state;
        BattleParty party = GetComponent<BattleParty>();

        transform.position = new Vector3(saveData.Position[0], saveData.Position[1]);
        Character.Animator.FacingDirection = saveData.FacingDirection;

        List<Battler> battlers = saveData.Battlers.Select(static s => new Battler(s)).ToList();
        party.Battlers = battlers;

        SetPlayerBattler(battlers[saveData.PlayerBattlerIndex]);
        party.PartyUpdated();
    }

    private IEnumerator Interact()
    {
        // Wait until the player finishes moving.
        while (Character.IsMoving)
        {
            yield return null;
        }

        Vector3 facingDir = new(Character.Animator.MoveX, Character.Animator.MoveY);
        Vector3 interactPos = transform.position + facingDir;
        Collider2D collider = Physics2D.OverlapCircle(interactPos, 0.1f, GameLayers.Instance.InteractablesLayer);

        if (collider != null)
        {
            // AudioManager.Instance.PlaySFX(AudioID.Interact);
            Character.Animator.IsMoving = false;
            yield return collider.GetComponent<IInteractable>()?.Interact(transform);
        }
        _isInteracting = false;
    }

    private void OnMoveOver()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position - new Vector3(0, Character.OffsetY),
            0.2f,
            GameLayers.Instance.TriggerableLayers);

        IPlayerTriggerable triggerable = null;
        foreach (Collider2D collider in colliders)
        {
            triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                // If already in this trigger and it doesn't repeat, do nothing.
                if (triggerable == _currentlyInTrigger && !triggerable.TriggerRepeatedly)
                {
                    break;
                }
                triggerable.OnPlayerTriggered(this);
                _currentlyInTrigger = triggerable;
                break;
            }
        }

        // Reset the current trigger if none found or if a different one is active.
        if (colliders.Length == 0 || triggerable != _currentlyInTrigger)
        {
            _currentlyInTrigger = null;
        }
    }
}

[Serializable]
public class PlayerSaveData
{
    public float[] Position;
    public FacingDirection FacingDirection;
    public List<BattlerSaveData> Battlers;
    public int PlayerBattlerIndex;
}