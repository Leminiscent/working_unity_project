using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class SummaryState : State<GameController>
{
    [SerializeField] private SummaryScreenUI _summaryScreenUI;

    private int _selectedPage = 0;
    private List<Monster> _playerParty;
    private GameController _gameController;

    public int SelectedMonsterIndex { get; set; }
    public static SummaryState Instance { get; set; }

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
        _playerParty = PlayerController.Instance.GetComponent<MonsterParty>().Monsters;
        _summaryScreenUI.gameObject.SetActive(true);
        _summaryScreenUI.SetBasicDetails(_playerParty[SelectedMonsterIndex]);
        _summaryScreenUI.ShowPage(_selectedPage);
    }

    public override void Execute()
    {
        if (!_summaryScreenUI.InMoveSelection)
        {
            // Page Selection
            int prevPage = _selectedPage;

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _selectedPage = (_selectedPage + 1) % 2;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _selectedPage = Mathf.Abs(_selectedPage - 1) % 2;
            }

            if (prevPage != _selectedPage)
            {
                _summaryScreenUI.ShowPage(_selectedPage);
            }

            // Monster Selection
            int prevIndex = SelectedMonsterIndex;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SelectedMonsterIndex++;

                if (SelectedMonsterIndex >= _playerParty.Count)
                {
                    SelectedMonsterIndex = 0;
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SelectedMonsterIndex--;

                if (SelectedMonsterIndex < 0)
                {
                    SelectedMonsterIndex = _playerParty.Count - 1;
                }
            }

            if (prevIndex != SelectedMonsterIndex)
            {
                _summaryScreenUI.SetBasicDetails(_playerParty[SelectedMonsterIndex]);
                _summaryScreenUI.ShowPage(_selectedPage);
            }
        }

        if (Input.GetButtonDown("Action"))
        {
            if (_selectedPage == 1 && !_summaryScreenUI.InMoveSelection)
            {
                _summaryScreenUI.InMoveSelection = true;
            }
        }
        else if (Input.GetButtonDown("Back"))
        {
            if (_summaryScreenUI.InMoveSelection)
            {
                _summaryScreenUI.InMoveSelection = false;
            }
            else
            {
                _gameController.StateMachine.Pop();
                return;
            }
        }

        _summaryScreenUI.HandleUpdate();
    }

    public override void Exit()
    {
        _summaryScreenUI.gameObject.SetActive(false);
    }
}
