using UnityEngine;

public class Encounters : MonoBehaviour, IPlayerTriggerable
{
    public bool TriggerRepeatedly => true;
    
    public void OnPlayerTriggered(PlayerController player)
    {
        if (Random.Range(1, 101) <= 10)
        {
            player.Character.Animator.IsMoving = false;
            GameController.Instance.StartWildBattle();
        }
    }
}
