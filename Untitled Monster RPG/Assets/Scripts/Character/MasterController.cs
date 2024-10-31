using System;
using System.Collections;
using UnityEngine;

public class MasterController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] private new string name;
    [SerializeField] private Sprite sprite;
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private Dialogue postBattleDialogue;
    [SerializeField] private GameObject exclamation;
    [SerializeField] private GameObject los;
    [SerializeField] private AudioClip playerDetectedClip;
    private bool battleLost = false;
    private Character character;

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
        GameController.Instance.StateMachine.Push(CutsceneState.Instance);
        AudioManager.Instance.PlayMusic(playerDetectedClip);
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        Vector3 diff = player.transform.position - transform.position;
        Vector3 moveVector = diff - diff.normalized;
        moveVector = new Vector2(Mathf.Round(moveVector.x), Mathf.Round(moveVector.y));

        yield return character.Move(moveVector);
        
        yield return DialogueManager.Instance.ShowDialogue(dialogue);
        GameController.Instance.StateMachine.Pop();
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
        MasterSaveData saveData = new MasterSaveData
        {
            position = new float[] { transform.position.x, transform.position.y },
            facingDirection = character.Animator.FacingDirection,
            battleLost = battleLost
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        MasterSaveData saveData = (MasterSaveData)state;

        transform.position = new Vector3(saveData.position[0], saveData.position[1]);
        character.Animator.FacingDirection = saveData.facingDirection;
        battleLost = saveData.battleLost;
        los.SetActive(!battleLost);
    }

    public string Name => name;
    public Sprite Sprite => sprite;
}

[Serializable]
public class MasterSaveData
{
    public float[] position;
    public FacingDirection facingDirection;
    public bool battleLost;
}