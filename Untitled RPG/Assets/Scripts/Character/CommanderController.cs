using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class CommanderController : MonoBehaviour, IInteractable, ISavable
{
    [field: SerializeField, FormerlySerializedAs("_name")] public string Name { get; private set; }
    [SerializeField] private Dialogue _dialogue;
    [SerializeField] private Dialogue _postBattleDialogue;
    [SerializeField] private GameObject _los;

    private bool _battleLost = false;

    public Character Character { get; private set; }

    private void Awake()
    {
        Character = GetComponent<Character>();
    }

    private void Start()
    {
        SetLosRotation(Character.Animator.DefaultDirection);
    }

    private void Update()
    {
        // Delegate the update of animator to the Character component.
        Character.UpdateAnimator();
    }

    public IEnumerator Interact(Transform initiator)
    {
        Character.LookTowards(initiator.position);

        if (!_battleLost)
        {
            AudioManager.Instance.PlaySFX(AudioID.Spotted, pauseMusic: true);
            yield return ShowExclamationIcon(0.5f);
            yield return DialogueManager.Instance.ShowDialogue(_dialogue, false, 0.75f);

            yield return Fader.Instance.FadeIn(0.5f);
            GameController.Instance.StartCommanderBattle(this);
        }
        else
        {
            yield return DialogueManager.Instance.ShowDialogue(_postBattleDialogue);
        }
    }

    public IEnumerator TriggerBattle(PlayerController player)
    {
        // Push cutscene state to prevent further input.
        GameController.Instance.StateMachine.Push(CutsceneState.Instance);

        AudioManager.Instance.PlaySFX(AudioID.Spotted, pauseMusic: true);
        yield return ShowExclamationIcon(0.5f);

        Vector3 moveVector = CalculateMoveVectorTowards(player.transform.position);
        yield return Character.MoveRoutine(moveVector);

        yield return DialogueManager.Instance.ShowDialogue(_dialogue, false, 1f);

        // Pop the cutscene state.
        GameController.Instance.StateMachine.Pop();
        yield return Fader.Instance.FadeIn(0.5f);
        GameController.Instance.StartCommanderBattle(this);
    }

    public void BattleLost()
    {
        _battleLost = true;
        _los.SetActive(false);
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
        _los.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public object CaptureState()
    {
        CommanderSaveData saveData = new()
        {
            Position = new float[] { transform.position.x, transform.position.y },
            FacingDirection = Character.Animator.FacingDirection,
            BattleLost = _battleLost
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        CommanderSaveData saveData = (CommanderSaveData)state;

        transform.position = new Vector3(saveData.Position[0], saveData.Position[1]);
        Character.Animator.FacingDirection = saveData.FacingDirection;
        _battleLost = saveData.BattleLost;
        _los.SetActive(!_battleLost);
    }

    private IEnumerator ShowExclamationIcon(float duration)
    {
        yield return ObjectUtil.ScaleIn(Character.Exclamation);
        yield return new WaitForSeconds(duration);
        yield return ObjectUtil.ScaleOut(Character.Exclamation);
    }

    private Vector3 CalculateMoveVectorTowards(Vector3 targetPosition)
    {
        Vector3 diff = targetPosition - transform.position;
        Vector3 normalizedDiff = diff.normalized;
        Vector3 moveVector = diff - normalizedDiff;
        // Round the values to ensure alignment with grid-based movement
        return new Vector2(Mathf.Round(moveVector.x), Mathf.Round(moveVector.y));
    }
}

[Serializable]
public class CommanderSaveData
{
    public float[] Position;
    public FacingDirection FacingDirection;
    public bool BattleLost;
}