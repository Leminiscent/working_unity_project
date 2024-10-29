using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] new string name;
    [SerializeField] Sprite sprite;
    private Vector2 input;
    private Character character;
    private DeputyController deputy;
    public static PlayerController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        character = GetComponent<Character>();
        deputy = FindObjectOfType<DeputyController>();
    }

    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            StartCoroutine(Interact());
        }
    }

    IEnumerator Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;
        var collider = Physics2D.OverlapCircle(interactPos, 0.1f, GameLayers.Instance.InteractablesLayer);

        if (collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    IPlayerTriggerable currentlyInTrigger;

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffestY), 0.2f, GameLayers.Instance.TriggerableLayers);
        IPlayerTriggerable triggerable = null;

        foreach (var collider in colliders)
        {
            triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                if (triggerable == currentlyInTrigger && !triggerable.TriggerRepeatedly)
                {
                    break;
                }
                triggerable.OnPlayerTriggered(this);
                currentlyInTrigger = triggerable;
                break;
            }
        }

        if (colliders.Count() == 0 || triggerable != currentlyInTrigger)
        {
            currentlyInTrigger = null;
        }
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData
        {
            position = new float[] { transform.position.x, transform.position.y },
            facingDirection = character.Animator.FacingDirection,
            monsters = GetComponent<MonsterParty>().Monsters.Select(m => m.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;

        transform.position = new Vector3(saveData.position[0], saveData.position[1]);
        character.Animator.FacingDirection = saveData.facingDirection;
        GetComponent<MonsterParty>().Monsters = saveData.monsters.Select(s => new Monster(s)).ToList();
    }

    public string Name => name;
    public Sprite Sprite => sprite;
    public Character Character => character;
    public DeputyController Deputy { get => deputy; set => deputy = value; }
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public FacingDirection facingDirection;
    public List<MonsterSaveData> monsters;
}