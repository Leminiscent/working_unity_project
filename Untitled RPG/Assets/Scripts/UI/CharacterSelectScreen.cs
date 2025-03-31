using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Util.GenericSelectionUI;

public class CharacterSelectScreen : DummySelectionUI
{
    [Header("Basic Details")]
    [SerializeField] private TextMeshProUGUI _battlerNameText;
    [SerializeField] private Image _battlerImage;
    [SerializeField] private TextMeshProUGUI _battlerType;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _strengthText;
    [SerializeField] private TextMeshProUGUI _enduranceText;
    [SerializeField] private TextMeshProUGUI _intelligenceText;
    [SerializeField] private TextMeshProUGUI _fortitudeText;
    [SerializeField] private TextMeshProUGUI _agilityText;

    [Header("Character Selection")]
    [SerializeField] private GameObject _selectionBar;
    [SerializeField] private GameObject _selectionBarItemPrefab;

    private List<Battler> _availableBattlers;
    private List<GameObject> _selectionBarItems = new();

    public void SetAvailableBattlers(List<Battler> battlers)
    {
        _availableBattlers = battlers;

        List<DummySelectable> dummyItems = new();
        for (int i = 0; i < _availableBattlers.Count; i++)
        {
            dummyItems.Add(new DummySelectable());
        }
        SetItems(dummyItems);
        SetSelectionSettings(SelectionType.Grid, _availableBattlers.Count);
        IgnoreVerticalInput = true;

        // Remove any existing items from the selection bar.
        foreach (Transform child in _selectionBar.transform)
        {
            Destroy(child.gameObject);
        }
        _selectionBarItems.Clear();

        // Instantiate a selection bar item for each battler.
        for (int i = 0; i < _availableBattlers.Count; i++)
        {
            GameObject prefab = Instantiate(_selectionBarItemPrefab, _selectionBar.transform);
            Image[] images = prefab.GetComponentsInChildren<Image>();
            if (images != null && images.Length > 1)
            {
                // Set the sprite of the image to the battler's portrait.
                Image portraitImage = images[1];
                if (_availableBattlers[i].Base.Portrait != null)
                {
                    portraitImage.sprite = _availableBattlers[i].Base.Portrait;
                }
            }
            _selectionBarItems.Add(prefab);
        }

        UpdateDetails();
        UpdateSelectionBar();
    }

    public void UpdateDetails() // TODO: Tween details left and right.
    {
        if (_availableBattlers == null || _availableBattlers.Count == 0)
        {
            Debug.LogWarning("No battlers available for selection.");
            return;
        }

        if (SelectedIndex < 0 || SelectedIndex >= _availableBattlers.Count)
        {
            Debug.LogWarning("SelectedIndex is out of range.");
            return;
        }

        Battler selectedBattler = _availableBattlers[SelectedIndex];
        BattlerBase battlerBase = selectedBattler.Base;

        _battlerNameText.text = battlerBase.Name;
        _battlerImage.sprite = battlerBase.Sprite;
        _battlerType.text = $"{battlerBase.Type1}{(battlerBase.Type2 != BattlerType.None ? $" / {battlerBase.Type2}" : "")}";
        _descriptionText.text = battlerBase.Description;

        _hpText.text = battlerBase.HP.ToString();
        _strengthText.text = battlerBase.Strength.ToString();
        _enduranceText.text = battlerBase.Endurance.ToString();
        _intelligenceText.text = battlerBase.Intelligence.ToString();
        _fortitudeText.text = battlerBase.Fortitude.ToString();
        _agilityText.text = battlerBase.Agility.ToString();
    }

    private void UpdateSelectionBar()
    {
        for (int i = 0; i < _selectionBarItems.Count; i++)
        {
            // Get the border and portrait images of the selection bar item.
            Image[] images = _selectionBarItems[i].GetComponentsInChildren<Image>();
            Image borderImage = (images != null && images.Length > 0) ? images[0] : null;
            Image portraitImage = (images != null && images.Length > 1) ? images[1] : null;

            // Set the color of the border and portrait images based on the selected index.
            Color newColor = (i == SelectedIndex) ? Color.white : Color.grey;
            borderImage.color = newColor;
            if (portraitImage != null)
            {
                portraitImage.color = newColor;
            }
        }
    }

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();
        UpdateDetails();
        UpdateSelectionBar();
    }
}