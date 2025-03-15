using UnityEngine;

public class CommanderLOS : MonoBehaviour, IPlayerTriggerable
{
    public bool TriggerRepeatedly => false;

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        GameController.Instance.OnEnterCommanderView(GetComponentInParent<CommanderController>());
    }
}
