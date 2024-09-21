using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils.GenericSelectionUI
{
    public class SelectionUI<T> : MonoBehaviour where T : ISelectableItem
    {
        List<T> items;
        float selectedItem;

        public virtual void HandleUpdate()
        {
            float prevSelection = selectedItem;

            HandleListSelection();
            selectedItem = Mathf.Clamp(selectedItem, 0, items.Count - 1);
            if (selectedItem != prevSelection)
            {
                
            }
        }

        void HandleListSelection()
        {
            float v = Input.GetAxis("Vertical");

            selectedItem += -(int)Mathf.Sign(v);
        }

        void UpdateSelectionInUI()
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].OnSelectionChanged(i == selectedItem);
            }
        }
    }
}