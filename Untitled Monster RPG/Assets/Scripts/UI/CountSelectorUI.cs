using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CountSelectorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI priceText;
    private bool selected;
    private int currentCount;
    private int maxCount;
    private float pricePerUnit;

    public IEnumerator ShowSelector(int maxCount, float pricePerUnit, Action<int> onCountSelected)
    {
        this.maxCount = maxCount;
        this.pricePerUnit = pricePerUnit;

        selected = false;
        currentCount = 1;
        gameObject.SetActive(true);
        SetValues();
        yield return new WaitUntil(() => selected);
        onCountSelected?.Invoke(currentCount);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        int prevCount = currentCount;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ++currentCount;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            --currentCount;
        }
        currentCount = Mathf.Clamp(currentCount, 1, maxCount);
        if (currentCount != prevCount)
        {
            SetValues();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            selected = true;
        }
    }

    private void SetValues()
    {
        countText.text = $"x {currentCount}";
        priceText.text = $"{currentCount * pricePerUnit} GP";
    }
}
