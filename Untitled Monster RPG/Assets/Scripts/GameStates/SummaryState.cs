using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class SummaryState : State<GameController>
{
    [SerializeField] SummaryScreenUI summaryScreenUI;
    List<Monster> playerParty;
    GameController gameController;

    public int SelectedMonsterIndex { get; set; }
    public static SummaryState Instance { get; set; }
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        playerParty = PlayerController.Instance.GetComponent<MonsterParty>().Monsters;
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;
        summaryScreenUI.gameObject.SetActive(true);
        summaryScreenUI.SetBasicDetails(playerParty[SelectedMonsterIndex]);
        summaryScreenUI.SetStatsAndExp();

    }

    public override void Execute()
    {
        int prevIndex = SelectedMonsterIndex;

        if (Input.GetButtonDown("Back"))
        {
            gameController.StateMachine.Pop();
            return;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SelectedMonsterIndex++;

            if (SelectedMonsterIndex >= playerParty.Count)
            {
                SelectedMonsterIndex = 0;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SelectedMonsterIndex--;

            if (SelectedMonsterIndex < 0)
            {
                SelectedMonsterIndex = playerParty.Count - 1;
            }
        }

        if (prevIndex != SelectedMonsterIndex)
        {
            summaryScreenUI.SetBasicDetails(playerParty[SelectedMonsterIndex]);
            summaryScreenUI.SetStatsAndExp();
        }
    }

    public override void Exit()
    {
        summaryScreenUI.gameObject.SetActive(false);
    }
}
