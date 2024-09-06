using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterLOS : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        GameController.Instance.OnEnterMasterView(GetComponentInParent<MasterController>());
    }

    public bool TriggerRepeatedly => false;
}
