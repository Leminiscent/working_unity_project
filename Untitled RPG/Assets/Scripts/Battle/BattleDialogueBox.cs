using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] private float _lettersPerSecond = 45f;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private GameObject _choiceBox;
    [SerializeField] private List<TextMeshProUGUI> _moveTexts;
    [SerializeField] private TextMeshProUGUI _yesText;
    [SerializeField] private TextMeshProUGUI _noText;

    private const float ACCELERATED_DELAY = 0.005f;
    private const float POST_DIALOGUE_WAIT_TIME = 0.75f;

    public bool IsChoiceBoxEnabled => _choiceBox != null && _choiceBox.activeSelf;

    public void SetDialogue(string dialogue)
    {
        if (_dialogueText != null)
        {
            _dialogueText.text = dialogue;
        }
    }

    public IEnumerator TypeDialogue(string dialogueToType, bool waitForInput = false, string setDialogue = null, bool clearDialogue = true)
    {
        yield return new WaitForEndOfFrame();

        if (_dialogueText == null)
        {
            yield break;
        }

        string prefix = !string.IsNullOrEmpty(setDialogue) ? $"{setDialogue} " : "";
        yield return TextUtil.TypeText(_dialogueText, dialogueToType, prefix, _lettersPerSecond, ACCELERATED_DELAY);

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

    public void EnableDialogueText(bool enabled)
    {
        if (_dialogueText != null)
        {
            _dialogueText.text = "";
            _dialogueText.enabled = enabled;
        }
    }

    public void EnableChoiceBox(bool enabled)
    {
        if (_choiceBox != null)
        {
            StartCoroutine(ObjectUtil.ScaleInOut(_choiceBox, enabled));
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < _moveTexts.Count; i++)
        {
            _moveTexts[i].text = i < moves.Count ? moves[i].Base.Name : "-";
        }
    }

    public void UpdateChoiceBox(bool yesSelected)
    {
        if (_yesText != null && _noText != null && GlobalSettings.Instance != null)
        {
            _yesText.color = yesSelected ? GlobalSettings.Instance.ActiveColor : GlobalSettings.Instance.InactiveColor;
            _noText.color = yesSelected ? GlobalSettings.Instance.InactiveColor : GlobalSettings.Instance.ActiveColor;
        }
    }
}
