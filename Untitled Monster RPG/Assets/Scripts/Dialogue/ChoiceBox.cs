using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChoiceBox : MonoBehaviour
{
    [SerializeField] ChoiceText choiceTextPrefab;
    bool choiceSelected = false;

    public IEnumerator ShowChoices(List<string> choices)
    {
        choiceSelected = false;
        gameObject.SetActive(true);
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var choice in choices)
        {
            var choiceTextObj = Instantiate(choiceTextPrefab, transform);

            choiceTextObj.TextField.text = choice;
        }
        yield return new WaitUntil(() => choiceSelected);
    }
}
