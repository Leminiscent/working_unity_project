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
        // No UI update required; display is managed via _countText and _priceText.
    }

    public override void HandleUpdate()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            ChangeCount(1);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            ChangeCount(-1);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            ChangeCount(10);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            ChangeCount(-10);
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

    private void ChangeCount(int delta)
    {
        if (_selectionTimer > 0f)
        {
            return;
        }

        if (Mathf.Abs(delta) == 1)
        {
            _currentCount = Mod(_currentCount - 1 + delta, _maxCount) + 1;
        }
        else if (Mathf.Abs(delta) == 10)
        {
            _currentCount = _maxCount < 10 ? delta > 0 ? _maxCount : 1 : Mathf.Clamp(_currentCount + delta, 1, _maxCount);
        }

        UpdateDisplay();
        _selectionTimer = 1f / SELECTION_SPEED;
    }

    private int Mod(int a, int m)
    {
        return ((a % m) + m) % m;
    }

    private void UpdateDisplay()
    {
        _countText.text = $"x {_currentCount}";
        _priceText.text = $"{_currentCount * _pricePerUnit} GP";
    }
}
