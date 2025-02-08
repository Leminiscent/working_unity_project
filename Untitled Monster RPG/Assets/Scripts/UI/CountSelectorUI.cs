using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils.GenericSelectionUI;

public class CountSelectorUI : SelectionUI<TextSlot>
{
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private TextMeshProUGUI _priceText;

    private bool _selected;
    private int _currentCount;
    private int _maxCount;
    private float _pricePerUnit;

    private void Update()
    {
        HandleUpdate();
    }

    private void UpdateDisplay()
    {
        _countText.text = $"x {_currentCount}";
        _priceText.text = $"{_currentCount * _pricePerUnit} GP";
    }

    public IEnumerator ShowSelector(int maxCount, float pricePerUnit, Action<int> onCountSelected)
    {
        _maxCount = maxCount;
        _pricePerUnit = pricePerUnit;
        _selected = false;
        _currentCount = 1;

        List<TextSlot> items = new();
        if (!TryGetComponent(out TextSlot ts))
        {
            ts = gameObject.AddComponent<TextSlot>();
        }
        items.Add(ts);

        SetSelectionSettings(SelectionType.List, 1);
        SetItems(items);

        gameObject.SetActive(true);
        UpdateDisplay();

        yield return new WaitUntil(() => _selected);

        onCountSelected?.Invoke(_currentCount);
        gameObject.SetActive(false);
    }

    public override void UpdateSelectionInUI()
    {
    }

    public override void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _currentCount++;
            _currentCount = Mathf.Clamp(_currentCount, 1, _maxCount);
            UpdateDisplay();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _currentCount--;
            _currentCount = Mathf.Clamp(_currentCount, 1, _maxCount);
            UpdateDisplay();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            _selected = true;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            _currentCount = 0;
            _selected = true;
        }
    }
}