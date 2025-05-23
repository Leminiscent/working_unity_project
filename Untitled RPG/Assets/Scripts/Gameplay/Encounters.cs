using System.Collections;
using UnityEngine;

public class Encounters : MonoBehaviour, IPlayerTriggerable
{
    public bool TriggerRepeatedly => true;

    public void OnPlayerTriggered(PlayerController player)
    {
        if (Random.Range(1, 101) <= 10)
        {
            _ = StartCoroutine(TriggerEncounter(player));
        }
    }

    public IEnumerator TriggerEncounter(PlayerController player)
    {
        if (player == null)
        {
            yield break;
        }

        player.Character.Animator.IsMoving = false;
        GameController.Instance.StateMachine.Push(CutsceneState.Instance);

        AudioManager.Instance.PlaySFX(AudioID.Spotted, pauseMusic: true);
        yield return ObjectUtil.ScaleIn(player.Character.Exclamation);

        yield return new WaitForSeconds(0.5f);

        yield return ObjectUtil.ScaleOut(player.Character.Exclamation);
        yield return DialogueManager.Instance.ShowDialogueText("Something's in the brush!", waitForInput: false, waitTime: 0.75f);

        GameController.Instance.StateMachine.Pop();
        yield return Fader.Instance.FadeIn(0.5f);
        GameController.Instance.StartRogueBattle();
    }
}