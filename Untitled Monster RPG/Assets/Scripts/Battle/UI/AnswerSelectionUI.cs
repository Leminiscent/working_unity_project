using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.GenericSelectionUI;

public class AnswerSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] List<TextSlot> answerTexts;
    List<string> currentOptions;

    private void Start()
    {
        SetSelectionSettings(SelectionType.Grid, 2);
    }

    public void SetAnswers(List<RecruitmentAnswer> answers)
    {
        selectedItem = 0;
        currentOptions = answers.Select(a => a.AnswerText).ToList();
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
        currentOptions = new List<string> { "Yes", "No" };
        SetItems(answerTexts.Take(currentOptions.Count).ToList());

        for (int i = 0; i < answerTexts.Count; ++i)
        {
            if (i < currentOptions.Count)
            {
                answerTexts[i].SetText(currentOptions[i]);
                answerTexts[i].gameObject.SetActive(true);
            }
            else
            {
                answerTexts[i].gameObject.SetActive(false);
            }
        }
    }
}
