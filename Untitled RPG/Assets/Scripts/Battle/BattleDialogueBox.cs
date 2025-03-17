using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles the dialogue box UI during battles.
/// </summary>
public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] private int _lettersPerSecond = 45;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private GameObject _choiceBox;
    [SerializeField] private List<TextMeshProUGUI> _moveTexts;
    [SerializeField] private TextMeshProUGUI _yesText;
    [SerializeField] private TextMeshProUGUI _noText;

    private const float ACCELERATION_FACTOR = 100f;
    private const float POST_DIALOGUE_WAIT_TIME = 0.75f;

    public bool IsChoiceBoxEnabled => _choiceBox != null && _choiceBox.activeSelf;

    /// <summary>
    /// Sets the dialogue text instantly.
    /// </summary>
    /// <param name="dialogue">The dialogue string to display.</param>
    public void SetDialogue(string dialogue)
    {
        if (_dialogueText != null)
        {
            _dialogueText.text = dialogue;
        }
    }

    /// <summary>
    /// Types out the dialogue letter-by-letter.
    /// </summary>
    /// <param name="dialogueToType">The dialogue to type out.</param>
    /// <param name="waitForInput">If true, waits for user input before finishing.</param>
    /// <param name="setDialogue">An optional initial dialogue string to set.</param>
    /// <param name="clearDialogue">If true, clears the dialogue after finishing.</param>
    /// <returns>A coroutine enumerator.</returns>
    public IEnumerator TypeDialogue(string dialogueToType, bool waitForInput = false, string setDialogue = null, bool clearDialogue = true)
    {
        // Wait for the end of the frame to ensure UI updates are ready.
        yield return new WaitForEndOfFrame();

        if (_dialogueText == null)
        {
            yield break;
        }

        _dialogueText.text = !string.IsNullOrEmpty(setDialogue) ? $"{setDialogue} " : "";

        // Ensure _lettersPerSecond is positive to avoid division errors.
        float effectiveLettersPerSecond = (_lettersPerSecond > 0) ? _lettersPerSecond : 1;
        float baseDelay = 1f / effectiveLettersPerSecond;
        bool isAccelerated = false;

        foreach (char letter in dialogueToType)
        {
            _dialogueText.text += letter;
            float delay = isAccelerated ? baseDelay / ACCELERATION_FACTOR : baseDelay;
            float elapsed = 0f;

            while (elapsed < delay)
            {
                if (!isAccelerated && (Input.GetButtonDown("Action") || Input.GetButtonDown("Back")))
                {
                    isAccelerated = true;
                    delay = baseDelay / ACCELERATION_FACTOR;
                    if (elapsed >= delay)
                    {
                        break;
                    }
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        if (waitForInput)
        {
            yield return new WaitUntil(static () => Input.GetButtonDown("Action") || Input.GetButtonDown("Back"));
            yield return new WaitUntil(static () => !Input.GetButton("Action") && !Input.GetButton("Back"));
        }
        else
        {
            yield return new WaitForSeconds(POST_DIALOGUE_WAIT_TIME);
        }

        if (clearDialogue)
        {
            _dialogueText.text = "";
        }
    }

    /// <summary>
    /// Enables or disables the dialogue text.
    /// </summary>
    /// <param name="enabled">If true, enables the dialogue text; otherwise, disables it.</param>
    public void EnableDialogueText(bool enabled)
    {
        if (_dialogueText != null)
        {
            _dialogueText.text = "";
            _dialogueText.enabled = enabled;
        }
    }

    /// <summary>
    /// Activates or deactivates the choice box.
    /// </summary>
    /// <param name="enabled">If true, activates the choice box; otherwise, deactivates it.</param>
    public void EnableChoiceBox(bool enabled)
    {
        if (_choiceBox != null)
        {
            _choiceBox.SetActive(enabled);
        }
    }

    /// <summary>
    /// Sets the move names in the move texts UI elements.
    /// </summary>
    /// <param name="moves">A list of moves to display.</param>
    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < _moveTexts.Count; i++)
        {
            _moveTexts[i].text = i < moves.Count ? moves[i].Base.Name : "-";
        }
    }

    /// <summary>
    /// Updates the choice box UI to indicate which option is selected.
    /// </summary>
    /// <param name="yesSelected">If true, highlights the 'Yes' option; otherwise, highlights 'No'.</param>
    public void UpdateChoiceBox(bool yesSelected)
    {
        if (_yesText != null && _noText != null && GlobalSettings.Instance != null)
        {
            _yesText.color = yesSelected ? GlobalSettings.Instance.ActiveColor : GlobalSettings.Instance.InactiveColor;
            _noText.color = yesSelected ? GlobalSettings.Instance.InactiveColor : GlobalSettings.Instance.ActiveColor;
        }
    }
}
