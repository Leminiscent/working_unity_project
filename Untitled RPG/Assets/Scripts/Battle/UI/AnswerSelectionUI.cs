using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.GenericSelectionUI;

/// <summary>
/// AnswerSelectionUI manages the display of recruitment answer choices within a grid-based selection UI.
/// It extends the generic SelectionUI class specialized for TextSlot components.
/// </summary>
public class AnswerSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] private List<TextSlot> _answerTexts;

    /// <summary>
    /// Initializes the selection settings for the answer UI.
    /// Configures the layout to a grid with 2 columns.
    /// </summary>
    private void Start()
    {
        SetSelectionSettings(SelectionType.Grid, 2);
    }

    /// <summary>
    /// Sets the available answers to be displayed in the UI.
    /// It activates the required answer text slots, sets their text, and deactivates any extra slots.
    /// </summary>
    /// <param name="answers">A list of RecruitmentAnswer objects representing the available answer choices.</param>
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
