using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public BattleActionType SelectedAction { get; set; }
    public Monster SelectedMonster { get; set; }
    public ItemBase SelectedItem { get; set; }
    public bool BattleIsOver { get; private set; }
    public MonsterParty PlayerParty { get; private set; }
    public MonsterParty EnemyParty { get; private set; }
    public List<Monster> WildMonsters { get; private set; }
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
        WildMonsters = wildMonsters;
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
        for (int i = 0; i < _unitCount; i++)
        {
            _playerUnits[i].Clear();
            _enemyUnits[i].Clear();
        }

        if (_backgroundMapping.ContainsKey(_battleTrigger))
        {
            _backgroundImage.sprite = _backgroundMapping[_battleTrigger];
        }
        else
        {
            _backgroundImage.sprite = _fieldBackground; // Fallback option
        }

        List<Monster> playerMonsters = PlayerParty.GetHealthyMonsters(_unitCount);

        if (!IsMasterBattle)
        {
            for (int i = 0; i < _unitCount; i++)
            {
                _playerUnits[i].Setup(playerMonsters[i]);
                _enemyUnits[i].Setup(WildMonsters[i]);
            }

            string wildAppearance = WildMonsters.Count > 1
                ? $"Wild {string.Join(", ", WildMonsters.Select(static m => m.Base.Name).Take(WildMonsters.Count - 1))} and {WildMonsters.Last().Base.Name} appeared!"
                : $"A wild {WildMonsters[0].Base.Name} appeared!";

            yield return _dialogueBox.TypeDialogue(wildAppearance);
        }
        else
        {
            for (int i = 0; i < _unitCount; i++)
            {
                _playerUnits[i].gameObject.SetActive(false);
                _enemyUnits[i].gameObject.SetActive(false);
            }

            _playerImage.gameObject.SetActive(true);
            _enemyImage.gameObject.SetActive(true);
            _playerImage.sprite = Player.Sprite;
            _enemyImage.sprite = Enemy.Sprite;

            yield return _dialogueBox.TypeDialogue($"{Enemy.Name} wants to battle!");
            _enemyImage.gameObject.SetActive(false);

            List<Monster> enemyMonsters = EnemyParty.GetHealthyMonsters(_unitCount);

            for (int i = 0; i < _unitCount; i++)
            {
                _enemyUnits[i].gameObject.SetActive(true);
                _enemyUnits[i].Setup(enemyMonsters[i]);
            }

            string monsterNames = enemyMonsters.Count > 1
                ? $"{string.Join(", ", enemyMonsters.Select(static m => m.Base.Name).Take(enemyMonsters.Count - 1))} and {enemyMonsters.Last().Base.Name}"
                : enemyMonsters[0].Base.Name;

            yield return _dialogueBox.TypeDialogue($"{Enemy.Name} sent out {monsterNames}!");

            _playerImage.gameObject.SetActive(false);

            for (int i = 0; i < _unitCount; i++)
            {
                _playerUnits[i].gameObject.SetActive(true);
                _playerUnits[i].Setup(playerMonsters[i]);
            }

            monsterNames = playerMonsters.Count > 1
                ? $"{string.Join(", ", playerMonsters.Select(static m => m.Base.Name).Take(playerMonsters.Count - 1))} and {playerMonsters.Last().Base.Name}"
                : playerMonsters[0].Base.Name;

            yield return _dialogueBox.TypeDialogue($"Go {monsterNames}!");
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
        for (int i = 0; i < _unitCount; i++)
        {
            _playerUnits[i].Hud.ClearData();
            _enemyUnits[i].Hud.ClearData();
        }
        ActionSelectionState.Instance.SelectionUI.ResetSelection();
        OnBattleOver(won);
    }

    public void HandleUpdate()
    {
        StateMachine.Execute();
    }

    public IEnumerator SwitchMonster(Monster newMonster, BattleUnit unitToSwitch)
    {
        if (unitToSwitch.Monster.Hp > 0)
        {
            yield return _dialogueBox.TypeDialogue($"Come back {unitToSwitch.Monster.Base.Name}!");
            unitToSwitch.PlayExitAnimation();
            yield return new WaitForSeconds(0.75f);
        }

        unitToSwitch.Setup(newMonster);
        _dialogueBox.SetMoveNames(newMonster.Moves);
        yield return _dialogueBox.TypeDialogue($"Go {newMonster.Base.Name}!");
    }

    public IEnumerator SendNextMasterMonster()
    {
        Monster nextMonster = EnemyParty.GetHealthyMonster();

        _enemyUnits[0].Setup(nextMonster);
        yield return _dialogueBox.TypeDialogue($"{Enemy.Name} sent out {nextMonster.Base.Name}!");
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