using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject answerSelector;
    [SerializeField] GameObject choiceBox;
    [SerializeField] List<TextMeshProUGUI> actionTexts;
    [SerializeField] List<TextMeshProUGUI> moveTexts;
    [SerializeField] List<TextMeshProUGUI> answerTexts;
    [SerializeField] TextMeshProUGUI spText;
    [SerializeField] TextMeshProUGUI typeText;
    [SerializeField] TextMeshProUGUI yesText;
    [SerializeField] TextMeshProUGUI noText;

    public BattleState? CalledFrom { get; set; }

    public void SetDialogue(string dialogue)
    {
        dialogueText.text = dialogue;
    }

    public IEnumerator TypeDialogue(string dialogue)
    {
        dialogueText.text = "";
        foreach (var letter in dialogue.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogueText(bool enabled)
    {
        dialogueText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void EnableAnswerSelector(bool enabled)
    {
        answerSelector.SetActive(enabled);
    }

    public void EnableChoiceBox(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; ++i)
        {
            if (i == selectedAction)
            {
                actionTexts[i].color = GlobalSettings.Instance.ActiveColor;
            }
            else
            {
                actionTexts[i].color = GlobalSettings.Instance.InactiveColor;
            }
        }
    }

    public void UpdateMoveSelction(int selectedMove, Move move)
    {
        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i == selectedMove)
            {
                moveTexts[i].color = GlobalSettings.Instance.ActiveColor;
            }
            else
            {
                moveTexts[i].color = GlobalSettings.Instance.InactiveColor;
            }
        }

        spText.text = $"SP {move.SP}/{move.Base.SP}";
        typeText.text = move.Base.Type.ToString();
        if (move.SP == 0)
        {
            spText.color = GlobalSettings.Instance.EmptyColor;
        }
        else
        {
            spText.color = GlobalSettings.Instance.InactiveColor;
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                moveTexts[i].text = "-";
            }
        }
    }

    public void UpdateAnswerSelection(int selectedAnswer)
    {
        for (int i = 0; i < answerTexts.Count; ++i)
        {
            if (i == selectedAnswer)
            {
                answerTexts[i].color = GlobalSettings.Instance.ActiveColor;
            }
            else
            {
                answerTexts[i].color = GlobalSettings.Instance.InactiveColor;
            }
        }
    }

    public void SetAnswers(List<RecruitmentAnswer> answers)
    {
        for (int i = 0; i < answerTexts.Count; ++i)
        {
            if (i < answers.Count)
            {
                answerTexts[i].text = answers[i].Answer;
            }
            else
            {
                answerTexts[i].text = "";
            }
        }
    }

    public void UpdateChoiceBox(bool yesSelected)
    {
        if (yesSelected)
        {
            yesText.color = GlobalSettings.Instance.ActiveColor;
            noText.color = GlobalSettings.Instance.InactiveColor;
        }
        else
        {
            yesText.color = GlobalSettings.Instance.InactiveColor;
            noText.color = GlobalSettings.Instance.ActiveColor;
        }
    }
}
