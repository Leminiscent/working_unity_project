using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] private int _lettersPerSecond;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private GameObject _actionSelector;
    [SerializeField] private GameObject _moveSelector;
    [SerializeField] private GameObject _moveDetails;
    [SerializeField] private GameObject _answerSelector;
    [SerializeField] private GameObject _choiceBox;
    [SerializeField] private List<TextMeshProUGUI> _actionTexts;
    [SerializeField] private List<TextMeshProUGUI> _moveTexts;
    [SerializeField] private List<TextMeshProUGUI> _answerTexts;
    [SerializeField] private TextMeshProUGUI _spText;
    [SerializeField] private TextMeshProUGUI _typeText;
    [SerializeField] private TextMeshProUGUI _yesText;
    [SerializeField] private TextMeshProUGUI _noText;

    public bool IsChoiceBoxEnabled => _choiceBox.activeSelf;

    public void SetDialogue(string dialogue)
    {
        _dialogueText.text = dialogue;
    }

    public IEnumerator TypeDialogue(string typeDialogue, bool waitForInput = false, string setDialogue = null, bool clearDialogue = true)
    {
        yield return new WaitForEndOfFrame();

        _dialogueText.text = (setDialogue != null) ? $"{setDialogue} " : "";
        bool isAccelerated = false;
        float accelerationFactor = 100f;
        float baseDelay = 1f / _lettersPerSecond;

        foreach (char letter in typeDialogue.ToCharArray())
        {
            _dialogueText.text += letter;
            float delay = isAccelerated ? baseDelay / accelerationFactor : baseDelay;
            float elapsed = 0f;
            while (elapsed < delay)
            {
                if (!isAccelerated && (Input.GetButtonDown("Action") || Input.GetButtonDown("Back")))
                {
                    isAccelerated = true;
                    delay = baseDelay / accelerationFactor;
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
            yield return new WaitForSeconds(0.75f);
        }

        if (clearDialogue)
        {
            _dialogueText.text = "";
        }
    }

    public void EnableDialogueText(bool enabled)
    {
        _dialogueText.text = "";
        _dialogueText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        _actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        _moveSelector.SetActive(enabled);
        _moveDetails.SetActive(enabled);
    }

    public void EnableAnswerSelector(bool enabled)
    {
        _answerSelector.SetActive(enabled);
    }

    public void EnableChoiceBox(bool enabled)
    {
        _choiceBox.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < _actionTexts.Count; ++i)
        {
            _actionTexts[i].color = i == selectedAction ? GlobalSettings.Instance.ActiveColor : GlobalSettings.Instance.InactiveColor;
        }
    }

    public void UpdateMoveSelction(int selectedMove, Move move)
    {
        for (int i = 0; i < _moveTexts.Count; ++i)
        {
            _moveTexts[i].color = i == selectedMove ? GlobalSettings.Instance.ActiveColor : GlobalSettings.Instance.InactiveColor;
        }

        _spText.text = $"SP {move.Sp}/{move.Base.SP}";
        _typeText.text = move.Base.Type.ToString();
        _spText.color = move.Sp == 0 ? GlobalSettings.Instance.EmptyColor : GlobalSettings.Instance.InactiveColor;
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < _moveTexts.Count; ++i)
        {
            _moveTexts[i].text = i < moves.Count ? moves[i].Base.Name : "-";
        }
    }

    public void UpdateChoiceBox(bool yesSelected)
    {
        if (yesSelected)
        {
            _yesText.color = GlobalSettings.Instance.ActiveColor;
            _noText.color = GlobalSettings.Instance.InactiveColor;
        }
        else
        {
            _yesText.color = GlobalSettings.Instance.InactiveColor;
            _noText.color = GlobalSettings.Instance.ActiveColor;
        }
    }
}
