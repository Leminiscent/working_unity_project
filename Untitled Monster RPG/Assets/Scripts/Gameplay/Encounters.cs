using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encounters : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        if (Random.Range(1, 101) <= 10)
        {
            player.Character.Animator.IsMoving = false;
            GameController.Instance.StartWildBattle(BattleTrigger.Field);
        }
    }

    public bool TriggerRepeatedly => true;
}
