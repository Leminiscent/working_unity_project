using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

/// <summary>
/// Manages the battle system, including battle setup, state transitions, and processing of battle actions.
/// </summary>
public class BattleSystem : MonoBehaviour
{
    [Header("Battle Units - Single")]
    [SerializeField] private BattleUnit _playerUnitSingle;
    [SerializeField] private BattleUnit _enemyUnitSingle;

    [Header("Battle Units - Double")]
    [SerializeField] private List<BattleUnit> _playerUnitsDouble;
    [SerializeField] private List<BattleUnit> _enemyUnitsDouble;

    [Header("Battle Units - Triple")]
    [SerializeField] private List<BattleUnit> _playerUnitsTriple;
    [SerializeField] private List<BattleUnit> _enemyUnitsTriple;

    [Header("UI Elements")]
    [SerializeField] private BattleDialogueBox _dialogueBox;
    [SerializeField] private PartyScreen _partyScreen;
    [SerializeField] private GameObject _playerElementsSingle;
    [SerializeField] private GameObject _playerElementsDouble;
    [SerializeField] private GameObject _playerElementsTriple;
    [SerializeField] private GameObject _enemyElementsSingle;
    [SerializeField] private GameObject _enemyElementsDouble;
    [SerializeField] private GameObject _enemyElementsTriple;

    [Header("Audio")]
    [SerializeField] private AudioClip _rogueBattleMusic;
    [SerializeField] private AudioClip _commanderBattleMusic;
    [SerializeField] private AudioClip _battleWonMusic;
    [SerializeField] private AudioClip _battleLostMusic;
    [SerializeField] private AudioClip _battleFledMusic;

    [Header("Background")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Sprite _desertBackground;
    [SerializeField] private Sprite _desertOasisBackground;
    [SerializeField] private Sprite _fieldBackground;
    [SerializeField] private Sprite _fieldLakeBackground;
    [SerializeField] private Sprite _meadowBackground;
    [SerializeField] private Sprite _mountainBackground;
    [SerializeField] private Sprite _mountainCloudsBackground;
    [SerializeField] private Sprite _wastelandBackground;

    private const float SWITCH_BATTLER_WAIT_DURATION = 0.75f;
    private const float ENEMY_GUARD_CHANCE = 0.2f;

    private BattleTrigger _battleTrigger;
    private Dictionary<BattleTrigger, Sprite> _backgroundMapping;
    private List<BattleUnit> _playerUnits;
    private List<BattleUnit> _enemyUnits;
    private int _playerUnitCount = 1;
    private int _enemyUnitCount = 1;
    private int _selectingUnitIndex = 0;
    private List<BattleAction> _battleActions;

    public StateMachine<BattleSystem> StateMachine { get; private set; }
    public event Action<bool> OnBattleOver;
    public bool BattleIsOver { get; private set; }
    public BattleParty PlayerParty { get; private set; }
    public BattleParty EnemyParty { get; private set; }
    public List<Battler> RogueBattlers { get; private set; }
    public Field Field { get; private set; }
    public bool IsCommanderBattle { get; private set; }
    public int EscapeAttempts { get; set; }
    public CommanderController Enemy { get; private set; }
    public PlayerController Player { get; private set; }
    public BattleDialogueBox DialogueBox => _dialogueBox;
    public List<BattleUnit> PlayerUnits => _playerUnits;
    public List<BattleUnit> EnemyUnits => _enemyUnits;
    public BattleUnit SelectingUnit => PlayerUnits[_selectingUnitIndex];
    public AudioClip BattleWonMusic => _battleWonMusic;
    public AudioClip BattleLostMusic => _battleLostMusic;
    public AudioClip BattleFledMusic => _battleFledMusic;

    private void Awake()
    {
        _backgroundMapping = new Dictionary<BattleTrigger, Sprite>
        {
            { BattleTrigger.Desert, _desertBackground },
            { BattleTrigger.DesertOasis, _desertOasisBackground },
            { BattleTrigger.Field, _fieldBackground },
            { BattleTrigger.FieldLake, _fieldLakeBackground },
            { BattleTrigger.Meadow, _meadowBackground },
            { BattleTrigger.Mountain, _mountainBackground },
            { BattleTrigger.MountainClouds, _mountainCloudsBackground },
            { BattleTrigger.Wasteland, _wastelandBackground }
        };
    }

    /// <summary>
    /// Starts a rogue battle.
    /// </summary>
    /// <param name="playerParty">The player's battle party.</param>
    /// <param name="rogueBattlers">List of rogue battlers.</param>
    /// <param name="trigger">The battle trigger (environment).</param>
    /// <param name="unitCount">The number of enemy units.</param>
    public void StartRogueBattle(BattleParty playerParty, List<Battler> rogueBattlers, BattleTrigger trigger, int unitCount = 1)
    {
        IsCommanderBattle = false;
        PlayerParty = playerParty;
        RogueBattlers = rogueBattlers;
        _enemyUnitCount = unitCount;
        _battleTrigger = trigger;
        AudioManager.Instance.PlayMusic(_rogueBattleMusic);
        _ = StartCoroutine(SetupBattle());
    }

    /// <summary>
    /// Starts a commander battle.
    /// </summary>
    /// <param name="playerParty">The player's battle party.</param>
    /// <param name="enemyParty">The enemy's battle party.</param>
    /// <param name="trigger">The battle trigger (environment).</param>
    /// <param name="unitCount">The number of enemy units.</param>
    public void StartCommanderBattle(BattleParty playerParty, BattleParty enemyParty, BattleTrigger trigger, int unitCount = 1)
    {
        IsCommanderBattle = true;
        PlayerParty = playerParty;
        EnemyParty = enemyParty;
        _enemyUnitCount = unitCount;
        Player = playerParty.GetComponent<PlayerController>();
        Enemy = enemyParty.GetComponent<CommanderController>();
        _battleTrigger = trigger;
        AudioManager.Instance.PlayMusic(_commanderBattleMusic);
        _ = StartCoroutine(SetupBattle());
    }

    /// <summary>
    /// Sets up the battle, initializing parties, UI elements, and background.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator SetupBattle()
    {
        StateMachine = new StateMachine<BattleSystem>(this);
        _battleActions = new List<BattleAction>();

        // Determine number of player units (up to 3 healthy battlers)
        _playerUnitCount = Mathf.Min(PlayerParty.Battlers.Count(static b => b.Hp > 0), 3);

        // Activate proper UI elements based on unit counts
        _playerElementsSingle.SetActive(_playerUnitCount == 1);
        _playerElementsDouble.SetActive(_playerUnitCount == 2);
        _playerElementsTriple.SetActive(_playerUnitCount == 3);
        _enemyElementsSingle.SetActive(_enemyUnitCount == 1);
        _enemyElementsDouble.SetActive(_enemyUnitCount == 2);
        _enemyElementsTriple.SetActive(_enemyUnitCount == 3);

        // Assign battle unit lists based on the unit counts.
        AssignBattleUnitLists();

        // Clear any previous data on the battle units.
        ClearUnitsData(_playerUnits);
        ClearUnitsData(_enemyUnits);

        // Set the background image.
        if (_backgroundMapping.ContainsKey(_battleTrigger))
        {
            _backgroundImage.sprite = _backgroundMapping[_battleTrigger];
        }
        else
        {
            _backgroundImage.sprite = _fieldBackground; // Fallback option
        }

        // Get healthy battlers from the player's party.
        List<Battler> playerBattlers = PlayerParty.GetHealthyBattlers(_playerUnitCount);

        if (!IsCommanderBattle)
        {
            for (int i = 0; i < _playerUnitCount; i++)
            {
                _playerUnits[i].Setup(playerBattlers[i]);
            }

            for (int i = 0; i < _enemyUnitCount; i++)
            {
                _enemyUnits[i].Setup(RogueBattlers[i]);
            }

            string rogueAppearance = RogueBattlers.Count > 1
                ? $"A group of rogue battlers has appeared!"
                : $"A rogue {RogueBattlers[0].Base.Name} has appeared!";
            yield return _dialogueBox.TypeDialogue(rogueAppearance);
        }
        else
        {
            List<Battler> enemyBattlers = EnemyParty.GetHealthyBattlers(_enemyUnitCount);
            enemyBattlers[0].IsCommander = true;

            for (int i = 0; i < _playerUnitCount; i++)
            {
                _playerUnits[i].Setup(playerBattlers[i]);
            }

            for (int i = 0; i < _enemyUnitCount; i++)
            {
                _enemyUnits[i].Setup(enemyBattlers[i]);
            }

            yield return _dialogueBox.TypeDialogue($"Commander {Enemy.Name} wants to battle!");
        }

        Field = new Field();
        BattleIsOver = false;
        EscapeAttempts = 0;
        _partyScreen.Init();
        _selectingUnitIndex = 0;
        StateMachine.ChangeState(ActionSelectionState.Instance);
    }

    /// <summary>
    /// Ends the battle and cleans up battle data.
    /// </summary>
    /// <param name="won">True if the player won the battle; otherwise, false.</param>
    public void BattleOver(bool won)
    {
        BattleIsOver = true;
        PlayerParty.Battlers.ForEach(static b => b.OnBattleOver());
        PlayerUnits.ForEach(static u => u.ClearData());
        EnemyUnits.ForEach(static u => u.ClearData());

        ActionSelectionState.Instance.SelectionUI.ResetSelection();
        OnBattleOver?.Invoke(won);
    }

    /// <summary>
    /// Called from the update loop to progress the battle state.
    /// </summary>
    public void HandleUpdate()
    {
        StateMachine.Execute();
    }

    /// <summary>
    /// Adds a battle action for the currently selecting unit and processes enemy actions if needed.
    /// </summary>
    /// <param name="battleAction">The battle action to add.</param>
    public void AddBattleAction(BattleAction battleAction)
    {
        battleAction.SourceUnit = SelectingUnit;
        _battleActions.Add(battleAction);

        if (_battleActions.Count == _playerUnits.Count)
        {
            SelectingUnit.SetSelected(false);
            // Process enemy actions
            ProcessEnemyActions();
            // Order actions by descending priority and then by agility.
            _battleActions = _battleActions
                .OrderByDescending(static a => a.Priority)
                .ThenByDescending(static a => a.SourceUnit.Battler.Agility)
                .ToList();
            RunTurnState.Instance.BattleActions = _battleActions;
            StateMachine.ChangeState(RunTurnState.Instance);
        }
        else
        {
            SelectingUnit.SetSelected(false);
            _selectingUnitIndex++;
            StateMachine.ChangeState(ActionSelectionState.Instance);
        }
    }

    /// <summary>
    /// Clears all currently stored battle actions and resets selection index.
    /// </summary>
    public void ClearBattleActions()
    {
        _battleActions = new List<BattleAction>();
        _selectingUnitIndex = 0;
    }

    /// <summary>
    /// Removes the last battle action and updates the selection state.
    /// </summary>
    public void UndoBattleAction()
    {
        if (_battleActions.Count > 0)
        {
            _battleActions.RemoveAt(_battleActions.Count - 1);
            SelectingUnit.SetSelected(false);
            _selectingUnitIndex--;
            StateMachine.ChangeState(ActionSelectionState.Instance);
        }
    }

    /// <summary>
    /// Switches the battler for a given battle unit, playing exit and entry animations.
    /// </summary>
    /// <param name="newBattler">The new battler to switch in.</param>
    /// <param name="unitToSwitch">The battle unit to switch out.</param>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator SwitchBattler(Battler newBattler, BattleUnit unitToSwitch)
    {
        if (unitToSwitch.Battler.Hp > 0)
        {
            yield return _dialogueBox.TypeDialogue($"Get back {unitToSwitch.Battler.Base.Name}!");
            _ = StartCoroutine(unitToSwitch.PlayExitAnimation());
            yield return new WaitForSeconds(SWITCH_BATTLER_WAIT_DURATION);
        }

        unitToSwitch.Setup(newBattler);
        _dialogueBox.SetMoveNames(newBattler.Moves);
        yield return _dialogueBox.TypeDialogue($"Come forward {newBattler.Base.Name}!");
    }

    /// <summary>
    /// Determines if the given battler is already scheduled to switch.
    /// </summary>
    /// <param name="battler">The battler to check.</param>
    /// <returns>True if the battler is already scheduled for switching; otherwise, false.</returns>
    public bool UnableToSwitch(Battler battler)
    {
        return _battleActions.Any(a => a.ActionType == BattleActionType.SwitchBattler && a.SelectedBattler == battler);
    }

    /// <summary>
    /// Processes the enemy actions by generating actions for each enemy unit.
    /// </summary>
    private void ProcessEnemyActions()
    {
        foreach (BattleUnit enemyUnit in _enemyUnits)
        {
            if (UnityEngine.Random.value < ENEMY_GUARD_CHANCE)
            {
                _battleActions.Add(new BattleAction
                {
                    ActionType = BattleActionType.Guard,
                    SourceUnit = enemyUnit
                });
            }
            else
            {
                Move selectedMove = enemyUnit.Battler.GetRandomMove() ?? new Move(GlobalSettings.Instance.BackupMove);
                List<BattleUnit> targetUnits = DetermineTargets(enemyUnit, selectedMove);
                _battleActions.Add(new BattleAction
                {
                    ActionType = BattleActionType.Fight,
                    SelectedMove = selectedMove,
                    SourceUnit = enemyUnit,
                    TargetUnits = targetUnits
                });
            }
        }
    }

    /// <summary>
    /// Determines the target units for an enemy action based on the move's target type.
    /// </summary>
    /// <param name="enemyUnit">The enemy battle unit performing the move.</param>
    /// <param name="selectedMove">The move selected.</param>
    /// <returns>A list of target BattleUnits.</returns>
    private List<BattleUnit> DetermineTargets(BattleUnit enemyUnit, Move selectedMove)
    {
        return selectedMove.Base.Target switch
        {
            MoveTarget.Self => new List<BattleUnit> { enemyUnit },
            MoveTarget.AllAllies => _enemyUnits,
            MoveTarget.AllEnemies => _playerUnits,
            MoveTarget.Ally => new List<BattleUnit> { _enemyUnits[UnityEngine.Random.Range(0, _enemyUnits.Count)] },
            MoveTarget.Enemy => new List<BattleUnit> { _playerUnits[UnityEngine.Random.Range(0, _playerUnits.Count)] },
            MoveTarget.Others => throw new NotImplementedException(),
            _ => _enemyUnits.Where(u => u != enemyUnit).Concat(_playerUnits).ToList(),
        };
    }

    /// <summary>
    /// Sends the next battler for the enemy commander into battle.
    /// </summary>
    /// <param name="newBattler">The new battler to be sent in.</param>
    /// <param name="defeatedUnit">The battle unit that was defeated.</param>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator SendNextCommanderBattler(Battler newBattler, BattleUnit defeatedUnit)
    {
        defeatedUnit.Setup(newBattler);
        yield return _dialogueBox.TypeDialogue($"Commander {Enemy.Name} summoned {newBattler.Base.Name} to battle!");
    }

    /// <summary>
    /// Assigns the correct BattleUnit lists for player and enemy based on the current unit counts.
    /// </summary>
    private void AssignBattleUnitLists()
    {
        // Assign player units
        _playerUnits = _playerUnitCount switch
        {
            1 => new List<BattleUnit> { _playerUnitSingle },
            2 => new List<BattleUnit>(_playerUnitsDouble),
            3 => new List<BattleUnit>(_playerUnitsTriple),
            _ => new List<BattleUnit>(),
        };

        // Assign enemy units
        _enemyUnits = _enemyUnitCount switch
        {
            1 => new List<BattleUnit> { _enemyUnitSingle },
            2 => new List<BattleUnit>(_enemyUnitsDouble),
            3 => new List<BattleUnit>(_enemyUnitsTriple),
            _ => new List<BattleUnit>(),
        };
    }

    /// <summary>
    /// Clears data for each BattleUnit in the provided list.
    /// </summary>
    /// <param name="units">The list of BattleUnits to clear.</param>
    private void ClearUnitsData(List<BattleUnit> units)
    {
        foreach (BattleUnit unit in units)
        {
            unit.ClearData();
        }
    }
}

/// <summary>
/// Enumerates possible battle trigger types for setting battle backgrounds.
/// </summary>
public enum BattleTrigger
{
    Desert,
    DesertOasis,
    Field,
    FieldLake,
    Meadow,
    Mountain,
    MountainClouds,
    Wasteland
}