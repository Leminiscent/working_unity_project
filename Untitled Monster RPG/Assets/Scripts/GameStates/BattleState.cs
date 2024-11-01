using UnityEngine;
using Utils.StateMachine;

public class BattleState : State<GameController>
{
    [SerializeField] private BattleSystem battleSystem;
    private GameController gameController;

    public BattleTrigger Trigger { get; set; }
    public MasterController Master { get; set; }

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
        gameController = owner;

        battleSystem.gameObject.SetActive(true);
        gameController.WorldCamera.gameObject.SetActive(false);

        MonsterParty playerParty = gameController.PlayerController.GetComponent<MonsterParty>();

        if (Master == null)
        {
            Monster wildMonster = gameController.CurrentScene.GetComponent<MapArea>().GetRandomWildMonster();
            Monster wildMonsterCopy = new Monster(wildMonster.Base, wildMonster.Level);

            battleSystem.StartWildBattle(playerParty, wildMonsterCopy, Trigger);
        }
        else
        {
            MonsterParty enemyParty = Master.GetComponent<MonsterParty>();
            battleSystem.StartMasterBattle(playerParty, enemyParty, Trigger);
        }

        battleSystem.OnBattleOver += EndBattle;
    }

    public override void Execute()
    {
        battleSystem.HandleUpdate();
    }

    public override void Exit()
    {
        battleSystem.gameObject.SetActive(false);
        gameController.WorldCamera.gameObject.SetActive(true);
        battleSystem.OnBattleOver -= EndBattle;
    }

    private void EndBattle(bool won)
    {
        if (Master != null && won == true)
        {
            Master.BattleLost();
            Master = null;
        }

        gameController.StateMachine.Pop();
    }

    public BattleSystem BattleSystem => battleSystem;
}
