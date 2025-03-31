using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject _dialogueBox;
    [SerializeField] private ChoiceBox _choiceBox;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private float _lettersPerSecond = 45f;

    private const float ACCELERATED_DELAY = 0.005f;
    private const float POST_DIALOGUE_WAIT_TIME = 0.5f;

    public event Action OnShowDialogue;
    public event Action OnDialogueFinished;
    public static DialogueManager Instance { get; private set; }
    public bool IsShowing { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public IEnumerator ShowDialogueText(string text, bool waitForInput = true, float waitTime = POST_DIALOGUE_WAIT_TIME, bool autoClose = true, List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        yield return InitializeDialogue();

        yield return TypeDialogue(text);

        // Wait for input or a preset duration
        yield return waitForInput
            ? new WaitUntil(static () => Input.GetButtonDown("Action") || Input.GetButtonDown("Back"))
            : new WaitForSeconds(waitTime);

        // Handle choice selection if provided
        if (choices != null && choices.Count > 1)
        {
            yield return _choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        if (autoClose)
        {
            yield return CloseDialogue();
        }
        OnDialogueFinished?.Invoke();
    }

    public IEnumerator ShowDialogue(Dialogue dialogue, bool waitForInput = true, float waitTime = POST_DIALOGUE_WAIT_TIME, List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        yield return InitializeDialogue();

        foreach (string line in dialogue.Lines)
        {
            yield return TypeDialogue(line);

            yield return waitForInput
                ? new WaitUntil(static () => Input.GetButtonDown("Action") || Input.GetButtonDown("Back"))
                : new WaitForSeconds(waitTime);
        }

        // Handle choice selection if provided
        if (choices != null && choices.Count > 1)
        {
            yield return _choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        yield return CloseDialogue();
        OnDialogueFinished?.Invoke();
    }

    private IEnumerator InitializeDialogue()
    {
        yield return new WaitForEndOfFrame();
        OnShowDialogue?.Invoke();
        IsShowing = true;
        _dialogueText.text = "";
        yield return ObjectUtil.ScaleIn(_dialogueBox);
    }

    public IEnumerator CloseDialogue()
    {
        yield return ObjectUtil.ScaleOut(_dialogueBox);
        IsShowing = false;
    }

    public IEnumerator TypeDialogue(string line)
    {
        yield return TextUtil.TypeText(_dialogueText, line, "", _lettersPerSecond, ACCELERATED_DELAY);
    }
}