using UnityEngine;

public class MasterLOS : MonoBehaviour, IPlayerTriggerable
{
    public bool TriggerRepeatedly => false;
    
    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        GameController.Instance.OnEnterMasterView(GetComponentInParent<MasterController>());
    }
}
