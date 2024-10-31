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

    public void SetDialogue(string dialogue)
    {
        _dialogueText.text = dialogue;
    }

    public IEnumerator TypeDialogue(string dialogue, bool waitForInput = false)
    {
        _dialogueText.text = "";
        foreach (char letter in dialogue.ToCharArray())
        {
            _dialogueText.text += letter;
            yield return new WaitForSeconds(1f / _lettersPerSecond);
        }

        if (waitForInput)
        {
            yield return new WaitUntil(static () => Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X));
        }
        else
        {
            yield return new WaitForSeconds(0.75f);
        }
    }

    public IEnumerator SetAndTypeDialogue(string setDialogue, string typeDialogue, bool waitForInput = false)
    {
        _dialogueText.text = $"{setDialogue} ";
        foreach (char letter in typeDialogue.ToCharArray())
        {
            _dialogueText.text += letter;
            yield return new WaitForSeconds(1f / _lettersPerSecond);
        }

        if (waitForInput)
        {
            yield return new WaitUntil(static () => Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X));
        }
        else
        {
            yield return new WaitForSeconds(0.75f);
        }
    }

    public void EnableDialogueText(bool enabled)
    {
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

    public bool IsChoiceBoxEnabled => _choiceBox.activeSelf;

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < _actionTexts.Count; ++i)
        {
            if (i == selectedAction)
            {
                _actionTexts[i].color = GlobalSettings.Instance.ActiveColor;
            }
            else
            {
                _actionTexts[i].color = GlobalSettings.Instance.InactiveColor;
            }
        }
    }

    public void UpdateMoveSelction(int selectedMove, Move move)
    {
        for (int i = 0; i < _moveTexts.Count; ++i)
        {
            if (i == selectedMove)
            {
                _moveTexts[i].color = GlobalSettings.Instance.ActiveColor;
            }
            else
            {
                _moveTexts[i].color = GlobalSettings.Instance.InactiveColor;
            }
        }

        _spText.text = $"SP {move.SP}/{move.Base.SP}";
        _typeText.text = move.Base.Type.ToString();
        if (move.SP == 0)
        {
            _spText.color = GlobalSettings.Instance.EmptyColor;
        }
        else
        {
            _spText.color = GlobalSettings.Instance.InactiveColor;
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < _moveTexts.Count; ++i)
        {
            if (i < moves.Count)
            {
                _moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                _moveTexts[i].text = "-";
            }
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
