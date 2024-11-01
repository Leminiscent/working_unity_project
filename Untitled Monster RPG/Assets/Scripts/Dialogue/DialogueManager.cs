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
    [SerializeField] private float _lettersPerSecond;

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

    public IEnumerator ShowDialogueText(string text, bool waitForInput = true, bool autoClose = true, List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        OnShowDialogue?.Invoke();
        IsShowing = true;
        _dialogueBox.SetActive(true);
        AudioManager.Instance.PlaySFX(AudioID.UISelect);
        yield return TypeDialogue(text);
        if (waitForInput)
        {
            yield return new WaitUntil(static () => Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X));
        }

        if (choices != null && choices.Count > 1)
        {
            yield return _choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        if (autoClose)
        {
            CloseDialogue();
        }
        OnDialogueFinished?.Invoke();
    }

    public void CloseDialogue()
    {
        _dialogueBox.SetActive(false);
        IsShowing = false;
    }

    public IEnumerator ShowDialogue(Dialogue dialogue, List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        yield return new WaitForEndOfFrame();

        OnShowDialogue?.Invoke();
        IsShowing = true;
        _dialogueBox.SetActive(true);
        foreach (string line in dialogue.Lines)
        {
            AudioManager.Instance.PlaySFX(AudioID.UISelect);
            yield return TypeDialogue(line);
            yield return new WaitUntil(static () => Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X));
        }

        if (choices != null && choices.Count > 1)
        {
            yield return _choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        _dialogueBox.SetActive(false);
        IsShowing = false;
        OnDialogueFinished?.Invoke();
    }

    public IEnumerator TypeDialogue(string line)
    {
        _dialogueText.text = "";
        foreach (char letter in line.ToCharArray())
        {
            _dialogueText.text += letter;
            yield return new WaitForSeconds(1f / _lettersPerSecond);
        }
    }
}
