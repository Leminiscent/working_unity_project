using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;

public class RunTurnState : State<BattleSystem>
{
    private BattleSystem _battleSystem;
    private BattleDialogueBox _dialogueBox;
    private BattleParty _playerParty;
    private BattleParty _enemyParty;
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
        _dialogueBox = _battleSystem.DialogueBox;
        _playerParty = _battleSystem.PlayerParty;
        _enemyParty = _battleSystem.EnemyParty;
        _isMasterBattle = _battleSystem.IsMasterBattle;
        _field = _battleSystem.Field;

        StartCoroutine(RunTurns());
    }

    private IEnumerator RunTurns()
    {
        foreach (BattleAction action in BattleActions)
        {
            if (!action.IsValid)
            {
                continue;
            }

            if (action.ActionType == BattleActionType.Fight)
            {
                action.SourceUnit.Battler.CurrentMove = action.SelectedMove;
                yield return RunMove(action.SourceUnit, action.TargetUnits, action.SelectedMove);
            }
            else if (action.ActionType == BattleActionType.Talk)
            {
                RecruitmentState.Instance.RecruitTarget = action.TargetUnits[0];
                yield return _battleSystem.StateMachine.PushAndWait(RecruitmentState.Instance);
            }
            else if (action.ActionType == BattleActionType.UseItem)
            {
                yield return UseItem(action.SourceUnit, action.TargetUnits, action.SelectedItem);
            }
            else if (action.ActionType == BattleActionType.Guard)
            {
                StartCoroutine(action.SourceUnit.StartGuarding());
                yield return _dialogueBox.TypeDialogue($"{action.SourceUnit.Battler.Base.Name} has begun guarding!");
            }
            else if (action.ActionType == BattleActionType.SwitchBattler)
            {
                yield return _battleSystem.SwitchBattler(action.SelectedBattler, action.SourceUnit);
            }
            else if (action.ActionType == BattleActionType.Run)
            {
                yield return AttemptEscape();
            }

            if (_battleSystem.BattleIsOver)
            {
                yield break;
            }
        }

        List<BattleUnit> agilitySortedUnits = _battleSystem.PlayerUnits.Concat(_battleSystem.EnemyUnits).OrderByDescending(static u => u.Battler.Agility).ToList();

        if (_field.Weather != null)
        {
            yield return _dialogueBox.TypeDialogue(_field.Weather.EffectMessage);

            foreach (BattleUnit unit in agilitySortedUnits)
            {
                _field.Weather.OnWeather?.Invoke(unit.Battler);
                yield return ShowStatusChanges(unit);
                yield return unit.Hud.WaitForHPUpdate();
                if (unit.Battler.Hp <= 0)
                {
                    yield return HandleUnitDefeat(unit);
                }
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

        foreach (BattleUnit unit in agilitySortedUnits)
        {
            yield return RunAfterTurn(unit);
        }

        _battleSystem.ClearBattleActions();

        foreach (BattleUnit unit in _battleSystem.PlayerUnits.Concat(_battleSystem.EnemyUnits))
        {
            if (unit.Battler.IsGuarding)
            {
                StartCoroutine(unit.StopGuarding());
            }
        }

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

        sourceUnit.Battler.OnEndOfTurn();
        yield return ShowStatusChanges(sourceUnit);
        if (sourceUnit.Battler.Hp <= 0)
        {
            yield return HandleUnitDefeat(sourceUnit);
        }
    }

    private bool CheckIfMoveHits(Move move, Battler source, Battler target)
    {
        if (move.Base.AlwaysHits)
        {
            return true;
        }

        if (move.Base.OneHitKO.IsOneHitKO)
        {
            if (source.Level < target.Level || target.HasType(move.Base.OneHitKO.ImmunityType))
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

    private IEnumerator RunMove(BattleUnit sourceUnit, List<BattleUnit> targetUnits, Move move)
    {
        bool canRunMove = sourceUnit.Battler.OnStartOfTurn();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }

        yield return ShowStatusChanges(sourceUnit);

        move.Sp--;

        if (move.Base == GlobalSettings.Instance.BackupMove)
        {
            yield return _dialogueBox.TypeDialogue($"{sourceUnit.Battler.Base.Name} has no SP left!");
        }

        yield return _dialogueBox.TypeDialogue($"{sourceUnit.Battler.Base.Name} used {move.Base.Name}!");
        yield return sourceUnit.PlayMoveCastAnimation(move.Base);

        List<BattleUnit> targetUnitsCopy = new(targetUnits);
        foreach (BattleUnit targetUnit in targetUnitsCopy)
        {
            if (sourceUnit.Battler.Hp <= 0)
            {
                yield break;
            }

            if (CheckIfMoveHits(move, sourceUnit.Battler, targetUnit.Battler))
            {
                int hitCount = move.Base.GetHitCount();
                float typeEffectiveness = TypeChart.GetEffectiveness(move.Base.Type, targetUnit.Battler.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, targetUnit.Battler.Base.Type2);
                int hit = 1;

                for (int i = 1; i <= hitCount; i++)
                {
                    DamageDetails damageDetails = new();
                    yield return targetUnit.PlayMoveEffectAnimation(move.Base);

                    if (move.Base.Category == MoveCategory.Status)
                    {
                        yield return typeEffectiveness > 0f
                            ? RunMoveEffects(move.Base.Effects, sourceUnit, targetUnit, move.Base.Target)
                            : _dialogueBox.TypeDialogue("It has no effect!");
                    }
                    else
                    {
                        damageDetails = targetUnit.Battler.TakeDamage(move, sourceUnit.Battler, _field.Weather);
                        StartCoroutine(targetUnit.PlayDamageAnimation());
                        yield return targetUnit.Hud.WaitForHPUpdate();
                        yield return ShowDamageDetails(damageDetails);
                        typeEffectiveness = damageDetails.TypeEffectiveness;
                    }

                    if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0 && typeEffectiveness > 0 && targetUnit.Battler.Hp > 0)
                    {
                        foreach (SecondaryEffects effect in move.Base.SecondaryEffects)
                        {
                            int rnd = Random.Range(1, 101);
                            if (rnd <= effect.Chance)
                            {
                                yield return RunMoveEffects(effect, sourceUnit, targetUnit, effect.Target);
                            }
                        }
                    }

                    yield return RunAfterMove(damageDetails, move.Base, sourceUnit, targetUnit);
                    hit = i;

                    if (targetUnit.Battler.Hp <= 0 || sourceUnit.Battler.Hp <= 0)
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

                if (targetUnit.Battler.Hp <= 0)
                {
                    yield return HandleUnitDefeat(targetUnit);
                }

                if (sourceUnit.Battler.Hp <= 0)
                {
                    yield return HandleUnitDefeat(sourceUnit);
                }
            }
            else
            {
                yield return _dialogueBox.TypeDialogue($"{sourceUnit.Battler.Base.Name}'s attack missed!");
            }
        }
    }

    private IEnumerator RunAfterMove(DamageDetails details, MoveBase move, BattleUnit sourceUnit, BattleUnit targetUnit)
    {
        if (details == null || details.TypeEffectiveness == 0f)
        {
            yield break;
        }

        if (move.Recoil.RecoilType != RecoilType.None)
        {
            int damage;

            switch (move.Recoil.RecoilType)
            {
                case RecoilType.RecoilByMaxHP:
                    int maxHp = sourceUnit.Battler.MaxHp;
                    damage = Mathf.FloorToInt(maxHp * (move.Recoil.RecoilDamage / 100f));
                    sourceUnit.Battler.TakeRecoilDamage(damage);
                    break;
                case RecoilType.RecoilByCurrentHP:
                    int currentHp = sourceUnit.Battler.Hp;
                    damage = Mathf.FloorToInt(currentHp * (move.Recoil.RecoilDamage / 100f));
                    sourceUnit.Battler.TakeRecoilDamage(damage);
                    break;
                case RecoilType.RecoilByDamage:
                    damage = Mathf.FloorToInt(details.ActualDamageDealt * (move.Recoil.RecoilDamage / 100f));
                    sourceUnit.Battler.TakeRecoilDamage(damage);
                    break;
                case RecoilType.None:
                    break;
                default:
                    break;
            }
        }

        if (move.DrainPercentage != 0 && sourceUnit.Battler.Hp != sourceUnit.Battler.MaxHp)
        {
            int heal = Mathf.Clamp(Mathf.CeilToInt(details.ActualDamageDealt / 100f * move.DrainPercentage), 1, sourceUnit.Battler.MaxHp);
            sourceUnit.Battler.DrainHealth(heal, targetUnit.Battler.Base.Name);
        }

        yield return ShowStatusChanges(sourceUnit);
        yield return ShowStatusChanges(targetUnit);
    }

    private IEnumerator RunMoveEffects(MoveEffects effects, BattleUnit sourceUnit, BattleUnit targetUnit, MoveTarget moveTarget)
    {
        // Stat Boosts
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
            {
                sourceUnit.Battler.ApplyBoosts(effects.Boosts);
            }
            else
            {
                targetUnit.Battler.ApplyBoosts(effects.Boosts);
            }
        }

        // Status Conditions
        if (effects.Status != ConditionID.None)
        {
            if (targetUnit.Battler.Statuses.ContainsKey(effects.Status))
            {
                yield return _dialogueBox.TypeDialogue($"{targetUnit.Battler.Base.Name} {ConditionsDB.Conditions[effects.Status].FailMessage}");
            }
            else
            {
                targetUnit.Battler.SetStatus(effects.Status);
            }
        }
        if (effects.VolatileStatus != ConditionID.None)
        {
            if (targetUnit.Battler.VolatileStatuses.ContainsKey(effects.VolatileStatus))
            {
                yield return _dialogueBox.TypeDialogue($"{targetUnit.Battler.Base.Name} {ConditionsDB.Conditions[effects.VolatileStatus].FailMessage}");
            }
            else
            {
                targetUnit.Battler.SetVolatileStatus(effects.VolatileStatus);
            }
        }

        // Weather
        if (effects.Weather != ConditionID.None)
        {
            _field.SetWeather(effects.Weather);
            _field.WeatherDuration = 5;
            yield return _dialogueBox.TypeDialogue(_field.Weather.StartMessage);
        }

        yield return ShowStatusChanges(sourceUnit);
        yield return ShowStatusChanges(targetUnit);
    }

    private IEnumerator ShowStatusChanges(BattleUnit unit)
    {
        while (unit.Battler.StatusChanges.Count > 0)
        {
            StatusEvent statusEvent = unit.Battler.StatusChanges.Dequeue();

            if (statusEvent.Type == StatusEventType.Damage)
            {
                unit.Battler.DecreaseHP((int)statusEvent.Value);
                StartCoroutine(unit.PlayDamageAnimation());
            }
            else if (statusEvent.Type == StatusEventType.Heal)
            {
                unit.Battler.IncreaseHP((int)statusEvent.Value);
                StartCoroutine(unit.PlayHealAnimation());
            }
            else if (statusEvent.Type == StatusEventType.SetCondition)
            {
                StartCoroutine(unit.PlayStatusSetAnimation());
            }
            else if (statusEvent.Type == StatusEventType.CureCondition)
            {
                StartCoroutine(unit.PlayStatusCureAnimation());
            }
            else if (statusEvent.Type == StatusEventType.StatBoost)
            {
                unit.Hud.UpdateStatBoosts();

                string statName = statusEvent.Message[3..].Split(' ')[0];
                if (System.Enum.TryParse(statName, out Stat stat))
                {
                    if (statusEvent.Value > 0)
                    {
                        StartCoroutine(unit.PlayStatGainAnimation(stat));
                    }
                    else
                    {
                        StartCoroutine(unit.PlayStatLossAnimation(stat));
                    }
                }
            }

            yield return _dialogueBox.TypeDialogue($"{unit.Battler.Base.Name}{statusEvent.Message}");
        }
    }

    private IEnumerator UseItem(BattleUnit sourceUnit, List<BattleUnit> targetUnits, ItemBase item)
    {
        bool canUseItem = sourceUnit.Battler.OnStartOfTurn();
        if (!canUseItem)
        {
            yield return ShowStatusChanges(sourceUnit);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit);

        Inventory inventory = Inventory.GetInventory();
        if (inventory.GetItemCount(item) == 0)
        {
            yield return _dialogueBox.TypeDialogue($"There are no {item.Name}s remaining to use!");
            yield break;
        }
        StartCoroutine(sourceUnit.PlayMoveCastAnimation()); // TODO: Decouple move and item casting animations
        yield return _dialogueBox.TypeDialogue($"{sourceUnit.Battler.Base.Name} used {item.Name}!");

        List<BattleUnit> targetUnitsCopy = new(targetUnits);
        foreach (BattleUnit targetUnit in targetUnitsCopy)
        {
            bool itemUsed = item.Use(targetUnit.Battler);
            if (itemUsed)
            {
                if (item is RecoveryItem)
                {
                    yield return _dialogueBox.TypeDialogue($"{targetUnit.Battler.Base.Name} {item.Message}!");
                }
            }
            else
            {
                if (item is RecoveryItem)
                {
                    yield return _dialogueBox.TypeDialogue($"The {item.Name} didn't have any effect on {targetUnit.Battler.Base.Name}!");
                }
            }
        }

        if (!item.IsReusable)
        {
            inventory.RemoveItem(item);
        }
    }

    private IEnumerator HandleUnitDefeat(BattleUnit defeatedUnit)
    {
        StartCoroutine(defeatedUnit.PlayDefeatAnimation());
        yield return _dialogueBox.TypeDialogue($"{defeatedUnit.Battler.Base.Name} has been defeated!");

        if (!defeatedUnit.IsPlayerUnit)
        {
            int enemyLevel = defeatedUnit.Battler.Level;
            float masterBonus = _isMasterBattle ? 1.5f : 1f;

            List<string> lootDescriptions = new();

            int gpYield = defeatedUnit.Battler.Base.CalculateGpYield();
            int gpDropped = Mathf.FloorToInt(gpYield * enemyLevel * masterBonus / 7);

            if (gpDropped > 0)
            {
                lootDescriptions.Add($"{gpDropped} GP");
                Wallet.Instance.GetWallet().AddMoney(gpDropped);
            }

            DropTable dropTable = defeatedUnit.Battler.Base.DropTable;

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
                string initialMessage = $"{defeatedUnit.Battler.Base.Name} dropped";

                AudioManager.Instance.PlaySFX(AudioID.ItemObtained);
                foreach (string loot in lootDescriptions)
                {
                    yield return loot != lootDescriptions.First() & loot != lootDescriptions.Last() ? _dialogueBox.TypeDialogue(loot, setDialogue: initialMessage, clearDialogue: false)
                        : loot == lootDescriptions.First() ? _dialogueBox.TypeDialogue($"{initialMessage} {loot}", clearDialogue: false)
                        : _dialogueBox.TypeDialogue(loot, setDialogue: initialMessage);
                }
            }

            int expYield = defeatedUnit.Battler.Base.ExpYield;
            int expGain = Mathf.FloorToInt(expYield * enemyLevel * masterBonus / 7) / _battleSystem.PlayerUnits.Count;

            for (int i = 0; i < _battleSystem.PlayerUnits.Count; i++)
            {
                BattleUnit playerUnit = _battleSystem.PlayerUnits[i];
                playerUnit.Battler.GainPvs(defeatedUnit.Battler.Base.PvYield);

                if (playerUnit.Battler.Level < GlobalSettings.Instance.MaxLevel)
                {
                    int maxExp = playerUnit.Battler.Base.GetExpForLevel(GlobalSettings.Instance.MaxLevel);
                    int expNeeded = maxExp - playerUnit.Battler.Exp;

                    expGain = Mathf.Min(expGain, expNeeded);
                    playerUnit.Battler.Exp += expGain;
                    StartCoroutine(playerUnit.PlayExpGainAnimation());
                    yield return playerUnit.Hud.SetExpSmooth();
                    yield return _dialogueBox.TypeDialogue($"{playerUnit.Battler.Base.Name} gained {expGain} experience!");

                    while (playerUnit.Battler.CheckForLevelUp())
                    {
                        playerUnit.Battler.HasJustLeveledUp = true;
                        playerUnit.Hud.SetLevel();
                        StartCoroutine(playerUnit.PlayLevelUpAnimation());
                        yield return _dialogueBox.TypeDialogue($"{playerUnit.Battler.Base.Name} grew to level {playerUnit.Battler.Level}!");

                        LearnableMove newMove = playerUnit.Battler.GetLearnableMoveAtCurrentLevel();
                        if (newMove != null)
                        {
                            if (playerUnit.Battler.Moves.Count < BattlerBase.MaxMoveCount)
                            {
                                playerUnit.Battler.LearnMove(newMove.Base);
                                yield return _dialogueBox.TypeDialogue($"{playerUnit.Battler.Base.Name} learned {newMove.Base.Name}!");
                                _dialogueBox.SetMoveNames(playerUnit.Battler.Moves);
                            }
                            else
                            {
                                yield return _dialogueBox.TypeDialogue($"{playerUnit.Battler.Base.Name} is trying to learn {newMove.Base.Name}!");
                                yield return _dialogueBox.TypeDialogue($"But {playerUnit.Battler.Base.Name} already knows {BattlerBase.MaxMoveCount} moves!");
                                yield return _dialogueBox.TypeDialogue($"Choose a move to forget.");

                                ForgettingMoveState.Instance.CurrentMoves = playerUnit.Battler.Moves.Select(static m => m.Base).ToList();
                                ForgettingMoveState.Instance.NewMove = newMove.Base;
                                yield return GameController.Instance.StateMachine.PushAndWait(ForgettingMoveState.Instance);

                                int moveIndex = ForgettingMoveState.Instance.Selection;

                                if (moveIndex == BattlerBase.MaxMoveCount || moveIndex == -1)
                                {
                                    yield return _dialogueBox.TypeDialogue($"{playerUnit.Battler.Base.Name} did not learn {newMove.Base.Name}!");
                                }
                                else
                                {
                                    Move selectedMove = playerUnit.Battler.Moves[moveIndex];

                                    yield return _dialogueBox.TypeDialogue($"{playerUnit.Battler.Base.Name} forgot {selectedMove.Base.Name} and learned {newMove.Base.Name}!");
                                    playerUnit.Battler.Moves[moveIndex] = new Move(newMove.Base);
                                }
                            }
                        }
                        yield return playerUnit.Hud.SetExpSmooth(true);
                    }
                    yield return new WaitForSeconds(0.75f);
                }
            }
        }

        yield return CheckForBattleOver(defeatedUnit);
    }

    private IEnumerator CheckForBattleOver(BattleUnit defeatedUnit)
    {
        BattleAction defeatedUnitAction = BattleActions.FirstOrDefault(a => a.SourceUnit == defeatedUnit);

        if (defeatedUnitAction != null)
        {
            defeatedUnitAction.IsValid = false;
        }

        if (defeatedUnit.IsPlayerUnit)
        {
            List<Battler> activeBattlers = _battleSystem.PlayerUnits.Select(static u => u.Battler).Where(b => b.Hp > 0).ToList();
            Battler nextBattler = _playerParty.GetHealthyBattlers(excludedBattlers: activeBattlers);

            if (nextBattler == null && activeBattlers.Count == 0)
            {
                yield return new WaitForSeconds(0.5f);
                AudioManager.Instance.PlayMusic(_battleSystem.BattleLostMusic, loop: false);
                yield return _dialogueBox.TypeDialogue("All allies have been defeated!");
                yield return _dialogueBox.TypeDialogue("The battle is lost...", clearDialogue: false);
                while (AudioManager.Instance.MusicPlayer.isPlaying)
                {
                    yield return null;
                }
                _battleSystem.BattleOver(false);
            }
            else if (nextBattler == null && activeBattlers.Count > 0)
            {
                _battleSystem.PlayerUnits.Remove(defeatedUnit);
                defeatedUnit.ClearData();

                List<BattleAction> actionsToAdjust = BattleActions.Where(a => a.TargetUnits != null && a.TargetUnits.Contains(defeatedUnit)).ToList();
                foreach (BattleAction a in actionsToAdjust)
                {
                    a.TargetUnits.Remove(defeatedUnit);
                    if (a.TargetUnits.Count == 0)
                    {
                        a.TargetUnits.Add(_battleSystem.PlayerUnits[Random.Range(0, _battleSystem.PlayerUnits.Count)]);
                    }
                }
            }
            else if (nextBattler != null)
            {
                yield return GameController.Instance.StateMachine.PushAndWait(PartyState.Instance);
                yield return _battleSystem.SwitchBattler(PartyState.Instance.SelectedMember, defeatedUnit);
            }
        }
        else
        {
            List<Battler> activeBattlers = _battleSystem.EnemyUnits.Select(static u => u.Battler).Where(b => b.Hp > 0).ToList();

            if (!_isMasterBattle)
            {
                if (activeBattlers.Count == 0)
                {
                    yield return new WaitForSeconds(0.5f);
                    AudioManager.Instance.PlayMusic(_battleSystem.BattleWonMusic, loop: false);
                    yield return _dialogueBox.TypeDialogue("All enemies have been defeated!");
                    yield return _dialogueBox.TypeDialogue("You are victorious!", clearDialogue: false);
                    while (AudioManager.Instance.MusicPlayer.isPlaying)
                    {
                        yield return null;
                    }
                    _battleSystem.BattleOver(true);
                }
                else
                {
                    _battleSystem.EnemyUnits.Remove(defeatedUnit);
                    defeatedUnit.ClearData();

                    List<BattleAction> actionsToAdjust = BattleActions.Where(a => a.TargetUnits != null && a.TargetUnits.Contains(defeatedUnit)).ToList();
                    foreach (BattleAction a in actionsToAdjust)
                    {
                        a.TargetUnits.Remove(defeatedUnit);
                        if (a.TargetUnits.Count == 0)
                        {
                            a.TargetUnits.Add(_battleSystem.EnemyUnits[Random.Range(0, _battleSystem.EnemyUnits.Count)]);
                        }
                    }
                }
            }
            else
            {
                Battler nextBattler = _enemyParty.GetHealthyBattlers(excludedBattlers: activeBattlers);

                if (nextBattler == null && activeBattlers.Count == 0)
                {
                    yield return new WaitForSeconds(0.5f);
                    AudioManager.Instance.PlayMusic(_battleSystem.BattleWonMusic, loop: false);
                    yield return _dialogueBox.TypeDialogue("All enemies have been defeated!");
                    yield return _dialogueBox.TypeDialogue("You are victorious!", clearDialogue: false);
                    while (AudioManager.Instance.MusicPlayer.isPlaying)
                    {
                        yield return null;
                    }
                    _battleSystem.BattleOver(true);
                }
                else if (nextBattler == null && activeBattlers.Count > 0)
                {
                    _battleSystem.EnemyUnits.Remove(defeatedUnit);
                    defeatedUnit.ClearData();

                    List<BattleAction> actionsToAdjust = BattleActions.Where(a => a.TargetUnits != null && a.TargetUnits.Contains(defeatedUnit)).ToList();
                    foreach (BattleAction a in actionsToAdjust)
                    {
                        a.TargetUnits.Remove(defeatedUnit);
                        if (a.TargetUnits.Count == 0)
                        {
                            a.TargetUnits.Add(_battleSystem.EnemyUnits[Random.Range(0, _battleSystem.EnemyUnits.Count)]);
                        }
                    }
                }
                else if (nextBattler != null)
                {
                    yield return _battleSystem.SendNextMasterBattler(nextBattler, defeatedUnit);
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

        int minPlayerAgility = _battleSystem.PlayerUnits.Min(static u => u.Battler.Agility);
        int maxEnemyAgility = _battleSystem.EnemyUnits.Max(static u => u.Battler.Agility);

        if (minPlayerAgility >= maxEnemyAgility)
        {
            AudioManager.Instance.PlayMusic(_battleSystem.BattleFledMusic, loop: false);
            yield return _dialogueBox.TypeDialogue("You got away safely!", clearDialogue: false);
            while (AudioManager.Instance.MusicPlayer.isPlaying)
            {
                yield return null;
            }
            _battleSystem.BattleOver(true);
        }
        else
        {
            float f = ((minPlayerAgility * 128 / maxEnemyAgility) + (30 * _battleSystem.EscapeAttempts)) % 256;

            if (Random.Range(0, 256) < f)
            {
                AudioManager.Instance.PlayMusic(_battleSystem.BattleFledMusic, loop: false);
                yield return _dialogueBox.TypeDialogue("You got away safely!", clearDialogue: false);
                while (AudioManager.Instance.MusicPlayer.isPlaying)
                {
                    yield return null;
                }
                _battleSystem.BattleOver(true);
            }
            else
            {
                yield return _dialogueBox.TypeDialogue("You couldn't get away!");
            }
        }
    }
}