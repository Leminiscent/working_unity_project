using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.GenericSelectionUI;

public class AnswerSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] List<TextSlot> answerTexts;

    private void Start()
    {
        SetSelectionSettings(SelectionType.Grid, 2);
    }

    public void SetAnswers(List<RecruitmentAnswer> answers)
    {
        selectedItem = 0;
        SetItems(answerTexts.Take(answers.Count).ToList());

        for (int i = 0; i < answerTexts.Count; ++i)
        {
            if (i < answers.Count)
            {
                answerTexts[i].SetText(answers[i].AnswerText);
                answerTexts[i].gameObject.SetActive(true);
            }
            else
            {
                answerTexts[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetAcceptRejectOptions()
    {
        selectedItem = 0;
        var options = new List<string> { "Yes", "No" };
        SetItems(answerTexts.Take(options.Count).ToList());

        for (int i = 0; i < answerTexts.Count; ++i)
        {
            if (i < options.Count)
            {
                answerTexts[i].SetText(options[i]);
                answerTexts[i].gameObject.SetActive(true);
            }
            else
            {
                answerTexts[i].gameObject.SetActive(false);
            }
        }
    }
}
