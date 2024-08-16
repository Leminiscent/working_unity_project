using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    Dialogue dialogue;
    Action onDialogueFinished;
    int currentLine = 0;
    bool isTyping;

    public bool isShowing { get; private set; }

    public IEnumerator ShowDialogue(Dialogue dialogue, Action onFnished = null)
    {
        yield return new WaitForEndOfFrame();

        OnShowDialogue?.Invoke();
        isShowing = true;
        this.dialogue = dialogue;
        onDialogueFinished = onFnished;
        dialogueBox.SetActive(true);
        StartCoroutine(TypeDialogue(dialogue.Lines[0]));
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !isTyping)
        {
            ++currentLine;
            if (currentLine < dialogue.Lines.Count)
            {
                dialogueText.text = "";
                StartCoroutine(TypeDialogue(dialogue.Lines[currentLine]));
            }
            else
            {
                currentLine = 0;
                isShowing = false;
                dialogueBox.SetActive(false);
                onDialogueFinished?.Invoke();
                OnCloseDialogue?.Invoke();
            }
        }
    }

    public IEnumerator TypeDialogue(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        isTyping = false;
    }
}
