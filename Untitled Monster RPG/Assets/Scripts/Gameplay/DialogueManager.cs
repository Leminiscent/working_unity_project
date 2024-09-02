using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] GameObject dialogueBox;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] float lettersPerSecond;

    public event Action OnShowDialogue;
    public event Action OnCloseDialogue;

    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public bool IsShowing { get; private set; }

    public IEnumerator ShowDialogueText(string text, bool waitForInput = true, bool autoClose = true)
    {
        IsShowing = true;
        dialogueBox.SetActive(true);
        yield return TypeDialogue(text);
        if (waitForInput)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }

        if (autoClose)
        {
            CloseDialogue();
        }
    }

    public void CloseDialogue()
    {
        dialogueBox.SetActive(false);
        IsShowing = false;
    }

    public IEnumerator ShowDialogue(Dialogue dialogue)
    {
        yield return new WaitForEndOfFrame();

        OnShowDialogue?.Invoke();
        IsShowing = true;
        dialogueBox.SetActive(true);
        foreach (var line in dialogue.Lines)
        {
            yield return TypeDialogue(line);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }
        dialogueBox.SetActive(false);
        IsShowing = false;
        OnCloseDialogue?.Invoke();
    }

    public void HandleUpdate()
    {

    }

    public IEnumerator TypeDialogue(string line)
    {
        dialogueText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
    }
}
