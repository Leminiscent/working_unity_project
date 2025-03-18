using UnityEngine;

public class CommanderLOS : MonoBehaviour, IPlayerTriggerable
{
    private CommanderController _commanderController;

    public bool TriggerRepeatedly => false;

    private void Awake()
    {
        _commanderController = GetComponentInParent<CommanderController>();
        if (_commanderController == null)
        {
            Debug.LogError("CommanderLOS: No CommanderController found in parent hierarchy.");
        }
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        if (player != null ? player.Character : null != null)
        {
            player.Character.Animator.IsMoving = false;
        }

        if (_commanderController != null)
        {
            GameController.Instance.OnEnterCommanderView(_commanderController);
        }
    }
}