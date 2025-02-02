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
        private float _selectionTimer = 0;
        private const float SELCTION_SPEED = 5f;

        protected int _selectedItem = 0;
        protected int? _savedSelection;

        public event Action<int> OnSelected;
        public event Action OnBack;

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

            _selectedItem = Mathf.Clamp(_selectedItem, 0, _items.Count - 1);

            if (_selectedItem != prevSelection)
            {
                UpdateSelectionInUI();
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
            float v = Input.GetAxisRaw("Vertical");

            if (_selectionTimer == 0 && Mathf.Abs(v) > 0.2f)
            {
                _selectedItem += -(int)Mathf.Sign(v);
                _selectionTimer = 1 / SELCTION_SPEED;
            }
        }

        private void HandleGridSelection()
        {
            float v = Input.GetAxisRaw("Vertical");
            float h = Input.GetAxisRaw("Horizontal");

            if ((_selectionTimer == 0 && (Mathf.Abs(v) > 0.2f)) || (_selectionTimer == 0 && (Mathf.Abs(h) > 0.2f)))
            {
                if (Mathf.Abs(h) > Mathf.Abs(v))
                {
                    _selectedItem += (int)Mathf.Sign(h);
                }
                else
                {
                    _selectedItem += -(int)Mathf.Sign(v) * _gridWidth;
                }
                _selectionTimer = 1 / SELCTION_SPEED;
            }
        }

        public virtual void UpdateSelectionInUI()
        {
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

        public void ResetSelection()
        {
            _selectedItem = 0;
            UpdateSelectionInUI();
        }

        private void UpdateSelectionTimer()
        {
            if (_selectionTimer > 0)
            {
                _selectionTimer = Mathf.Clamp(_selectionTimer - Time.deltaTime, 0, _selectionTimer);
            }
        }
    }

    public enum SelectionType
    {
        List,
        Grid
    }
}