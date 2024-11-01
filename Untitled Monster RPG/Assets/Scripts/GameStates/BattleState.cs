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
            Monster wildMonster = _gameController.CurrentScene.GetComponent<MapArea>().GetRandomWildMonster();
            Monster wildMonsterCopy = new(wildMonster.Base, wildMonster.Level);

            _battleSystem.StartWildBattle(playerParty, wildMonsterCopy, Trigger);
        }
        else
        {
            MonsterParty enemyParty = Master.GetComponent<MonsterParty>();
            _battleSystem.StartMasterBattle(playerParty, enemyParty, Trigger);
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
