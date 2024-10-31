using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.GenericSelectionUI;

public class AnswerSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] private List<TextSlot> _answerTexts;

    private void Start()
    {
        SetSelectionSettings(SelectionType.Grid, 2);
    }

    public void SetAnswers(List<RecruitmentAnswer> answers)
    {
        selectedItem = 0;
        SetItems(_answerTexts.Take(answers.Count).ToList());

        for (int i = 0; i < _answerTexts.Count; ++i)
        {
            if (i < answers.Count)
            {
                _answerTexts[i].SetText(answers[i].AnswerText);
                _answerTexts[i].gameObject.SetActive(true);
            }
            else
            {
                _answerTexts[i].gameObject.SetActive(false);
            }
        }
    }
}
