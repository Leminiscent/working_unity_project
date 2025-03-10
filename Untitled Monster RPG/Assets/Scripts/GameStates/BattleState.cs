using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class BattleState : State<GameController>
{
    [SerializeField] private BattleSystem _battleSystem;
    private GameController _gameController;

    public BattleTrigger Trigger { get; set; }
    public MasterController Master { get; set; }
    public BattleSystem BattleSystem => _battleSystem;
    public static BattleState Instance { get; private set; }

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

        _battleSystem.gameObject.SetActive(true);
        _gameController.WorldCamera.gameObject.SetActive(false);

        BattleParty playerParty = _gameController.PlayerController.GetComponent<BattleParty>();

        if (Master == null)
        {
            List<Battler> wildBattlers = _gameController.CurrentScene.GetComponent<MapArea>().GetRandomWildBattlers(Random.Range(1, 4));
            List<Battler> wildBattlerCopies = new();

            for (int i = 0; i < wildBattlers.Count; i++)
            {
                wildBattlerCopies.Add(new(wildBattlers[i].Base, wildBattlers[i].Level));
            }

            _battleSystem.StartWildBattle(playerParty, wildBattlerCopies, Trigger, wildBattlers.Count);
        }
        else
        {
            BattleParty enemyParty = Master.GetComponent<BattleParty>();
            _battleSystem.StartMasterBattle(playerParty, enemyParty, Trigger, Mathf.Min(enemyParty.Battlers.Count, 3));
        }

        _battleSystem.OnBattleOver += EndBattle;
    }

    public override void Execute()
    {
        _battleSystem.HandleUpdate();
    }

    public override void Exit()
    {
        _battleSystem.gameObject.SetActive(false);
        _gameController.WorldCamera.gameObject.SetActive(true);
        _battleSystem.OnBattleOver -= EndBattle;
    }

    private void EndBattle(bool won)
    {
        if (won)
        {
            if (Master != null)
            {
                Master.BattleLost();
                Master = null;
            }
            _gameController.StateMachine.Pop();
        }
    }
}
