using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class DynamicMenuState : State<GameController>
{
    [SerializeField] private DynamicMenuUI dynamicMenuUI;
    [SerializeField] private TextSlot itemTextPrefab;
    private GameController gameController;

    public List<string> MenuItems { get; set; }
    public int? SelectedItem { get; private set; }
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
        gameController = owner;

        foreach (Transform child in dynamicMenuUI.transform)
        {
            Destroy(child.gameObject);
        }

        List<TextSlot> itemTextSlots = new List<TextSlot>();

        foreach (string item in MenuItems)
        {
            TextSlot itemTextSlot = Instantiate(itemTextPrefab, dynamicMenuUI.transform);

            itemTextSlot.SetText(item);
            itemTextSlots.Add(itemTextSlot);
        }

        dynamicMenuUI.SetItems(itemTextSlots);
        dynamicMenuUI.gameObject.SetActive(true);
        dynamicMenuUI.OnSelected += OnItemSelected;
        dynamicMenuUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        dynamicMenuUI.HandleUpdate();
    }

    public override void Exit()
    {
        dynamicMenuUI.ClearItems();
        dynamicMenuUI.gameObject.SetActive(false);
        dynamicMenuUI.OnSelected -= OnItemSelected;
        dynamicMenuUI.OnBack -= OnBack;
    }

    private void OnItemSelected(int selection)
    {
        SelectedItem = selection;
        gameController.StateMachine.Pop();
    }

    private void OnBack()
    {
        SelectedItem = null;
        gameController.StateMachine.Pop();
    }
}
