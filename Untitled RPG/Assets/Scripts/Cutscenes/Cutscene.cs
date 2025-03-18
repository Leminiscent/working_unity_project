using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Cutscene : MonoBehaviour, IPlayerTriggerable
{
    [SerializeReference, SerializeField] private List<CutsceneAction> _actions;

    public bool TriggerRepeatedly => false;

    public IEnumerator Play()
    {
        if (GameController.Instance == null || GameController.Instance.StateMachine == null)
        {
            Debug.LogWarning("GameController or its StateMachine instance is null. Aborting cutscene play.");
            yield break;
        }

        GameController.Instance.StateMachine.Push(CutsceneState.Instance);
        try
        {
            foreach (CutsceneAction action in _actions)
            {
                if (action.WaitForCompletion)
                {
                    yield return action.Play();
                }
                else
                {
                    _ = StartCoroutine(action.Play());
                }
            }
        }
        finally
        {
            GameController.Instance.StateMachine.Pop();
        }
    }

    public void AddAction(CutsceneAction action)
    {
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(this, "Added Cutscene Action");
#endif
        action.ActionName = action.GetType().Name;
        _actions.Add(action);
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        if (player == null || player.Character == null || player.Character.Animator == null)
        {
            Debug.LogWarning("Player or its associated components are missing; cannot trigger cutscene.");
            return;
        }
        player.Character.Animator.IsMoving = false;
        _ = StartCoroutine(Play());
    }
}