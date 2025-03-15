using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class BattleState : State<GameController>
{
    [SerializeField] private BattleSystem _battleSystem;
    private GameController _gameController;

    public BattleTrigger Trigger { get; set; }
    public CommanderController Commander { get; set; }
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

        if (Commander == null)
        {
            List<Battler> rogueBattlers = _gameController.CurrentScene.GetComponent<MapArea>().GetRandomRogueBattlers(Random.Range(1, 4));
            List<Battler> rogueBattlerCopies = new();

            for (int i = 0; i < rogueBattlers.Count; i++)
            {
                rogueBattlerCopies.Add(new(rogueBattlers[i].Base, rogueBattlers[i].Level));
            }

            _battleSystem.StartRogueBattle(playerParty, rogueBattlerCopies, Trigger, rogueBattlers.Count);
        }
        else
        {
            BattleParty enemyParty = Commander.GetComponent<BattleParty>();
            _battleSystem.StartCommanderBattle(playerParty, enemyParty, Trigger, Mathf.Min(enemyParty.Battlers.Count, 3));
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
            if (Commander != null)
            {
                Commander.BattleLost();
                Commander = null;
            }
            _gameController.StateMachine.Pop();
        }
    }
}
