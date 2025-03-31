using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Util.GenericSelectionUI;

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

        List<TextSlot> items = new();
        if (!TryGetComponent(out TextSlot ts))
        {
            ts = gameObject.AddComponent<TextSlot>();
        }
        items.Add(ts);

        SetSelectionSettings(SelectionType.List, 1);
        SetItems(items);

        yield return ObjectUtil.ScaleIn(gameObject);
        UpdateDisplay();

        yield return new WaitUntil(() => _selected);
        onCountSelected?.Invoke(_currentCount);
        
        yield return ObjectUtil.ScaleOut(gameObject);
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
        // No additional UI update required; display is managed via _countText and _priceText.
    }

    public override void HandleUpdate()
    {
        ProcessCountInput();
        ProcessSelectionInput();
    }

    private void ProcessCountInput()
    {
        if (_selectionTimer > 0f)
        {
            return;
        }

        float verticalInput = Input.GetAxisRaw("Vertical");
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (verticalInput > 0.2f)
        {
            ChangeCount(1);
        }
        else if (verticalInput < -0.2f)
        {
            ChangeCount(-1);
        }
        else if (horizontalInput > 0.2f)
        {
            ChangeCount(10);
        }
        else if (horizontalInput < -0.2f)
        {
            ChangeCount(-10);
        }
    }

    private void ProcessSelectionInput()
    {
        if (Input.GetButtonDown("Action"))
        {
            _selected = true;
            AudioManager.Instance.PlaySFX(AudioID.UISelect);
        }
        else if (Input.GetButtonDown("Back"))
        {
            _currentCount = 0;
            _selected = true;
            AudioManager.Instance.PlaySFX(AudioID.UIReturn);
        }
    }

    private void ChangeCount(int delta)
    {
        if (Mathf.Abs(delta) == 1)
        {
            // Vertical input: wrap using modulo arithmetic.
            _currentCount = Mod(_currentCount - 1 + delta, _maxCount) + 1;
        }
        else if (Mathf.Abs(delta) == 10)
        {
            // Horizontal input: if max is less than 10, jump directly to the end values;
            // otherwise, increment/decrement by 10 with clamping.
            _currentCount = _maxCount < 10 ? delta > 0 ? _maxCount : 1 : Mathf.Clamp(_currentCount + delta, 1, _maxCount);
        }
        UpdateDisplay();
        AudioManager.Instance.PlaySFX(AudioID.UIShift);
        _selectionTimer = 1f / SELECTION_SPEED;
    }

    private static int Mod(int a, int m)
    {
        return ((a % m) + m) % m;
    }

    private void UpdateDisplay()
    {
        _countText.text = $"x {_currentCount}";
        _priceText.text = $"{_currentCount * _pricePerUnit} GP";
    }
}