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

        MonsterParty playerParty = _gameController.PlayerController.GetComponent<MonsterParty>();

        if (Master == null)
        {
            List<Monster> wildMonsters = _gameController.CurrentScene.GetComponent<MapArea>().GetRandomWildMonsters(Random.Range(1, 4));
            List<Monster> wildMonsterCopies = new();

            for (int i = 0; i < wildMonsters.Count; i++)
            {
                wildMonsterCopies.Add(new(wildMonsters[i].Base, wildMonsters[i].Level));
            }

            _battleSystem.StartWildBattle(playerParty, wildMonsterCopies, Trigger, wildMonsters.Count);
        }
        else
        {
            MonsterParty enemyParty = Master.GetComponent<MonsterParty>();
            _battleSystem.StartMasterBattle(playerParty, enemyParty, Trigger, enemyParty.Monsters.Count);
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
        if (Master != null && won)
        {
            Master.BattleLost();
            Master = null;
        }

        _gameController.StateMachine.Pop();
    }
}
