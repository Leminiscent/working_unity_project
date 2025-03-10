using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] private string _name;

    private Monster _playerMonster;
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

    public void SetPlayerMonster(Monster monster)
    {
        if (_playerMonster != null)
        {
            return;
        }

        _playerMonster = monster ?? throw new ArgumentNullException(nameof(monster));
        _playerMonster.IsPlayer = true;
        _name = monster.Base.Name;

        MonsterParty party = GetComponent<MonsterParty>();
        if (!party.Monsters.Contains(monster))
        {
            party.AddMonster(monster);
        }

        _character.Animator.SetSprites(monster.Base.WalkDownSprites,
                            monster.Base.WalkUpSprites,
                            monster.Base.WalkRightSprites,
                            monster.Base.WalkLeftSprites);
    }

    public object CaptureState()
    {
        List<Monster> party = GetComponent<MonsterParty>().Monsters;
        int playerIndex = party.FindIndex(static m => m.IsPlayer);

        PlayerSaveData saveData = new()
        {
            Position = new float[] { transform.position.x, transform.position.y },
            FacingDirection = _character.Animator.FacingDirection,
            Monsters = party.Select(static m => m.GetSaveData()).ToList(),
            PlayerMonsterIndex = playerIndex
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        PlayerSaveData saveData = (PlayerSaveData)state;

        transform.position = new Vector3(saveData.Position[0], saveData.Position[1]);
        _character.Animator.FacingDirection = saveData.FacingDirection;

        List<Monster> monsters = saveData.Monsters.Select(static s => new Monster(s)).ToList();
        GetComponent<MonsterParty>().Monsters = monsters;

        SetPlayerMonster(monsters[saveData.PlayerMonsterIndex]);
    }
}

[Serializable]
public class PlayerSaveData
{
    public float[] Position;
    public FacingDirection FacingDirection;
    public List<MonsterSaveData> Monsters;
    public int PlayerMonsterIndex;
}