using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Util.StateMachine;

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
    [SerializeField] private GameObject _playerElementsSingle;
    [SerializeField] private GameObject _playerElementsDouble;
    [SerializeField] private GameObject _playerElementsTriple;
    [SerializeField] private GameObject _enemyElementsSingle;
    [SerializeField] private GameObject _enemyElementsDouble;
    [SerializeField] private GameObject _enemyElementsTriple;
    [field: SerializeField, FormerlySerializedAs("_dialogueBox")] public BattleDialogueBox DialogueBox { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioClip _rogueBattleMusic;
    [SerializeField] private AudioClip _commanderBattleMusic;
    [field: SerializeField, FormerlySerializedAs("_battleWonMusic")] public AudioClip BattleWonMusic { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_battleLostMusic")] public AudioClip BattleLostMusic { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_battleFledMusic")] public AudioClip BattleFledMusic { get; private set; }

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
    public List<BattleUnit> PlayerUnits { get; private set; }
    public List<BattleUnit> EnemyUnits { get; private set; }
    public Dictionary<BattleUnit, int> UnitSelectionIndices { get; private set; }
    public BattleUnit SelectingUnit => PlayerUnits[_selectingUnitIndex];

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

    public IEnumerator SetupBattle()
    {
        _ = StartCoroutine(Fader.Instance.FadeOut(0.5f));
        StateMachine = new StateMachine<BattleSystem>(this);
        _battleActions = new List<BattleAction>();
        UnitSelectionIndices = new Dictionary<BattleUnit, int>();

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
        ClearUnitsData(PlayerUnits);
        ClearUnitsData(EnemyUnits);

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
                PlayerUnits[i].Setup(playerBattlers[i]);
            }

            for (int i = 0; i < _enemyUnitCount; i++)
            {
                EnemyUnits[i].Setup(RogueBattlers[i]);
            }

            string rogueAppearance = RogueBattlers.Count > 1
                ? $"A group of rogue battlers has appeared!"
                : $"A rogue {RogueBattlers[0].Base.Name} has appeared!";
            yield return DialogueBox.TypeDialogue(rogueAppearance);
        }
        else
        {
            List<Battler> enemyBattlers = EnemyParty.GetHealthyBattlers(_enemyUnitCount);
            enemyBattlers[0].IsCommander = true;

            for (int i = 0; i < _playerUnitCount; i++)
            {
                PlayerUnits[i].Setup(playerBattlers[i]);
            }

            for (int i = 0; i < _enemyUnitCount; i++)
            {
                EnemyUnits[i].Setup(enemyBattlers[i]);
            }

            yield return DialogueBox.TypeDialogue($"Commander {Enemy.Name} wants to battle!");
        }

        Field = new Field();
        BattleIsOver = false;
        EscapeAttempts = 0;
        _selectingUnitIndex = 0;
        StateMachine.ChangeState(ActionSelectionState.Instance);
    }

    public IEnumerator BattleOver(bool won)
    {
        BattleIsOver = true;
        PlayerParty.Battlers.ForEach(static b => b.OnBattleOver());
        PlayerUnits.ForEach(static u => u.ClearData());
        EnemyUnits.ForEach(static u => u.ClearData());
        ActionSelectionState.Instance.SelectionUI.ResetSelection();

        yield return Fader.Instance.FadeIn(0.5f);
        OnBattleOver?.Invoke(won);
    }

    public void HandleUpdate()
    {
        StateMachine.Execute();
    }

    public void AddBattleAction(BattleAction battleAction)
    {
        battleAction.SourceUnit = SelectingUnit;
        _battleActions.Add(battleAction);

        if (_battleActions.Count == PlayerUnits.Count)
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

    public void ClearBattleActions()
    {
        _battleActions = new List<BattleAction>();
        _selectingUnitIndex = 0;
    }

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

    public IEnumerator SwitchBattler(Battler newBattler, BattleUnit unitToSwitch)
    {
        if (unitToSwitch.Battler.Hp > 0)
        {
            yield return DialogueBox.TypeDialogue($"Get back {unitToSwitch.Battler.Base.Name}!");
            _ = StartCoroutine(unitToSwitch.PlayExitAnimation());
            yield return new WaitForSeconds(SWITCH_BATTLER_WAIT_DURATION);
        }

        unitToSwitch.Setup(newBattler);
        DialogueBox.SetMoveNames(newBattler.Moves);
        yield return DialogueBox.TypeDialogue($"Come forward {newBattler.Base.Name}!");
    }

    public bool UnableToSwitch(Battler battler)
    {
        return _battleActions.Any(a => a.ActionType == BattleActionType.SwitchBattler && a.SelectedBattler == battler);
    }

    private void ProcessEnemyActions()
    {
        foreach (BattleUnit enemyUnit in EnemyUnits)
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

    private List<BattleUnit> DetermineTargets(BattleUnit enemyUnit, Move selectedMove)
    {
        return selectedMove.Base.Target switch
        {
            MoveTarget.Self => new List<BattleUnit> { enemyUnit },
            MoveTarget.AllAllies => EnemyUnits,
            MoveTarget.AllEnemies => PlayerUnits,
            MoveTarget.Ally => new List<BattleUnit> { EnemyUnits[UnityEngine.Random.Range(0, EnemyUnits.Count)] },
            MoveTarget.Enemy => new List<BattleUnit> { PlayerUnits[UnityEngine.Random.Range(0, PlayerUnits.Count)] },
            MoveTarget.Others => throw new NotImplementedException(),
            _ => EnemyUnits.Where(u => u != enemyUnit).Concat(PlayerUnits).ToList(),
        };
    }

    public IEnumerator SendNextCommanderBattler(Battler newBattler, BattleUnit defeatedUnit)
    {
        defeatedUnit.Setup(newBattler);
        yield return DialogueBox.TypeDialogue($"Commander {Enemy.Name} summoned {newBattler.Base.Name} to battle!");
    }

    private void AssignBattleUnitLists()
    {
        // Assign player units
        PlayerUnits = _playerUnitCount switch
        {
            1 => new List<BattleUnit> { _playerUnitSingle },
            2 => new List<BattleUnit>(_playerUnitsDouble),
            3 => new List<BattleUnit>(_playerUnitsTriple),
            _ => new List<BattleUnit>(),
        };

        // Assign enemy units
        EnemyUnits = _enemyUnitCount switch
        {
            1 => new List<BattleUnit> { _enemyUnitSingle },
            2 => new List<BattleUnit>(_enemyUnitsDouble),
            3 => new List<BattleUnit>(_enemyUnitsTriple),
            _ => new List<BattleUnit>(),
        };
    }

    private void ClearUnitsData(List<BattleUnit> units)
    {
        foreach (BattleUnit unit in units)
        {
            unit.ClearData(false);
        }
    }
}

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