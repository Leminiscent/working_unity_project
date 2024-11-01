using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CountSelectorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private TextMeshProUGUI _priceText;

    private bool _selected;
    private int _currentCount;
    private int _maxCount;
    private float _pricePerUnit;

    public IEnumerator ShowSelector(int maxCount, float pricePerUnit, Action<int> onCountSelected)
    {
        _maxCount = maxCount;
        _pricePerUnit = pricePerUnit;

        _selected = false;
        _currentCount = 1;
        gameObject.SetActive(true);
        SetValues();
        yield return new WaitUntil(() => _selected);
        onCountSelected?.Invoke(_currentCount);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        int prevCount = _currentCount;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ++_currentCount;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            --_currentCount;
        }
        _currentCount = Mathf.Clamp(_currentCount, 1, _maxCount);
        if (_currentCount != prevCount)
        {
            SetValues();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            _selected = true;
        }
    }

    private void SetValues()
    {
        _countText.text = $"x {_currentCount}";
        _priceText.text = $"{_currentCount * _pricePerUnit} GP";
    }
}
