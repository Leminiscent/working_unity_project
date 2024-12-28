using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private List<BattleUnit> _playerUnits;
    [SerializeField] private List<BattleUnit> _enemyUnits;
    [SerializeField] private BattleDialogueBox _dialogueBox;
    [SerializeField] private PartyScreen _partyScreen;
    [SerializeField] private Image _playerImage;
    [SerializeField] private Image _enemyImage;
    [SerializeField] private MoveForgettingUI _moveForgettingUI;
    [SerializeField] private InventoryUI _inventoryUI;

    [Header("Audio")]
    [SerializeField] private AudioClip _wildBattleMusic;
    [SerializeField] private AudioClip _masterBattleMusic;
    [SerializeField] private AudioClip _battleVictoryMusic;

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
    private int _unitCount;

    public StateMachine<BattleSystem> StateMachine { get; private set; }
    public event Action<bool> OnBattleOver;
    public int SelectedMove { get; set; }
    public BattleAction SelectedAction { get; set; }
    public Monster SelectedMonster { get; set; }
    public ItemBase SelectedItem { get; set; }
    public bool BattleIsOver { get; private set; }
    public MonsterParty PlayerParty { get; private set; }
    public MonsterParty EnemyParty { get; private set; }
    public Monster WildMonster { get; private set; }
    public Field Field { get; private set; }
    public bool IsMasterBattle { get; private set; }
    public int EscapeAttempts { get; set; }
    public MasterController Enemy { get; private set; }
    public PlayerController Player { get; private set; }
    public BattleDialogueBox DialogueBox => _dialogueBox;
    public PartyScreen PartyScreen => _partyScreen;
    public List<BattleUnit> PlayerUnits => _playerUnits;
    public List<BattleUnit> EnemyUnits => _enemyUnits;
    public AudioClip BattleVictoryMusic => _battleVictoryMusic;

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

    public void StartWildBattle(MonsterParty playerParty, List<Monster> wildMonsters, BattleTrigger trigger, int unitCount = 1)
    {
        IsMasterBattle = false;

        PlayerParty = playerParty;
        WildMonster = wildMonsters;
        _unitCount = unitCount;

        _battleTrigger = trigger;
        AudioManager.Instance.PlayMusic(_wildBattleMusic);
        StartCoroutine(SetupBattle());
    }

    public void StartMasterBattle(MonsterParty playerParty, MonsterParty enemyParty, BattleTrigger trigger, int unitCount = 1)
    {
        IsMasterBattle = true;

        PlayerParty = playerParty;
        EnemyParty = enemyParty;
        _unitCount = unitCount;

        Player = playerParty.GetComponent<PlayerController>();
        Enemy = enemyParty.GetComponent<MasterController>();
        _battleTrigger = trigger;
        AudioManager.Instance.PlayMusic(_masterBattleMusic);
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        StateMachine = new StateMachine<BattleSystem>(this);
        _playerUnits.Clear();
        _enemyUnits.Clear();

        if (_backgroundMapping.ContainsKey(_battleTrigger))
        {
            _backgroundImage.sprite = _backgroundMapping[_battleTrigger];
        }
        else
        {
            _backgroundImage.sprite = _fieldBackground; // Fallback option
        }

        if (!IsMasterBattle)
        {
            _playerUnits.Setup(PlayerParty.GetHealthyMonster());
            _enemyUnits.Setup(WildMonster);

            _dialogueBox.SetMoveNames(_playerUnits.Monster.Moves);
            yield return _dialogueBox.TypeDialogue("A wild " + _enemyUnits.Monster.Base.Name + " appeared!");
        }
        else
        {
            _playerUnits.gameObject.SetActive(false);
            _enemyUnits.gameObject.SetActive(false);

            _playerImage.gameObject.SetActive(true);
            _enemyImage.gameObject.SetActive(true);
            _playerImage.sprite = Player.Sprite;
            _enemyImage.sprite = Enemy.Sprite;

            yield return _dialogueBox.TypeDialogue(Enemy.Name + " wants to battle!");

            _enemyImage.gameObject.SetActive(false);
            _enemyUnits.gameObject.SetActive(true);

            Monster enemyMonster = EnemyParty.GetHealthyMonster();

            _enemyUnits.Setup(enemyMonster);
            yield return _dialogueBox.TypeDialogue(Enemy.Name + " sent out " + enemyMonster.Base.Name + "!");

            _playerImage.gameObject.SetActive(false);
            _playerUnits.gameObject.SetActive(true);

            Monster playerMonster = PlayerParty.GetHealthyMonster();

            _playerUnits.Setup(playerMonster);
            yield return _dialogueBox.TypeDialogue("Go " + playerMonster.Base.Name + "!");
            _dialogueBox.SetMoveNames(_playerUnits.Monster.Moves);
        }

        Field = new Field();
        BattleIsOver = false;
        EscapeAttempts = 0;
        _partyScreen.Init();
        StateMachine.ChangeState(ActionSelectionState.Instance);
    }

    public void BattleOver(bool won)
    {
        BattleIsOver = true;
        PlayerParty.Monsters.ForEach(static m => m.OnBattleOver());
        _playerUnits.Hud.ClearData();
        _enemyUnits.Hud.ClearData();
        ActionSelectionState.Instance.SelectionUI.ResetSelection();
        OnBattleOver(won);
    }

    public void HandleUpdate()
    {
        StateMachine.Execute();
    }

    public IEnumerator SwitchMonster(Monster newMonster)
    {
        if (_playerUnits.Monster.Hp > 0)
        {
            yield return _dialogueBox.TypeDialogue("Come back " + _playerUnits.Monster.Base.Name + "!");
            _playerUnits.PlayExitAnimation();
            yield return new WaitForSeconds(0.75f);
        }

        _playerUnits.Setup(newMonster);
        _dialogueBox.SetMoveNames(newMonster.Moves);
        yield return _dialogueBox.TypeDialogue("Go " + newMonster.Base.Name + "!");
    }

    public IEnumerator SendNextMasterMonster()
    {
        Monster nextMonster = EnemyParty.GetHealthyMonster();

        _enemyUnits.Setup(nextMonster);
        yield return _dialogueBox.TypeDialogue(Enemy.Name + " sent out " + nextMonster.Base.Name + "!");
    }
}

public enum BattleAction
{
    Fight,
    Talk,
    UseItem,
    SwitchMonster,
    Run
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