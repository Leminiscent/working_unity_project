using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class BattleState : State<GameController>
{
    [SerializeField] BattleSystem battleSystem;
    GameController gameController;

    public BattleTrigger trigger { get; set; }
    public MasterController master { get; set; }

    public static BattleState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;

        battleSystem.gameObject.SetActive(true);
        gameController.WorldCamera.gameObject.SetActive(false);

        var playerParty = gameController.PlayerController.GetComponent<MonsterParty>();

        if (master == null)
        {
            var wildMonster = gameController.CurrentScene.GetComponent<MapArea>().GetRandomWildMonster(trigger);
            var wildMonsterCopy = new Monster(wildMonster.Base, wildMonster.Level);

            battleSystem.StartWildBattle(playerParty, wildMonsterCopy, trigger);
        }
        else
        {
            var enemyParty = master.GetComponent<MonsterParty>();
            battleSystem.StartMasterBattle(playerParty, enemyParty);
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

    void EndBattle(bool won)
    {
        if (master != null && won == true)
        {
            master.BattleLost();
            master = null;
        }

        gameController.StateMachine.Pop();
    }

    public BattleSystem BattleSystem => battleSystem;
}
