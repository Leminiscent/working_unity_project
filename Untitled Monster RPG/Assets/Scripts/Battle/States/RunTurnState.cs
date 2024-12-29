using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;

public class RunTurnState : State<BattleSystem>
{
    private BattleSystem _battleSystem;
    private BattleUnit _playerUnit;
    private BattleUnit _enemyUnit;
    private string _playerMonsterName;
    private string _enemyMonsterName;
    private BattleDialogueBox _dialogueBox;
    private MonsterParty _playerParty;
    private MonsterParty _enemyParty;
    private bool _isMasterBattle;
    private Field _field;

    public static RunTurnState Instance { get; private set; }
    public List<BattleAction> BattleActions { get; set; }

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

    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;
        _playerUnit = _battleSystem.PlayerUnits;
        _enemyUnit = _battleSystem.EnemyUnits;
        _dialogueBox = _battleSystem.DialogueBox;
        _playerParty = _battleSystem.PlayerParty;
        _enemyParty = _battleSystem.EnemyParty;
        _isMasterBattle = _battleSystem.IsMasterBattle;
        _field = _battleSystem.Field;

        StartCoroutine(RunTurns(_battleSystem.SelectedAction));
    }

    private IEnumerator RunTurns(BattleActionType playerAction)
    {
        foreach (BattleAction action in BattleActions)
        {
            if (action.ActionType == BattleActionType.Fight)
            {
                action.SourceUnit.Monster.CurrentMove = action.SelectedMove;
                yield return RunMove(action.SourceUnit, action.TargetUnit, action.SelectedMove);
                yield return RunAfterTurn(action.SourceUnit);
            }
            else if (action.ActionType == BattleActionType.Talk)
            {

            }
            else if (action.ActionType == BattleActionType.UseItem)
            {

            }
            else if (action.ActionType == BattleActionType.SwitchMonster)
            {
                yield return _battleSystem.SwitchMonster(action.SelectedMonster, action.SourceUnit);
            }
            else if (action.ActionType == BattleActionType.Run)
            {

            }
        }

        if (playerAction == BattleActionType.Fight)
        {
            _playerUnit.Monster.CurrentMove = (_battleSystem.SelectedMove != -1) ? _playerUnit.Monster.Moves[_battleSystem.SelectedMove] : new Move(GlobalSettings.Instance.BackupMove);
            _enemyUnit.Monster.CurrentMove = _enemyUnit.Monster.GetRandomMove() ?? new Move(GlobalSettings.Instance.BackupMove);

            int playerMovePriority = _playerUnit.Monster.CurrentMove.Base.Priority;
            int enemyMovePriority = _enemyUnit.Monster.CurrentMove.Base.Priority;
            bool playerGoesFirst = true;

            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = _playerUnit.Monster.Agility >= _enemyUnit.Monster.Agility;
            }

            BattleUnit firstUnit = playerGoesFirst ? _playerUnit : _enemyUnit;
            string firstUnitName = playerGoesFirst ? _playerMonsterName : _enemyMonsterName;
            BattleUnit secondUnit = playerGoesFirst ? _enemyUnit : _playerUnit;
            string secondUnitName = playerGoesFirst ? _enemyMonsterName : _playerMonsterName;
            Monster secondMonster = secondUnit.Monster;

            yield return RunMove(firstUnit, secondUnit, firstUnit.Monster.CurrentMove);
            if (_battleSystem.BattleIsOver)
            {
                yield break;
            }

            if (secondMonster.Hp > 0)
            {
                yield return RunMove(secondUnit, firstUnit, secondMonster.CurrentMove);
                if (_battleSystem.BattleIsOver)
                {
                    yield break;
                }
            }
        }
        else
        {
            if (playerAction == BattleActionType.SwitchMonster)
            {
                yield return _battleSystem.SwitchMonster(_battleSystem.SelectedMonster);
            }
            else if (playerAction == BattleActionType.Run)
            {
                yield return AttemptEscape();
            }

            _enemyUnit.Monster.CurrentMove = _enemyUnit.Monster.GetRandomMove() ?? new Move(GlobalSettings.Instance.BackupMove);
            yield return RunMove(_enemyUnit, _playerUnit, _enemyUnit.Monster.CurrentMove);
            if (_battleSystem.BattleIsOver)
            {
                yield break;
            }
        }

        BattleUnit fasterUnit = _playerUnit.Monster.Agility >= _enemyUnit.Monster.Agility ? _playerUnit : _enemyUnit;
        string fasterUnitName = fasterUnit == _playerUnit ? _playerMonsterName : _enemyMonsterName;
        BattleUnit slowerUnit = fasterUnit == _playerUnit ? _enemyUnit : _playerUnit;
        string slowerUnitName = slowerUnit == _playerUnit ? _playerMonsterName : _enemyMonsterName;

        if (_field.Weather != null)
        {
            yield return _dialogueBox.TypeDialogue(_field.Weather.EffectMessage);

            _field.Weather.OnWeather?.Invoke(fasterUnit.Monster);
            yield return ShowStatusChanges(fasterUnit.Monster);
            yield return fasterUnit.Hud.WaitForHPUpdate();
            if (fasterUnit.Monster.Hp <= 0)
            {
                yield return HandleMonsterDefeat(fasterUnit);
            }

            _field.Weather.OnWeather?.Invoke(slowerUnit.Monster);
            yield return ShowStatusChanges(slowerUnit.Monster);
            yield return slowerUnit.Hud.WaitForHPUpdate();
            if (slowerUnit.Monster.Hp <= 0)
            {
                yield return HandleMonsterDefeat(slowerUnit);
            }

            if (_field.WeatherDuration != null)
            {
                _field.WeatherDuration--;
                if (_field.WeatherDuration == 0)
                {
                    _field.Weather = null;
                    _field.WeatherDuration = null;
                    yield return _dialogueBox.TypeDialogue("The weather has returned to normal.");
                }
            }
        }

        yield return RunAfterTurn(fasterUnit);
        yield return RunAfterTurn(slowerUnit);

        if (!_battleSystem.BattleIsOver)
        {
            _battleSystem.StateMachine.ChangeState(ActionSelectionState.Instance);
        }
    }

    private IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (_battleSystem.BattleIsOver)
        {
            yield break;
        }

        sourceUnit.Monster.OnEndOfTurn();
        yield return ShowStatusChanges(sourceUnit.Monster);
        yield return sourceUnit.Hud.WaitForHPUpdate();
        if (sourceUnit.Monster.Hp <= 0)
        {
            yield return HandleMonsterDefeat(sourceUnit);
        }
    }

    private bool CheckIfMoveHits(Move move, Monster source, Monster target)
    {
        if (move.Base.AlwaysHits)
        {
            return true;
        }

        if (move.Base.OneHitKO.IsOneHitKO)
        {
            if (source.Level < target.Level)
            {
                return false;
            }
            if (target.HasType(move.Base.OneHitKO.ImmunityType))
            {
                return false;
            }

            int baseAccuracy = move.Base.Accuracy;

            if (move.Base.OneHitKO.LowerOddsException)
            {
                baseAccuracy = source.HasType(move.Base.Type) ? baseAccuracy : baseAccuracy / 2;
            }

            int chance = source.Level - target.Level + baseAccuracy;
            return Random.Range(1, 101) <= chance;
        }

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];
        float[] boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else
        {
            moveAccuracy *= boostValues[-evasion];
        }

        return Random.Range(1, 101) <= moveAccuracy;
    }

    private IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Monster.OnStartOfTurn();

        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Monster);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }

        yield return ShowStatusChanges(sourceUnit.Monster);
        move.Sp--;

        if (move.Base == GlobalSettings.Instance.BackupMove)
        {
            yield return _dialogueBox.TypeDialogue($"{sourceUnit.Monster.Base.Name} has no SP left!");
        }
        yield return _dialogueBox.TypeDialogue($"{sourceUnit.Monster.Base.Name} used {move.Base.Name}!");

        if (CheckIfMoveHits(move, sourceUnit.Monster, targetUnit.Monster))
        {
            int hitCount = move.Base.GetHitCount();
            float typeEffectiveness = 1f;
            int hit = 1;

            for (int i = 1; i <= hitCount; i++)
            {
                DamageDetails damageDetails = new();

                sourceUnit.PlayAttackAnimation();
                AudioManager.Instance.PlaySFX(move.Base.Sound);
                yield return new WaitForSeconds(0.75f);
                targetUnit.PlayHitAnimation();
                AudioManager.Instance.PlaySFX(AudioID.Hit);

                if (move.Base.Category == MoveCategory.Status)
                {
                    yield return RunMoveEffects(move.Base.Effects, sourceUnit.Monster, targetUnit.Monster, move.Base.Target);
                }
                else
                {
                    damageDetails = targetUnit.Monster.TakeDamage(move, sourceUnit.Monster, _field.Weather);
                    yield return targetUnit.Hud.WaitForHPUpdate();
                    yield return ShowDamageDetails(damageDetails);
                    typeEffectiveness = damageDetails.TypeEffectiveness;
                }

                if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0 && targetUnit.Monster.Hp > 0)
                {
                    foreach (SecondaryEffects effect in move.Base.SecondaryEffects)
                    {
                        int rnd = Random.Range(1, 101);
                        if (rnd <= effect.Chance)
                        {
                            yield return RunMoveEffects(effect, sourceUnit.Monster, targetUnit.Monster, effect.Target);
                        }
                    }
                }
                yield return RunAfterMove(damageDetails, move.Base, sourceUnit, targetUnit);

                hit = i;
                if (targetUnit.Monster.Hp <= 0 || sourceUnit.Monster.Hp <= 0)
                {
                    break;
                }
            }

            if (move.Base.Category != MoveCategory.Status)
            {
                yield return ShowEffectiveness(typeEffectiveness);
            }

            if (hit > 1)
            {
                yield return _dialogueBox.TypeDialogue($"Hit {hit} times!");
            }

            if (targetUnit.Monster.Hp <= 0)
            {
                yield return HandleMonsterDefeat(targetUnit);
            }

            if (sourceUnit.Monster.Hp <= 0)
            {
                yield return HandleMonsterDefeat(sourceUnit);
            }
        }
        else
        {
            yield return _dialogueBox.TypeDialogue($"{sourceUnit.Monster.Base.Name}'s attack missed!");
        }
    }

    private IEnumerator RunAfterMove(DamageDetails details, MoveBase move, BattleUnit sourceUnit, BattleUnit targetUnit)
    {
        if (details == null)
        {
            yield break;
        }

        if (move.Recoil.RecoilType != RecoilType.None)
        {
            int damage;

            switch (move.Recoil.RecoilType)
            {
                case RecoilType.RecoilByMaxHP:
                    int maxHp = sourceUnit.Monster.MaxHp;

                    damage = Mathf.FloorToInt(maxHp * (move.Recoil.RecoilDamage / 100f));
                    sourceUnit.Monster.TakeRecoilDamage(damage);
                    break;
                case RecoilType.RecoilByCurrentHP:
                    int currentHp = sourceUnit.Monster.Hp;

                    damage = Mathf.FloorToInt(currentHp * (move.Recoil.RecoilDamage / 100f));
                    sourceUnit.Monster.TakeRecoilDamage(damage);
                    break;
                case RecoilType.RecoilByDamage:
                    damage = Mathf.FloorToInt(details.ActualDamageDealt * (move.Recoil.RecoilDamage / 100f));
                    sourceUnit.Monster.TakeRecoilDamage(damage);
                    break;
                case RecoilType.None:
                    break;
                default:
                    break;
            }
        }

        if (move.DrainPercentage != 0 && sourceUnit.Monster.Hp != sourceUnit.Monster.MaxHp)
        {
            int heal = Mathf.Clamp(Mathf.CeilToInt(details.ActualDamageDealt / 100f * move.DrainPercentage), 1, sourceUnit.Monster.MaxHp);
            string updatedTargetUnitName = targetUnit.Monster.Base.Name;

            if (targetUnit.Monster == _enemyUnit.Monster)
            {
                updatedTargetUnitName = _isMasterBattle ? $"the enemy {targetUnit.Monster.Base.Name}" : $"the wild {targetUnit.Monster.Base.Name}";
            }

            sourceUnit.Monster.DrainHealth(heal, updatedTargetUnitName);
        }

        yield return ShowStatusChanges(sourceUnit.Monster);
        yield return ShowStatusChanges(targetUnit.Monster);
    }

    private IEnumerator RunMoveEffects(MoveEffects effects, Monster source, string sourceName, Monster target, string targetName, MoveTarget moveTarget)
    {
        // Stat Boosts
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }

        // Status Conditions
        if (effects.Status != ConditionID.None)
        {
            target.SetStatus(effects.Status);
        }
        if (effects.VolatileStatus != ConditionID.None)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        // Weather
        if (effects.Weather != ConditionID.None)
        {
            _field.SetWeather(effects.Weather);
            _field.WeatherDuration = 5;
            yield return _dialogueBox.TypeDialogue(_field.Weather.StartMessage);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    private IEnumerator ShowStatusChanges(Monster monster)
    {
        while (monster.StatusChanges.Count > 0)
        {
            string message = monster.StatusChanges.Dequeue();

            yield return _dialogueBox.TypeDialogue($"{monster.Base.Name}{message}");
        }
    }

    private IEnumerator HandleMonsterDefeat(BattleUnit defeatedUnit)
    {
        yield return _dialogueBox.TypeDialogue($"{defeatedUnit.Monster.Base.Name} has been defeated!");
        defeatedUnit.PlayDefeatAnimation();
        yield return new WaitForSeconds(1f);

        if (!defeatedUnit.IsPlayerUnit)
        {
            bool battleWon = true;

            if (_isMasterBattle)
            {
                battleWon = _enemyParty.GetHealthyMonster() == null;
            }
            if (battleWon)
            {
                AudioManager.Instance.PlayMusic(_battleSystem.BattleVictoryMusic);
            }

            int enemyLevel = defeatedUnit.Monster.Level;
            float masterBonus = _isMasterBattle ? 1.5f : 1f;

            List<string> lootDescriptions = new();

            int gpYield = defeatedUnit.Monster.Base.CalculateGpYield();
            int gpDropped = Mathf.FloorToInt(gpYield * enemyLevel * masterBonus / 7);

            if (gpDropped > 0)
            {
                lootDescriptions.Add($"{gpDropped} GP");
                Wallet.Instance.GetWallet().AddMoney(gpDropped);
            }

            DropTable dropTable = defeatedUnit.Monster.Base.DropTable;

            if (dropTable != null)
            {
                foreach (ItemDrop itemDrop in dropTable.ItemDrops)
                {
                    if (Random.Range(1, 101) <= itemDrop.DropChance)
                    {
                        int quantity = Random.Range(itemDrop.QuantityRange.x, itemDrop.QuantityRange.y + 1);

                        if (quantity > 0)
                        {
                            lootDescriptions.Add($"{quantity}x {itemDrop.Item.Name}");
                            Inventory.GetInventory().AddItem(itemDrop.Item, quantity);
                        }
                    }
                }
            }

            if (lootDescriptions.Count > 0)
            {
                string initialMessage = $"{defeatedUnit.Monster.Base.Name} dropped";

                foreach (string loot in lootDescriptions)
                {
                    yield return loot != lootDescriptions.First()
                        ? _dialogueBox.SetAndTypeDialogue(initialMessage, loot)
                        : (object)_dialogueBox.TypeDialogue($"{initialMessage} {loot}");
                }
            }

            _playerUnit.Monster.GainPvs(defeatedUnit.Monster.Base.PvYield);

            int expYield = defeatedUnit.Monster.Base.ExpYield;
            int expGain = Mathf.FloorToInt(expYield * enemyLevel * masterBonus / 7);

            if (_playerUnit.Monster.Level < GlobalSettings.Instance.MaxLevel)
            {
                int maxExp = _playerUnit.Monster.Base.GetExpForLevel(GlobalSettings.Instance.MaxLevel);
                int expNeeded = maxExp - _playerUnit.Monster.Exp;

                expGain = Mathf.Min(expGain, expNeeded);
                _playerUnit.Monster.Exp += expGain;
                yield return _dialogueBox.TypeDialogue($"{_playerMonsterName} gained {expGain} experience!");
                yield return _playerUnit.Hud.SetExpSmooth();

                while (_playerUnit.Monster.CheckForLevelUp())
                {
                    _playerUnit.Monster.HasJustLeveledUp = true;
                    _playerUnit.Hud.SetLevel();
                    yield return _dialogueBox.TypeDialogue($"{_playerMonsterName} grew to level {_playerUnit.Monster.Level}!");

                    LearnableMove newMove = _playerUnit.Monster.GetLearnableMoveAtCurrentLevel();

                    if (newMove != null)
                    {
                        if (_playerUnit.Monster.Moves.Count < MonsterBase.MaxMoveCount)
                        {
                            _playerUnit.Monster.LearnMove(newMove.Base);
                            yield return _dialogueBox.TypeDialogue($"{_playerMonsterName} learned {newMove.Base.Name}!");
                            _dialogueBox.SetMoveNames(_playerUnit.Monster.Moves);
                        }
                        else
                        {
                            yield return _dialogueBox.TypeDialogue($"{_playerMonsterName} is trying to learn {newMove.Base.Name}!");
                            yield return _dialogueBox.TypeDialogue($"But {_playerMonsterName} already knows {MonsterBase.MaxMoveCount} moves!");
                            yield return _dialogueBox.TypeDialogue($"Choose a move to forget.");

                            ForgettingMoveState.Instance.CurrentMoves = _playerUnit.Monster.Moves.Select(static m => m.Base).ToList();
                            ForgettingMoveState.Instance.NewMove = newMove.Base;
                            yield return GameController.Instance.StateMachine.PushAndWait(ForgettingMoveState.Instance);

                            int moveIndex = ForgettingMoveState.Instance.Selection;

                            if (moveIndex == MonsterBase.MaxMoveCount || moveIndex == -1)
                            {
                                yield return _dialogueBox.TypeDialogue($"{_playerMonsterName} did not learn {newMove.Base.Name}!");
                            }
                            else
                            {
                                Move selectedMove = _playerUnit.Monster.Moves[moveIndex];

                                yield return _dialogueBox.TypeDialogue($"{_playerMonsterName} forgot {selectedMove.Base.Name} and learned {newMove.Base.Name}!");
                                _playerUnit.Monster.Moves[moveIndex] = new Move(newMove.Base);
                            }
                        }
                    }
                    yield return _playerUnit.Hud.SetExpSmooth(true);
                }
                yield return new WaitForSeconds(0.75f);
            }
        }

        yield return CheckForBattleOver(defeatedUnit);
    }

    private IEnumerator CheckForBattleOver(BattleUnit defeatedUnit)
    {
        if (defeatedUnit.IsPlayerUnit)
        {
            Monster nextMonster = _playerParty.GetHealthyMonster();
            if (nextMonster != null)
            {
                yield return GameController.Instance.StateMachine.PushAndWait(PartyState.Instance);
                yield return _battleSystem.SwitchMonster(PartyState.Instance.SelectedMonster);
            }
            else
            {
                _battleSystem.BattleOver(false);
            }
        }
        else
        {
            if (!_isMasterBattle)
            {
                _battleSystem.BattleOver(true);
            }
            else
            {
                Monster nextMonster = _enemyParty.GetHealthyMonster();
                if (nextMonster != null)
                {
                    StartCoroutine(_battleSystem.SendNextMasterMonster());
                }
                else
                {
                    _battleSystem.BattleOver(true);
                }
            }
        }
    }

    private IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return _dialogueBox.TypeDialogue("A critical hit!");
        }
    }

    private IEnumerator ShowEffectiveness(float typeEffectiveness)
    {
        if (typeEffectiveness > 1f)
        {
            yield return _dialogueBox.TypeDialogue("It's super effective!");
        }
        else if (typeEffectiveness > 0f)
        {
            yield return _dialogueBox.TypeDialogue("It's not very effective!");
        }
        else if (typeEffectiveness == 0f)
        {
            yield return _dialogueBox.TypeDialogue("It has no effect!");
        }
    }

    private IEnumerator AttemptEscape()
    {
        if (_isMasterBattle)
        {
            yield return _dialogueBox.TypeDialogue("You can't run from a Master battle!");
            yield break;
        }

        ++_battleSystem.EscapeAttempts;

        int playerAgility = _playerUnit.Monster.Agility;
        int enemyAgility = _enemyUnit.Monster.Agility;

        if (playerAgility >= enemyAgility)
        {
            yield return _dialogueBox.TypeDialogue("You got away safely!");
            _battleSystem.BattleOver(true);
        }
        else
        {
            float f = ((playerAgility * 128 / enemyAgility) + (30 * _battleSystem.EscapeAttempts)) % 256;

            if (Random.Range(0, 256) < f)
            {
                yield return _dialogueBox.TypeDialogue("You got away safely!");
                _battleSystem.BattleOver(true);
            }
            else
            {
                yield return _dialogueBox.TypeDialogue("You couldn't get away!");
            }
        }
    }
}
