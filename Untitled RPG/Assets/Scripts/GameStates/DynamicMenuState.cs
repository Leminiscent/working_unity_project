using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class DynamicMenuState : State<GameController>
{
    [SerializeField] private DynamicMenuUI _dynamicMenuUI;
    [SerializeField] private TextSlot _itemTextPrefab;

    private GameController _gameController;

    public List<string> MenuItems { get; set; }
    public int? SelectedItem { get; set; }
    public static DynamicMenuState Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public override void Enter(GameController owner)
    {
        _gameController = owner;

        foreach (Transform child in _dynamicMenuUI.transform)
        {
            Destroy(child.gameObject);
        }

        List<TextSlot> itemTextSlots = new();

        foreach (string item in MenuItems)
        {
            TextSlot itemTextSlot = Instantiate(_itemTextPrefab, _dynamicMenuUI.transform);

            itemTextSlot.SetText(item);
            itemTextSlots.Add(itemTextSlot);
        }

        _dynamicMenuUI.SetItems(itemTextSlots);
        _dynamicMenuUI.gameObject.SetActive(true);
        _dynamicMenuUI.OnSelected += OnItemSelected;
        _dynamicMenuUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        _dynamicMenuUI.HandleUpdate();
    }

    public override void Exit()
    {
        _dynamicMenuUI.ClearItems();
        _dynamicMenuUI.gameObject.SetActive(false);
        _dynamicMenuUI.OnSelected -= OnItemSelected;
        _dynamicMenuUI.OnBack -= OnBack;
    }

    private void OnItemSelected(int selection)
    {
        _dynamicMenuUI.ResetSelection();
        SelectedItem = selection;
        AudioManager.Instance.PlaySFX(AudioID.UISelect);
        _gameController.StateMachine.Pop();
    }

    private void OnBack()
    {
        _dynamicMenuUI.ResetSelection();
        SelectedItem = null;
        AudioManager.Instance.PlaySFX(AudioID.UIReturn);
        _gameController.StateMachine.Pop();
    }
}