using System.Collections.Generic;
using UnityEngine;
using Util.StateMachine;

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

        // Clear existing UI elements from the menu container.
        ClearMenuUI();

        // Build the menu items from the provided list.
        BuildMenuUI();

        _ = StartCoroutine(ObjectUtil.ScaleIn(_dynamicMenuUI.gameObject));
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
        _ = StartCoroutine(ObjectUtil.ScaleOut(_dynamicMenuUI.gameObject));
        _dynamicMenuUI.OnSelected -= OnItemSelected;
        _dynamicMenuUI.OnBack -= OnBack;
    }

    private void ClearMenuUI()
    {
        foreach (Transform child in _dynamicMenuUI.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void BuildMenuUI()
    {
        if (MenuItems == null)
        {
            Debug.LogWarning("MenuItems is null. No menu items to display.");
            return;
        }

        List<TextSlot> itemTextSlots = new();

        foreach (string item in MenuItems)
        {
            TextSlot itemTextSlot = Instantiate(_itemTextPrefab, _dynamicMenuUI.transform);
            itemTextSlot.SetText(item);
            itemTextSlots.Add(itemTextSlot);
        }

        _dynamicMenuUI.SetItems(itemTextSlots);
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