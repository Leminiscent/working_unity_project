using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class SummaryState : State<GameController>
{
    [SerializeField] SummaryScreenUI summaryScreenUI;
    int selectedPage = 0;
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
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;
        playerParty = PlayerController.Instance.GetComponent<MonsterParty>().Monsters;
        summaryScreenUI.gameObject.SetActive(true);
        summaryScreenUI.SetBasicDetails(playerParty[SelectedMonsterIndex]);
        summaryScreenUI.ShowPage(selectedPage);

    }

    public override void Execute()
    {
        if (!summaryScreenUI.InMoveSelection)
        {
            // Page Selection
            int prevPage = selectedPage;

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                selectedPage = (selectedPage + 1) % 2;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selectedPage = Mathf.Abs(selectedPage - 1) % 2;
            }

            if (prevPage != selectedPage)
            {
                summaryScreenUI.ShowPage(selectedPage);
            }

            // Monster Selection
            int prevIndex = SelectedMonsterIndex;

            if (Input.GetKeyDown(KeyCode.DownArrow))
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
                summaryScreenUI.ShowPage(selectedPage);
            }
        }

        if (Input.GetButtonDown("Action"))
        {
            if (selectedPage == 1 && !summaryScreenUI.InMoveSelection)
            {
                summaryScreenUI.InMoveSelection = true;
            }
        }
        else if (Input.GetButtonDown("Back"))
        {
            if (summaryScreenUI.InMoveSelection)
            {
                summaryScreenUI.InMoveSelection = false;
            }
            else
            {
                gameController.StateMachine.Pop();
                return;
            }
        }

        summaryScreenUI.HandleUpdate();
    }

    public override void Exit()
    {
        summaryScreenUI.gameObject.SetActive(false);
    }
}
