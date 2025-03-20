using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils.GenericSelectionUI
{
    public class SelectionUI<T> : MonoBehaviour where T : ISelectableItem
    {
        private List<T> _items;
        private SelectionType _selectionType;
        private int _gridWidth = 2;

        protected const float SELECTION_SPEED = 5f;

        protected float _selectionTimer = 0;
        protected int _selectedItem = 0;
        protected int? _savedSelection;

        public event Action<int> OnSelected;
        public event Action OnBack;
        public event Action<int> OnIndexChanged;

        public bool IgnoreVerticalInput { get; set; }
        public bool IgnoreHorizontalInput { get; set; }
        public int SelectedIndex => _selectedItem;

        public void SetSelectionSettings(SelectionType selectionType, int gridWidth)
        {
            _selectionType = selectionType;
            _gridWidth = gridWidth;
        }

        public void SetItems(List<T> items)
        {
            _items = items;
            _items.ForEach(static i => i.Init());
            UpdateSelectionInUI();
        }

        public void ClearItems()
        {
            _items?.ForEach(static i => i.Clear());
            _items = null;
        }

        public virtual void HandleUpdate()
        {
            if (_items == null || _items.Count == 0)
            {
                return;
            }

            UpdateSelectionTimer();

            int prevSelection = _selectedItem;

            if (_selectionType == SelectionType.List)
            {
                HandleListSelection();
            }
            else if (_selectionType == SelectionType.Grid)
            {
                HandleGridSelection();
            }

            if (_selectedItem != prevSelection)
            {
                UpdateSelectionInUI();
                OnIndexChanged?.Invoke(_selectedItem);
                PlayShiftAudio(prevSelection, _selectedItem);
            }
            if (Input.GetButtonDown("Action"))
            {
                OnSelected?.Invoke(_selectedItem);
            }
            if (Input.GetButtonDown("Back"))
            {
                OnBack?.Invoke();
            }
        }

        private void HandleListSelection()
        {
            float v = IgnoreVerticalInput ? 0f : Input.GetAxisRaw("Vertical");

            if (_selectionTimer == 0 && Mathf.Abs(v) > 0.2f)
            {
                _selectedItem += -(int)Mathf.Sign(v);
                if (_selectedItem < 0)
                {
                    _selectedItem = _items.Count - 1;
                }
                else if (_selectedItem >= _items.Count)
                {
                    _selectedItem = 0;
                }

                _selectionTimer = 1 / SELECTION_SPEED;
            }
        }

        private void HandleGridSelection()
        {
            float v = IgnoreVerticalInput ? 0f : Input.GetAxisRaw("Vertical");
            float h = IgnoreHorizontalInput ? 0f : Input.GetAxisRaw("Horizontal");

            if (_selectionTimer == 0 && (Mathf.Abs(v) > 0.2f || Mathf.Abs(h) > 0.2f))
            {
                int oldIndex = _selectedItem;
                int row = oldIndex / _gridWidth;
                int col = oldIndex % _gridWidth;
                int totalRows = Mathf.CeilToInt((float)_items.Count / _gridWidth);
                int lastRow = totalRows - 1;

                _selectedItem = Mathf.Abs(h) > Mathf.Abs(v)
                    ? GetNewGridIndexHorizontal(row, col, h)
                    : GetNewGridIndexVertical(row, col, v, lastRow, totalRows);

                _selectionTimer = 1f / SELECTION_SPEED;
            }
        }

        private int GetNewGridIndexHorizontal(int row, int col, float horizontalInput)
        {
            int rowItemCount = (row == Mathf.CeilToInt((float)_items.Count / _gridWidth) - 1 && _items.Count % _gridWidth != 0)
                ? _items.Count % _gridWidth : _gridWidth;

            int newCol = col + (int)Mathf.Sign(horizontalInput);
            if (newCol < 0)
            {
                newCol = rowItemCount - 1;
            }
            else if (newCol >= rowItemCount)
            {
                newCol = 0;
            }

            return (row * _gridWidth) + newCol;
        }

        private int GetNewGridIndexVertical(int row, int col, float verticalInput, int lastRow, int totalRows)
        {
            int newRow = row - (int)Mathf.Sign(verticalInput);
            if (newRow < 0)
            {
                newRow = lastRow;
            }
            else if (newRow >= totalRows)
            {
                newRow = 0;
            }

            int newRowItemCount = (newRow == lastRow && _items.Count % _gridWidth != 0)
                ? _items.Count % _gridWidth : _gridWidth;
            int newCol = col;
            if (newCol >= newRowItemCount)
            {
                newCol = newRowItemCount - 1;
            }

            return (newRow * _gridWidth) + newCol;
        }

        public virtual void UpdateSelectionInUI()
        {
            if (_items == null || _items.Count == 0)
            {
                return;
            }
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].OnSelectionChanged(i == _selectedItem);
            }
        }

        public void SaveSelection()
        {
            _savedSelection = _selectedItem;
        }

        public void RestoreSelection()
        {
            if (_savedSelection.HasValue)
            {
                _selectedItem = _savedSelection.Value;
                _savedSelection = null;
                UpdateSelectionInUI();
            }
        }

        public virtual void ResetSelection()
        {
            _selectedItem = 0;
            UpdateSelectionInUI();
        }

        public void SetSelectedIndex(int index)
        {
            if (_items != null && _items.Count > 0)
            {
                _selectedItem = Mathf.Clamp(index, 0, _items.Count - 1);
                UpdateSelectionInUI();
            }
        }

        public int GetItemsCount()
        {
            return _items != null ? _items.Count : 0;
        }

        protected void UpdateSelectionTimer()
        {
            if (_selectionTimer > 0)
            {
                _selectionTimer = Mathf.Clamp(_selectionTimer - Time.deltaTime, 0, _selectionTimer);
            }
        }

        protected virtual void PlayShiftAudio(int previousSelection, int newSelection)
        {
            AudioManager.Instance.PlaySFX(AudioID.UIShift);
        }
    }

    public class DummySelectable : ISelectableItem
    {
        public void Init() { }
        public void Clear() { }
        public void OnSelectionChanged(bool selected) { }
    }

    public class DummySelectionUI : SelectionUI<DummySelectable> { }

    public enum SelectionType
    {
        List,
        Grid
    }
}