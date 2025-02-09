using System;
using System.Collections;
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

    public IEnumerator ShowSelector(int maxCount, float pricePerUnit, Action<int> onCountSelected)
    {
        _maxCount = maxCount;
        _pricePerUnit = pricePerUnit;
        _selected = false;
        _currentCount = 1;
        _selectionTimer = 0f;

        System.Collections.Generic.List<TextSlot> items = new();
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

    private void Update()
    {
        HandleUpdate();

        if (_selectionTimer > 0f)
        {
            _selectionTimer = Mathf.Max(_selectionTimer - Time.deltaTime, 0f);
        }
    }

    public override void UpdateSelectionInUI()
    {
        // Do nothing – This class manages its own display via _countText and _priceText.
    }

    public override void HandleUpdate()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (_selectionTimer <= 0f)
            {
                _currentCount++;
                if (_currentCount > _maxCount)
                {
                    _currentCount = 1;
                }
                UpdateDisplay();
                _selectionTimer = 1f / SELECTION_SPEED;
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            if (_selectionTimer <= 0f)
            {
                _currentCount--;
                if (_currentCount < 1)
                {
                    _currentCount = _maxCount;
                }
                UpdateDisplay();
                _selectionTimer = 1f / SELECTION_SPEED;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (_selectionTimer <= 0f)
            {
                _currentCount = ((_currentCount - 1 + 10) % _maxCount) + 1;
                UpdateDisplay();
                _selectionTimer = 1f / SELECTION_SPEED;
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (_selectionTimer <= 0f)
            {
                _currentCount = (_currentCount - 1 - 10) % _maxCount;
                _currentCount = (_currentCount + _maxCount) % _maxCount;
                _currentCount++;
                UpdateDisplay();
                _selectionTimer = 1f / SELECTION_SPEED;
            }
        }

        if (Input.GetButtonDown("Action"))
        {
            _selected = true;
        }
        else if (Input.GetButtonDown("Back"))
        {
            _currentCount = 0;
            _selected = true;
        }
    }

    private void UpdateDisplay()
    {
        _countText.text = $"x {_currentCount}";
        _priceText.text = $"{_currentCount * _pricePerUnit} GP";
    }
}
