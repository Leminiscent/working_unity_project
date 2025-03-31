using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public static class ObjectUtil
{
    public static IEnumerator ScaleIn(GameObject target, float duration = 0.1f, float delay = 0f)
    {
        yield return ScaleInOut(target, true, duration, delay);
    }

    public static IEnumerator ScaleOut(GameObject target, float duration = 0.1f, float delay = 0f)
    {
        yield return ScaleInOut(target, false, duration, delay);
    }

    public static IEnumerator ScaleInOut(GameObject target, bool active, float duration = 0.1f, float delay = 0f)
    {
        if (target == null)
        {
            yield break;
        }

        if (active)
        {
            target.transform.localScale = Vector3.zero;
            target.SetActive(true);
            yield return target.transform.DOScale(Vector3.one, duration).SetDelay(delay).SetEase(Ease.Flash).WaitForCompletion();
        }
        else
        {
            yield return target.transform.DOScale(Vector3.zero, duration).SetDelay(delay).SetEase(Ease.Flash).OnComplete(() => target.SetActive(false)).WaitForCompletion();
        }
    }
}

public static class TextUtil
{
    public static IEnumerator TypeText(TextMeshProUGUI target, string textToType, string prefix, float lettersPerSecond, float acceleratedDelay)
    {
        yield return new WaitForEndOfFrame();

        if (target == null)
        {
            yield break;
        }

        target.text = prefix;
        float normalDelay = 1f / lettersPerSecond;
        float currentDelay = normalDelay;
        float accumulatedTime = 0f;
        int letterIndex = 0;
        bool isAccelerated = false;

        while (letterIndex < textToType.Length)
        {
            if (!isAccelerated && (Input.GetButtonDown("Action") || Input.GetButtonDown("Back")))
            {
                isAccelerated = true;
                currentDelay = acceleratedDelay;
            }
            accumulatedTime += Time.deltaTime;
            while (letterIndex < textToType.Length && accumulatedTime >= currentDelay)
            {
                target.text += textToType[letterIndex];
                letterIndex++;
                accumulatedTime -= currentDelay;
            }
            yield return null;
        }
    }

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