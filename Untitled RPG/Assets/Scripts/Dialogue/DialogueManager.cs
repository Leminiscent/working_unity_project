using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject _dialogueBox;
    [SerializeField] private ChoiceBox _choiceBox;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private float _lettersPerSecond = 45f;

    private const float ACCELERATION_FACTOR = 100f;
    private const float POST_DIALOGUE_WAIT_TIME = 0.5f;

    public event Action OnShowDialogue;
    public event Action OnDialogueFinished;
    public static DialogueManager Instance { get; private set; }
    public bool IsShowing { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public IEnumerator ShowDialogueText(string text, bool waitForInput = true, float waitTime = POST_DIALOGUE_WAIT_TIME, bool autoClose = true, List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        yield return InitializeDialogue();

        yield return TypeDialogue(text);

        // Wait for input or a preset duration
        yield return waitForInput
            ? new WaitUntil(static () => Input.GetButtonDown("Action") || Input.GetButtonDown("Back"))
            : new WaitForSeconds(waitTime);

        // Handle choice selection if provided
        if (choices != null && choices.Count > 1)
        {
            yield return _choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        if (autoClose)
        {
            CloseDialogue();
        }
        OnDialogueFinished?.Invoke();
    }

    public IEnumerator ShowDialogue(Dialogue dialogue, bool waitForInput = true, float waitTime = POST_DIALOGUE_WAIT_TIME, List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        yield return InitializeDialogue();

        foreach (string line in dialogue.Lines)
        {
            yield return TypeDialogue(line);

            yield return waitForInput
                ? new WaitUntil(static () => Input.GetButtonDown("Action") || Input.GetButtonDown("Back"))
                : new WaitForSeconds(waitTime);
        }

        // Handle choice selection if provided
        if (choices != null && choices.Count > 1)
        {
            yield return _choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        CloseDialogue();
        OnDialogueFinished?.Invoke();
    }

    private IEnumerator InitializeDialogue()
    {
        yield return new WaitForEndOfFrame();
        OnShowDialogue?.Invoke();
        IsShowing = true;
        _dialogueText.text = "";
        _dialogueBox.SetActive(true);
    }

    public void CloseDialogue()
    {
        _dialogueBox.SetActive(false);
        IsShowing = false;
    }

    public IEnumerator TypeDialogue(string line)
    {
        _dialogueText.text = "";
        bool isAccelerated = false;
        float accelerationFactor = ACCELERATION_FACTOR;
        float baseDelay = 1f / _lettersPerSecond;

        foreach (char letter in line)
        {
            _dialogueText.text += letter;
            float delay = isAccelerated ? baseDelay / accelerationFactor : baseDelay;
            float elapsed = 0f;
            while (elapsed < delay)
            {
                // Check for acceleration input
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
    }
}

public static class TextUtil
{
    public static string GetNumText(int num)
    {
        // If the number is less than 10, return the text representation of the number.
        if (num < 10)
        {
            switch (num)
            {
                case 1: return "one";
                case 2: return "two";
                case 3: return "three";
                case 4: return "four";
                case 5: return "five";
                case 6: return "six";
                case 7: return "seven";
                case 8: return "eight";
                case 9: return "nine";
                default:
                    break;
            }
        }
        return num.ToString();
    }

    public static string GetPlural(string noun, int? count = null)
    {
        // If count is provided and is 1, return the singular form of the noun.
        return count.HasValue && count.Value == 1
            ? noun
            : noun switch // Otherwise, return the plural form of the noun.
            {
                string n when n.EndsWith("s") || n.EndsWith("x") || n.EndsWith("ch") ||
                            n.EndsWith("sh") || n.EndsWith("z") => n + "es",
                string n when n.EndsWith("y") && !(n.EndsWith("ay") || n.EndsWith("ey") ||
                                                n.EndsWith("iy") || n.EndsWith("oy") ||
                                                n.EndsWith("uy")) => n[..^1] + "ies",
                _ => noun + "s"
            };
    }

    public static string GetPossessive(string noun)
    {
        // If the noun ends with "s", return the possessive form with just an apostrophe.
        return noun switch
        {
            string n when n.EndsWith("s") => n + "'",
            _ => noun + "'s"
        };
    }

    public static string GetArticle(string noun)
    {
        // Return "an" if the noun starts with a vowel, otherwise return "a".
        return noun switch
        {
            string n when n.StartsWith("a") || n.StartsWith("e") || n.StartsWith("i") ||
                        n.StartsWith("o") || n.StartsWith("u") => "an",
            _ => "a"
        };
    }
}