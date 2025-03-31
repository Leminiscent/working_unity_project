using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.GenericSelectionUI;

public class AnswerSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] private List<TextSlot> _answerTexts;

    private void Start()
    {
        SetSelectionSettings(SelectionType.Grid, 2);
    }

    public void SetAnswers(List<RecruitmentAnswer> answers)
    {
        // Reset selection to the first item.
        _selectedItem = 0;

        // Use only as many text slots as there are answers.
        SetItems(_answerTexts.Take(answers.Count).ToList());

        // Loop through all answer text slots.
        for (int i = 0; i < _answerTexts.Count; ++i)
        {
            if (i < answers.Count)
            {
                // For slots corresponding to an answer, update the text and make sure the GameObject is active.
                _answerTexts[i].SetText(answers[i].AnswerText);
                _answerTexts[i].gameObject.SetActive(true);
            }
            else
            {
                // Deactivate any extra text slots.
                _answerTexts[i].gameObject.SetActive(false);
            }
        }
    }
}
