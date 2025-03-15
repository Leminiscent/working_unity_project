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
    [SerializeField] private TextMeshProUGUI _battlerType1;
    [SerializeField] private TextMeshProUGUI _battlerType2;
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
        Battler selectedBattler = _availableBattlers[SelectedIndex];

        _battlerNameText.text = selectedBattler.Base.Name;
        _battlerImage.sprite = selectedBattler.Base.Sprite;
        _battlerType1.text = selectedBattler.Base.Type1.ToString();
        if (selectedBattler.Base.Type2 != BattlerType.None)
        {
            _battlerType2.transform.parent.gameObject.SetActive(true);
            _battlerType2.text = selectedBattler.Base.Type2.ToString();
        }
        else
        {
            _battlerType2.transform.parent.gameObject.SetActive(false);
        }
        _descriptionText.text = selectedBattler.Base.Description;

        _hpText.text = selectedBattler.Base.HP.ToString();
        _strengthText.text = selectedBattler.Base.Strength.ToString();
        _enduranceText.text = selectedBattler.Base.Endurance.ToString();
        _intelligenceText.text = selectedBattler.Base.Intelligence.ToString();
        _fortitudeText.text = selectedBattler.Base.Fortitude.ToString();
        _agilityText.text = selectedBattler.Base.Agility.ToString();
    }

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();
        UpdateDetails();
    }
}