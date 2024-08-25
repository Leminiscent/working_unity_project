using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] new string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialogue dialogue;
    [SerializeField] Dialogue postBattleDialogue;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject los;
    bool battleLost = false;
    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetLosRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public void Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);
        if (!battleLost)
        {
            StartCoroutine(DialogueManager.Instance.ShowDialogue(dialogue, () =>
            {
                GameController.Instance.StartMasterBattle(this);
            }));
        }
        else
        {
            StartCoroutine(DialogueManager.Instance.ShowDialogue(postBattleDialogue));
        }
    }

    public IEnumerator TriggerBattle(PlayerController player)
    {
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        var diff = player.transform.position - transform.position;
        var moveVector = diff - diff.normalized;
        moveVector = new Vector2(Mathf.Round(moveVector.x), Mathf.Round(moveVector.y));

        yield return character.Move(moveVector);

        StartCoroutine(DialogueManager.Instance.ShowDialogue(dialogue, () =>
        {
            GameController.Instance.StartMasterBattle(this);
        }));
    }

    public void BattleLost()
    {
        battleLost = true;
        los.gameObject.SetActive(false);
    }

    public void SetLosRotation(FacingDirections dir)
    {
        float angle = 0f;

        if (dir == FacingDirections.Right)
        {
            angle = 90f;
        }
        else if (dir == FacingDirections.Up)
        {
            angle = 180f;
        }
        else if (dir == FacingDirections.Left)
        {
            angle = 270f;
        }
        los.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool)state;

        if (battleLost)
        {
            los.gameObject.SetActive(false);
        }
    }

    public string Name => name;
    public Sprite Sprite => sprite;
}
