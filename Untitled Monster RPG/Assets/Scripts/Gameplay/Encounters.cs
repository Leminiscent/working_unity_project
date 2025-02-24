using System.Collections;
using UnityEngine;

public class Encounters : MonoBehaviour, IPlayerTriggerable
{
    public bool TriggerRepeatedly => true;

    public void OnPlayerTriggered(PlayerController player)
    {
        if (Random.Range(1, 101) <= 10)
        {
            StartCoroutine(TriggerEncounter(player));
        }
    }

    public IEnumerator TriggerEncounter(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        GameController.Instance.StateMachine.Push(CutsceneState.Instance);
        AudioManager.Instance.PlaySFX(AudioID.Spotted, pauseMusic: true);
        player.Character.Exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        player.Character.Exclamation.SetActive(false);
        yield return DialogueManager.Instance.ShowDialogueText("Something's in the brush!");
        GameController.Instance.StateMachine.Pop();
        GameController.Instance.StartWildBattle();
    }
}
