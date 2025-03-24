using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils.GenericSelectionUI;

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

    private List<Battler> _availableBattlers;

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
        UpdateDetails();
    }

    public void UpdateDetails()
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

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();
        UpdateDetails();
    }
}