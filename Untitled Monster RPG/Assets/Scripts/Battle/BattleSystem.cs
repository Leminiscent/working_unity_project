using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private BattleUnit _playerUnitSingle;
    [SerializeField] private BattleUnit _enemyUnitSingle;
    [SerializeField] private List<BattleUnit> _playerUnitsDouble;
    [SerializeField] private List<BattleUnit> _enemyUnitsDouble;
    [SerializeField] private List<BattleUnit> _playerUnitsTriple;
    [SerializeField] private List<BattleUnit> _enemyUnitsTriple;
    [SerializeField] private BattleDialogueBox _dialogueBox;
    [SerializeField] private PartyScreen _partyScreen;
    [SerializeField] private Image _playerImage;
    [SerializeField] private Image _enemyImage;
    [SerializeField] private MoveForgettingUI _moveForgettingUI;
    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private GameObject _playerElementsSingle;
    [SerializeField] private GameObject _playerElementsDouble;
    [SerializeField] private GameObject _playerElementsTriple;
    [SerializeField] private GameObject _enemyElementsSingle;
    [SerializeField] private GameObject _enemyElementsDouble;
    [SerializeField] private GameObject _enemyElementsTriple;

    [Header("Audio")]
    [SerializeField] private AudioClip _wildBattleMusic;
    [SerializeField] private AudioClip _masterBattleMusic;
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
    public List<Battler> WildBattlers { get; private set; }
    public Field Field { get; private set; }
    public bool IsMasterBattle { get; private set; }
    public int EscapeAttempts { get; set; }
    public MasterController Enemy { get; private set; }
    public PlayerController Player { get; private set; }
    public BattleDialogueBox DialogueBox => _dialogueBox;
    public PartyScreen PartyScreen => _partyScreen;
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

    public void StartWildBattle(BattleParty playerParty, List<Battler> wildBattlers, BattleTrigger trigger, int unitCount = 1)
    {
        IsMasterBattle = false;

        PlayerParty = playerParty;
        WildBattlers = wildBattlers;
        _enemyUnitCount = unitCount;

        _battleTrigger = trigger;
        AudioManager.Instance.PlayMusic(_wildBattleMusic);
        StartCoroutine(SetupBattle());
    }

    public void StartMasterBattle(BattleParty playerParty, BattleParty enemyParty, BattleTrigger trigger, int unitCount = 1)
    {
        IsMasterBattle = true;

        PlayerParty = playerParty;
        EnemyParty = enemyParty;
        _enemyUnitCount = unitCount;

        Player = playerParty.GetComponent<PlayerController>();
        Enemy = enemyParty.GetComponent<MasterController>();
        _battleTrigger = trigger;
        AudioManager.Instance.PlayMusic(_masterBattleMusic);
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        StateMachine = new StateMachine<BattleSystem>(this);
        _battleActions = new List<BattleAction>();
        _playerUnitCount = Mathf.Min(PlayerParty.Battlers.Count(static m => m.Hp > 0), 3);
        _playerElementsSingle.SetActive(_playerUnitCount == 1);
        _playerElementsDouble.SetActive(_playerUnitCount == 2);
        _playerElementsTriple.SetActive(_playerUnitCount == 3);
        _enemyElementsSingle.SetActive(_enemyUnitCount == 1);
        _enemyElementsDouble.SetActive(_enemyUnitCount == 2);
        _enemyElementsTriple.SetActive(_enemyUnitCount == 3);

        switch (_playerUnitCount)
        {
            case 1:
                _playerUnits = new List<BattleUnit> { _playerUnitSingle };
                break;
            case 2:
                _playerUnits = _playerUnitsDouble.GetRange(0, _playerUnitsDouble.Count);
                break;
            case 3:
                _playerUnits = _playerUnitsTriple.GetRange(0, _playerUnitsTriple.Count);
                break;
            default:
                break;
        }
        switch (_enemyUnitCount)
        {
            case 1:
                _enemyUnits = new List<BattleUnit> { _enemyUnitSingle };
                break;
            case 2:
                _enemyUnits = _enemyUnitsDouble.GetRange(0, _enemyUnitsDouble.Count);
                break;
            case 3:
                _enemyUnits = _enemyUnitsTriple.GetRange(0, _enemyUnitsTriple.Count);
                break;
            default:
                break;
        }

        for (int i = 0; i < Mathf.Min(_playerUnitCount, 3); i++)
        {
            _playerUnits[i].ClearData();
        }
        for (int i = 0; i < _enemyUnitCount; i++)
        {
            _enemyUnits[i].ClearData();
        }

        if (_backgroundMapping.ContainsKey(_battleTrigger))
        {
            _backgroundImage.sprite = _backgroundMapping[_battleTrigger];
        }
        else
        {
            _backgroundImage.sprite = _fieldBackground; // Fallback option
        }

        List<Battler> playerBattlers = PlayerParty.GetHealthyBattlers(_playerUnitCount);

        if (!IsMasterBattle)
        {
            for (int i = 0; i < _playerUnitCount; i++)
            {
                _playerUnits[i].Setup(playerBattlers[i]);
            }
            for (int i = 0; i < _enemyUnitCount; i++)
            {
                _enemyUnits[i].Setup(WildBattlers[i]);
            }

            string wildAppearance = WildBattlers.Count > 1
                ? $"Wild {string.Join(", ", WildBattlers.Select(static m => m.Base.Name).Take(WildBattlers.Count - 1))} and {WildBattlers.Last().Base.Name} have appeared!"
                : $"A wild {WildBattlers[0].Base.Name} has appeared!";

            yield return _dialogueBox.TypeDialogue(wildAppearance);
        }
        else
        {
            for (int i = 0; i < _playerUnitCount; i++)
            {
                _playerUnits[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < _enemyUnitCount; i++)
            {
                _enemyUnits[i].gameObject.SetActive(false);
            }

            _playerImage.gameObject.SetActive(true);
            _enemyImage.gameObject.SetActive(true);
            _playerImage.sprite = Player.Character.Animator.GetAllSprites()[8];
            _enemyImage.sprite = Enemy.Character.Animator.GetAllSprites()[12];

            yield return _dialogueBox.TypeDialogue($"{Enemy.Name} wants to battle!");

            _enemyImage.sprite = Enemy.Character.Animator.GetAllSprites()[8];
            _enemyImage.transform.DOLocalMoveX(1500, 1f);
            yield return new WaitForSeconds(0.75f);
            _enemyImage.gameObject.SetActive(false);

            List<Battler> enemyBattlers = EnemyParty.GetHealthyBattlers(_enemyUnitCount);

            for (int i = 0; i < _enemyUnitCount; i++)
            {
                _enemyUnits[i].gameObject.SetActive(true);
                _enemyUnits[i].Setup(enemyBattlers[i]);
            }

            string battlerNames = enemyBattlers.Count > 1
                ? $"{string.Join(", ", enemyBattlers.Select(static m => m.Base.Name).Take(enemyBattlers.Count - 1))} and {enemyBattlers.Last().Base.Name}"
                : enemyBattlers[0].Base.Name;

            yield return _dialogueBox.TypeDialogue($"{Enemy.Name} sent out {battlerNames}!");

            _playerImage.sprite = Player.Character.Animator.GetAllSprites()[12];
            _playerImage.transform.DOLocalMoveX(-1500, 1f);
            yield return new WaitForSeconds(0.75f);
            _playerImage.gameObject.SetActive(false);

            for (int i = 0; i < _playerUnitCount; i++)
            {
                _playerUnits[i].gameObject.SetActive(true);
                _playerUnits[i].Setup(playerBattlers[i]);
            }

            battlerNames = playerBattlers.Count > 1
                ? $"{string.Join(", ", playerBattlers.Select(static m => m.Base.Name).Take(playerBattlers.Count - 1))} and {playerBattlers.Last().Base.Name}"
                : playerBattlers[0].Base.Name;

            yield return _dialogueBox.TypeDialogue($"Go {battlerNames}!");
        }

        Field = new Field();
        BattleIsOver = false;
        EscapeAttempts = 0;
        _partyScreen.Init();
        _selectingUnitIndex = 0;
        StateMachine.ChangeState(ActionSelectionState.Instance);
    }

    public void BattleOver(bool won)
    {
        BattleIsOver = true;
        PlayerParty.Battlers.ForEach(static m => m.OnBattleOver());

        PlayerUnits.ForEach(static u => u.ClearData());
        EnemyUnits.ForEach(static u => u.ClearData());

        ActionSelectionState.Instance.SelectionUI.ResetSelection();
        OnBattleOver(won);
    }

    public void HandleUpdate()
    {
        StateMachine.Execute();
    }

    public void AddBattleAction(BattleAction battleAction)
    {
        battleAction.SourceUnit = SelectingUnit;
        _battleActions.Add(battleAction);

        if (_battleActions.Count == _playerUnits.Count)
        {
            SelectingUnit.SetSelected(false);
            
            foreach (BattleUnit enemyUnit in _enemyUnits)
            {
                if (UnityEngine.Random.value < 0.2f)
                {
                    _battleActions.Add(new BattleAction()
                    {
                        ActionType = BattleActionType.Guard,
                        SourceUnit = enemyUnit
                    });
                }
                else
                {
                    Move selectedMove = enemyUnit.Battler.GetRandomMove() ?? new Move(GlobalSettings.Instance.BackupMove);
                    _battleActions.Add(new BattleAction()
                    {
                        ActionType = BattleActionType.Fight,
                        SelectedMove = selectedMove,
                        SourceUnit = enemyUnit,
                        TargetUnits = selectedMove.Base.Target == MoveTarget.Self ? new List<BattleUnit> { enemyUnit }
                            : selectedMove.Base.Target == MoveTarget.AllAllies ? _enemyUnits
                            : selectedMove.Base.Target == MoveTarget.AllEnemies ? _playerUnits
                            : selectedMove.Base.Target == MoveTarget.Ally ? new List<BattleUnit> { _enemyUnits[UnityEngine.Random.Range(0, _enemyUnits.Count)] }
                            : selectedMove.Base.Target == MoveTarget.Enemy ? new List<BattleUnit> { _playerUnits[UnityEngine.Random.Range(0, _playerUnits.Count)] }
                            : _enemyUnits.Where(u => u != enemyUnit).Concat(_playerUnits).ToList()
                    });
                }
            }
            _battleActions = _battleActions.OrderByDescending(static a => a.Priority).ThenByDescending(static a => a.SourceUnit.Battler.Agility).ToList();
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
            yield return _dialogueBox.TypeDialogue($"Come back {unitToSwitch.Battler.Base.Name}!");
            StartCoroutine(unitToSwitch.PlayExitAnimation());
            yield return new WaitForSeconds(0.75f);
        }

        unitToSwitch.Setup(newBattler);
        _dialogueBox.SetMoveNames(newBattler.Moves);
        yield return _dialogueBox.TypeDialogue($"Go {newBattler.Base.Name}!");
    }

    public bool UnableToSwitch(Battler battler)
    {
        return _battleActions.Any(a => a.ActionType == BattleActionType.SwitchBattler && a.SelectedBattler == battler);
    }

    public IEnumerator SendNextMasterBattler(Battler newBattler, BattleUnit defeatedUnit)
    {
        defeatedUnit.Setup(newBattler);
        yield return _dialogueBox.TypeDialogue($"{Enemy.Name} sent out {newBattler.Base.Name}!");
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