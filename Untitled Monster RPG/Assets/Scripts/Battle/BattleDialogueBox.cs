using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;
    [SerializeField] Color activeColor;
    [SerializeField] Color inactiveColor;
    [SerializeField] Color noApColor;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject answerSelector;
    [SerializeField] GameObject choiceBox;
    [SerializeField] List<TextMeshProUGUI> actionTexts;
    [SerializeField] List<TextMeshProUGUI> moveTexts;
    [SerializeField] List<TextMeshProUGUI> answerTexts;
    [SerializeField] TextMeshProUGUI apText;
    [SerializeField] TextMeshProUGUI typeText;
    [SerializeField] TextMeshProUGUI yesText;
    [SerializeField] TextMeshProUGUI noText;

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
                actionTexts[i].color = activeColor;
            }
            else
            {
                actionTexts[i].color = inactiveColor;
            }
        }
    }

    public void UpdateMoveSelction(int selectedMove, Move move)
    {
        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i == selectedMove)
            {
                moveTexts[i].color = activeColor;
            }
            else
            {
                moveTexts[i].color = inactiveColor;
            }
        }

        apText.text = $"AP {move.AP}/{move.Base.AP}";
        typeText.text = move.Base.Type.ToString();
        if (move.AP == 0)
        {
            apText.color = noApColor;
        }
        else
        {
            apText.color = Color.white;
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
                answerTexts[i].color = activeColor;
            }
            else
            {
                answerTexts[i].color = inactiveColor;
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
            yesText.color = activeColor;
            noText.color = inactiveColor;
        }
        else
        {
            yesText.color = inactiveColor;
            noText.color = activeColor;
        }
    }
}
