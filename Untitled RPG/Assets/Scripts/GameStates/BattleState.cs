using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Util.StateMachine;

public class BattleState : State<GameController>
{
    [field: SerializeField, FormerlySerializedAs("_battleSystem")] public BattleSystem BattleSystem { get; private set; }

    private GameController _gameController;

    public BattleTrigger Trigger { get; set; }
    public CommanderController Commander { get; set; }
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

        // Activate the battle system and disable the world camera.
        if (BattleSystem != null)
        {
            BattleSystem.gameObject.SetActive(true);
        }
        if (_gameController.WorldCamera != null)
        {
            _gameController.WorldCamera.gameObject.SetActive(false);
        }

        // Retrieve the player's battle party from the PlayerController.
        if (!_gameController.PlayerController.TryGetComponent(out BattleParty playerParty))
        {
            Debug.LogError("PlayerController is missing a BattleParty component.");
            return;
        }

        // Retrieve the MapArea component from the current scene.
        if (!_gameController.CurrentScene.TryGetComponent(out MapArea mapArea))
        {
            Debug.LogError("CurrentScene is missing a MapArea component.");
            return;
        }

        // Start either a rogue battle or a commander battle based on whether Commander is set.
        if (Commander == null)
        {
            // Get a list of random rogue battlers from the current scene.
            List<Battler> rogueBattlers = mapArea.GetRandomRogueBattlers(Random.Range(1, 4));
            List<Battler> rogueBattlerCopies = new();

            // Create copies of the rogue battlers.
            foreach (Battler battler in rogueBattlers)
            {
                rogueBattlerCopies.Add(new Battler(battler.Base, battler.Level));
            }

            BattleSystem.StartRogueBattle(playerParty, rogueBattlerCopies, Trigger, mapArea.Weather, rogueBattlers.Count);
        }
        else
        {
            // Get the enemy party from the commander.
            if (!Commander.TryGetComponent(out BattleParty enemyParty))
            {
                Debug.LogError("Commander is missing a BattleParty component.");
                return;
            }
            int maxBattlers = Mathf.Min(enemyParty.Battlers.Count, 3);
            BattleSystem.StartCommanderBattle(playerParty, enemyParty, Trigger, mapArea.Weather, maxBattlers);
        }

        // Subscribe to the battle over event.
        BattleSystem.OnBattleOver += EndBattle;
    }

    public override void Execute()
    {
        BattleSystem.HandleUpdate();
    }

    public override void Exit()
    {
        _ = StartCoroutine(ExitBattleState());
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

    private IEnumerator ExitBattleState()
    {
        if (BattleSystem != null)
        {
            BattleSystem.gameObject.SetActive(false);
        }
        if (_gameController.WorldCamera != null)
        {
            _gameController.WorldCamera.gameObject.SetActive(true);
        }
        BattleSystem.OnBattleOver -= EndBattle;

        yield return Fader.Instance.FadeOut(0.5f);
    }
}
