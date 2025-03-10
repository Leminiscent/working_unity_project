using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils.GenericSelectionUI;

public class CharacterSelectScreen : DummySelectionUI
{
    [Header("Basic Details")]
    [SerializeField] private TextMeshProUGUI _monsterNameText;
    [SerializeField] private Image _monsterImage;
    [SerializeField] private TextMeshProUGUI _monsterType1;
    [SerializeField] private TextMeshProUGUI _monsterType2;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _strengthText;
    [SerializeField] private TextMeshProUGUI _enduranceText;
    [SerializeField] private TextMeshProUGUI _intelligenceText;
    [SerializeField] private TextMeshProUGUI _fortitudeText;
    [SerializeField] private TextMeshProUGUI _agilityText;

    private List<Monster> _availableMonsters;

    public void SetAvailableMonsters(List<Monster> monsters)
    {
        _availableMonsters = monsters;

        List<DummySelectable> dummyItems = new();
        for (int i = 0; i < _availableMonsters.Count; i++)
        {
            dummyItems.Add(new DummySelectable());
        }
        SetItems(dummyItems);
        SetSelectionSettings(SelectionType.Grid, _availableMonsters.Count);
        IgnoreVerticalInput = true;
        UpdateDetails();
    }

    public void UpdateDetails()
    {
        Monster selectedMonster = _availableMonsters[SelectedIndex];

        _monsterNameText.text = selectedMonster.Base.Name;
        _monsterImage.sprite = selectedMonster.Base.Sprite;
        _monsterType1.text = selectedMonster.Base.Type1.ToString();
        if (selectedMonster.Base.Type2 != MonsterType.None)
        {
            _monsterType2.gameObject.SetActive(true);
            _monsterType2.text = selectedMonster.Base.Type2.ToString();
        }
        else
        {
            _monsterType2.gameObject.SetActive(false);
        }
        _descriptionText.text = selectedMonster.Base.Description;

        _hpText.text = selectedMonster.Hp.ToString();
        _strengthText.text = selectedMonster.Strength.ToString();
        _enduranceText.text = selectedMonster.Endurance.ToString();
        _intelligenceText.text = selectedMonster.Intelligence.ToString();
        _fortitudeText.text = selectedMonster.Fortitude.ToString();
        _agilityText.text = selectedMonster.Agility.ToString();
    }

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();
        UpdateDetails();
    }
}