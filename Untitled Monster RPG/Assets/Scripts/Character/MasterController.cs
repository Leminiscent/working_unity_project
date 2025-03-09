using System;
using System.Collections;
using UnityEngine;

public class MasterController : MonoBehaviour, IInteractable, ISavable
{
    [SerializeField] private string _name;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private Dialogue _dialogue;
    [SerializeField] private Dialogue _postBattleDialogue;
    [SerializeField] private GameObject _los;

    private bool _battleLost = false;
    private Character _character;

    public string Name => _name;
    public Sprite Sprite => _sprite;
    public Character Character => _character;

    private void Awake()
    {
        _character = GetComponent<Character>();
    }

    private void Start()
    {
        SetLosRotation(_character.Animator.DefaultDirection);
    }

    private void Update()
    {
        _character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator)
    {
        _character.LookTowards(initiator.position);
        if (!_battleLost)
        {
            AudioManager.Instance.PlaySFX(AudioID.Spotted, pauseMusic: true);
            _character.Exclamation.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            _character.Exclamation.SetActive(false);
            yield return DialogueManager.Instance.ShowDialogue(_dialogue, false, 0.75f);
            GameController.Instance.StartMasterBattle(this);
        }
        else
        {
            yield return DialogueManager.Instance.ShowDialogue(_postBattleDialogue);
        }
    }

    public IEnumerator TriggerBattle(PlayerController player)
    {
        GameController.Instance.StateMachine.Push(CutsceneState.Instance);
        AudioManager.Instance.PlaySFX(AudioID.Spotted, pauseMusic: true);
        _character.Exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        _character.Exclamation.SetActive(false);

        Vector3 diff = player.transform.position - transform.position;
        Vector3 moveVector = diff - diff.normalized;
        moveVector = new Vector2(Mathf.Round(moveVector.x), Mathf.Round(moveVector.y));

        yield return _character.Move(moveVector);

        yield return DialogueManager.Instance.ShowDialogue(_dialogue, false, 1f);
        GameController.Instance.StateMachine.Pop();
        GameController.Instance.StartMasterBattle(this);
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
        MasterSaveData saveData = new()
        {
            Position = new float[] { transform.position.x, transform.position.y },
            FacingDirection = _character.Animator.FacingDirection,
            BattleLost = _battleLost
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        MasterSaveData saveData = (MasterSaveData)state;

        transform.position = new Vector3(saveData.Position[0], saveData.Position[1]);
        _character.Animator.FacingDirection = saveData.FacingDirection;
        _battleLost = saveData.BattleLost;
        _los.SetActive(!_battleLost);
    }
}

[Serializable]
public class MasterSaveData
{
    public float[] Position;
    public FacingDirection FacingDirection;
    public bool BattleLost;
}