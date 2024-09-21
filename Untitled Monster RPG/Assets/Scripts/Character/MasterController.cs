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
    [SerializeField] AudioClip playerDetectedClip;
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

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);
        if (!battleLost)
        {
            AudioManager.Instance.PlayMusic(playerDetectedClip);
            yield return DialogueManager.Instance.ShowDialogue(dialogue);
            GameController.Instance.StartMasterBattle(this);
        }
        else
        {
            yield return DialogueManager.Instance.ShowDialogue(postBattleDialogue);
        }
    }

    public IEnumerator TriggerBattle(PlayerController player)
    {
        AudioManager.Instance.PlayMusic(playerDetectedClip);
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        var diff = player.transform.position - transform.position;
        var moveVector = diff - diff.normalized;
        moveVector = new Vector2(Mathf.Round(moveVector.x), Mathf.Round(moveVector.y));

        yield return character.Move(moveVector);
        
        yield return DialogueManager.Instance.ShowDialogue(dialogue);
        GameController.Instance.StartMasterBattle(this);
    }

    public void BattleLost()
    {
        battleLost = true;
        los.gameObject.SetActive(false);
    }

    public void SetLosRotation(FacingDirection dir)
    {
        float angle = 0f;

        if (dir == FacingDirection.Right)
        {
            angle = 90f;
        }
        else if (dir == FacingDirection.Up)
        {
            angle = 180f;
        }
        else if (dir == FacingDirection.Left)
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
