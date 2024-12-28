using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private BattleUnit _playerUnit;
    [SerializeField] private BattleUnit _enemyUnit;
    [SerializeField] private BattleDialogueBox _dialogueBox;
    [SerializeField] private PartyScreen _partyScreen;
    [SerializeField] private Image _playerImage;
    [SerializeField] private Image _enemyImage;

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
    public BattleUnit PlayerUnit => _playerUnit;
    public BattleUnit EnemyUnit => _enemyUnit;
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

    public void StartWildBattle(MonsterParty playerParty, Monster wildMonster, BattleTrigger trigger)
    {
        IsMasterBattle = false;

        PlayerParty = playerParty;
        WildMonster = wildMonster;

        _battleTrigger = trigger;
        AudioManager.Instance.PlayMusic(_wildBattleMusic);
        StartCoroutine(SetupBattle());
    }

    public void StartMasterBattle(MonsterParty playerParty, MonsterParty enemyParty, BattleTrigger trigger)
    {
        IsMasterBattle = true;

        PlayerParty = playerParty;
        EnemyParty = enemyParty;

        Player = playerParty.GetComponent<PlayerController>();
        Enemy = enemyParty.GetComponent<MasterController>();
        _battleTrigger = trigger;
        AudioManager.Instance.PlayMusic(_masterBattleMusic);
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        StateMachine = new StateMachine<BattleSystem>(this);
        _playerUnit.Clear();
        _enemyUnit.Clear();

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
            _playerUnit.Setup(PlayerParty.GetHealthyMonster());
            _enemyUnit.Setup(WildMonster);

            _dialogueBox.SetMoveNames(_playerUnit.Monster.Moves);
            yield return _dialogueBox.TypeDialogue("A wild " + _enemyUnit.Monster.Base.Name + " appeared!");
        }
        else
        {
            _playerUnit.gameObject.SetActive(false);
            _enemyUnit.gameObject.SetActive(false);

            _playerImage.gameObject.SetActive(true);
            _enemyImage.gameObject.SetActive(true);
            _playerImage.sprite = Player.Sprite;
            _enemyImage.sprite = Enemy.Sprite;

            yield return _dialogueBox.TypeDialogue(Enemy.Name + " wants to battle!");

            _enemyImage.gameObject.SetActive(false);
            _enemyUnit.gameObject.SetActive(true);

            Monster enemyMonster = EnemyParty.GetHealthyMonster();

            _enemyUnit.Setup(enemyMonster);
            yield return _dialogueBox.TypeDialogue(Enemy.Name + " sent out " + enemyMonster.Base.Name + "!");

            _playerImage.gameObject.SetActive(false);
            _playerUnit.gameObject.SetActive(true);

            Monster playerMonster = PlayerParty.GetHealthyMonster();

            _playerUnit.Setup(playerMonster);
            yield return _dialogueBox.TypeDialogue("Go " + playerMonster.Base.Name + "!");
            _dialogueBox.SetMoveNames(_playerUnit.Monster.Moves);
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
        _playerUnit.Hud.ClearData();
        _enemyUnit.Hud.ClearData();
        ActionSelectionState.Instance.SelectionUI.ResetSelection();
        OnBattleOver(won);
    }

    public void HandleUpdate()
    {
        StateMachine.Execute();
    }

    public IEnumerator SwitchMonster(Monster newMonster)
    {
        if (_playerUnit.Monster.Hp > 0)
        {
            yield return _dialogueBox.TypeDialogue("Come back " + _playerUnit.Monster.Base.Name + "!");
            _playerUnit.PlayExitAnimation();
            yield return new WaitForSeconds(0.75f);
        }

        _playerUnit.Setup(newMonster);
        _dialogueBox.SetMoveNames(newMonster.Moves);
        yield return _dialogueBox.TypeDialogue("Go " + newMonster.Base.Name + "!");
    }

    public IEnumerator SendNextMasterMonster()
    {
        Monster nextMonster = EnemyParty.GetHealthyMonster();

        _enemyUnit.Setup(nextMonster);
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