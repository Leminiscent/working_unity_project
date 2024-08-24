using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterLOS : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        GameController.Instance.OnEnterMasterView(GetComponentInParent<MasterController>());
    }
}
