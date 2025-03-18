using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] private string _name;

    private Battler _playerBattler;
    private Vector2 _input;
    private Character _character;
    private DeputyController _deputy;
    private IPlayerTriggerable _currentlyInTrigger;
    private bool _isInteracting;

    public static PlayerController Instance { get; private set; }
    public string Name => _name;
    public Character Character => _character;
    public DeputyController Deputy { get => _deputy; set => _deputy = value; }

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

        _character = GetComponent<Character>();
        _deputy = FindObjectOfType<DeputyController>();
    }

    private IEnumerator Interact()
    {
        while (_character.IsMoving)
        {
            yield return null;
        }

        Vector3 facingDir = new(_character.Animator.MoveX, _character.Animator.MoveY);
        Vector3 interactPos = transform.position + facingDir;
        Collider2D collider = Physics2D.OverlapCircle(interactPos, 0.1f, GameLayers.Instance.InteractablesLayer);

        if (collider != null)
        {
            // AudioManager.Instance.PlaySFX(AudioID.Interact);
            _character.Animator.IsMoving = false;
            yield return collider.GetComponent<IInteractable>()?.Interact(transform);
        }
        _isInteracting = false;
    }

    private void OnMoveOver()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, _character.OffestY), 0.2f, GameLayers.Instance.TriggerableLayers);
        IPlayerTriggerable triggerable = null;

        foreach (Collider2D collider in colliders)
        {
            triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                if (triggerable == _currentlyInTrigger && !triggerable.TriggerRepeatedly)
                {
                    break;
                }
                triggerable.OnPlayerTriggered(this);
                _currentlyInTrigger = triggerable;
                break;
            }
        }

        if (colliders.Count() == 0 || triggerable != _currentlyInTrigger)
        {
            _currentlyInTrigger = null;
        }
    }

    public void HandleUpdate()
    {
        if (!_character.IsMoving)
        {
            _input.x = Input.GetAxisRaw("Horizontal");
            _input.y = Input.GetAxisRaw("Vertical");

            if (_input.x != 0)
            {
                _input.y = 0;
            }

            if (_input != Vector2.zero)
            {
                StartCoroutine(_character.Move(_input, OnMoveOver));
            }
        }

        _character.HandleUpdate();

        if (Input.GetButtonDown("Action") && !_isInteracting)
        {
            _isInteracting = true;
            StartCoroutine(Interact());
        }
    }

    public void SetPlayerBattler(Battler battler)
    {
        _playerBattler = battler ?? throw new ArgumentNullException(nameof(battler));
        _playerBattler.IsCommander = true;
        _name = battler.Base.Name;

        BattleParty party = GetComponent<BattleParty>();
        if (!party.Battlers.Contains(battler))
        {
            party.AddMember(battler);
        }

        _character.Animator.SetSprites(battler.Base.WalkDownSprites,
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
            FacingDirection = _character.Animator.FacingDirection,
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
        _character.Animator.FacingDirection = saveData.FacingDirection;

        List<Battler> battlers = saveData.Battlers.Select(static s => new Battler(s)).ToList();
        GetComponent<BattleParty>().Battlers = battlers;

        SetPlayerBattler(battlers[saveData.PlayerBattlerIndex]);
        party.PartyUpdated();
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