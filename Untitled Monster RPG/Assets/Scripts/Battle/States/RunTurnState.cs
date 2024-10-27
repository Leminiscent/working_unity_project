using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;

public class RunTurnState : State<BattleSystem>
{
    BattleSystem battleSystem;
    BattleUnit playerUnit;
    BattleUnit enemyUnit;
    BattleDialogueBox dialogueBox;
    PartyScreen partyScreen;
    Monster wildMonster;
    MonsterParty playerParty;
    MonsterParty enemyParty;
    bool isMasterBattle;
    Field field;


    public static RunTurnState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(BattleSystem owner)
    {
        battleSystem = owner;
        playerUnit = battleSystem.PlayerUnit;
        enemyUnit = battleSystem.EnemyUnit;
        dialogueBox = battleSystem.DialogueBox;
        partyScreen = battleSystem.PartyScreen;
        wildMonster = battleSystem.WildMonster;
        playerParty = battleSystem.PlayerParty;
        enemyParty = battleSystem.EnemyParty;
        isMasterBattle = battleSystem.IsMasterBattle;
        field = battleSystem.Field;

        StartCoroutine(RunTurns(battleSystem.SelectedAction));
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        if (playerAction == BattleAction.Fight)
        {
            playerUnit.Monster.CurrentMove = (battleSystem.SelectedMove != -1) ? playerUnit.Monster.Moves[battleSystem.SelectedMove] : new Move(GlobalSettings.Instance.BackupMove);
            enemyUnit.Monster.CurrentMove = enemyUnit.Monster.GetRandomMove() ?? new Move(GlobalSettings.Instance.BackupMove);

            int playerMovePriority = playerUnit.Monster.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Monster.CurrentMove.Base.Priority;
            bool playerGoesFirst = true;

            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Monster.Agility >= enemyUnit.Monster.Agility;
            }

            var firstUnit = playerGoesFirst ? playerUnit : enemyUnit;
            var secondUnit = playerGoesFirst ? enemyUnit : playerUnit;
            var secondMonster = secondUnit.Monster;

            yield return RunMove(firstUnit, secondUnit, firstUnit.Monster.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (battleSystem.BattleIsOver) yield break;

            if (secondMonster.HP > 0)
            {
                yield return RunMove(secondUnit, firstUnit, secondUnit.Monster.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (battleSystem.BattleIsOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchMonster)
            {
                yield return battleSystem.SwitchMonster(battleSystem.SelectedMonster);
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return AttemptEscape();
            }

            enemyUnit.Monster.CurrentMove = enemyUnit.Monster.GetRandomMove() ?? new Move(GlobalSettings.Instance.BackupMove);
            yield return RunMove(enemyUnit, playerUnit, enemyUnit.Monster.CurrentMove);
            yield return RunAfterTurn(enemyUnit);
            if (battleSystem.BattleIsOver) yield break;
        }

        if (field.Weather != null)
        {
            yield return dialogueBox.TypeDialogue(field.Weather.EffectMessage);

            field.Weather.OnWeather?.Invoke(playerUnit.Monster);
            yield return ShowStatusChanges(playerUnit.Monster);
            yield return playerUnit.Hud.WaitForHPUpdate();
            if (playerUnit.Monster.HP <= 0)
            {
                yield return HandleMonsterDefeat(playerUnit);
            }

            field.Weather.OnWeather?.Invoke(enemyUnit.Monster);
            yield return ShowStatusChanges(enemyUnit.Monster);
            yield return enemyUnit.Hud.WaitForHPUpdate();
            if (enemyUnit.Monster.HP <= 0)
            {
                yield return HandleMonsterDefeat(enemyUnit);
            }

            if (field.WeatherDuration != null)
            {
                field.WeatherDuration--;
                if (field.WeatherDuration == 0)
                {
                    field.Weather = null;
                    field.WeatherDuration = null;
                    yield return dialogueBox.TypeDialogue("The weather has returned to normal.");
                }
            }
        }

        if (!battleSystem.BattleIsOver)
        {
            battleSystem.StateMachine.ChangeState(ActionSelectionState.Instance);
        }
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (battleSystem.BattleIsOver) yield break;

        sourceUnit.Monster.OnEndOfTurn();
        yield return ShowStatusChanges(sourceUnit.Monster);
        yield return sourceUnit.Hud.WaitForHPUpdate();
        if (sourceUnit.Monster.HP <= 0)
        {
            yield return HandleMonsterDefeat(sourceUnit);
        }
    }

    bool CheckIfMoveHits(Move move, Monster source, Monster target)
    {
        if (move.Base.AlwaysHits)
        {
            return true;
        }

        if (move.Base.OneHitKO.isOneHitKO)
        {
            if (source.Level < target.Level)
            {
                return false;
            }
            if (target.HasType(move.Base.OneHitKO.immunityType))
            {
                return false;
            }

            int baseAccuracy = move.Base.Accuracy;

            if (move.Base.OneHitKO.lowerOddsException)
            {
                baseAccuracy = source.HasType(move.Base.Type) ? baseAccuracy : baseAccuracy / 2;
            }

            int chance = source.Level - target.Level + baseAccuracy;

            return Random.Range(1, 101) <= chance;
        }

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];
        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

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

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Monster.OnStartOfTurn();

        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Monster);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }

        yield return ShowStatusChanges(sourceUnit.Monster);
        move.SP--;

        if (move.Base == GlobalSettings.Instance.BackupMove)
        {
            yield return dialogueBox.TypeDialogue($"{sourceUnit.Monster.Base.Name} has no SP left!");
        }
        yield return dialogueBox.TypeDialogue(sourceUnit.Monster.Base.Name + " used " + move.Base.Name + "!");

        if (CheckIfMoveHits(move, sourceUnit.Monster, targetUnit.Monster))
        {
            int hitCount = move.Base.GetHitCount();
            float typeEffectiveness = 1f;
            int hit = 1;

            for (int i = 1; i <= hitCount; i++)
            {
                var damageDetails = new DamageDetails();

                sourceUnit.PlayAttackAnimation();
                AudioManager.Instance.PlaySFX(move.Base.Sound);
                yield return new WaitForSeconds(1f);
                targetUnit.PlayHitAnimation();
                AudioManager.Instance.PlaySFX(AudioID.Hit);

                if (move.Base.Category == MoveCategory.Status)
                {
                    yield return RunMoveEffects(move.Base.Effects, sourceUnit.Monster, targetUnit.Monster, move.Base.Target);
                }
                else
                {
                    damageDetails = targetUnit.Monster.TakeDamage(move, sourceUnit.Monster, field.Weather);
                    yield return targetUnit.Hud.WaitForHPUpdate();
                    yield return ShowDamageDetails(damageDetails);
                    typeEffectiveness = damageDetails.TypeEffectiveness;
                }

                if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0 && targetUnit.Monster.HP > 0)
                {
                    foreach (var effect in move.Base.SecondaryEffects)
                    {
                        var rnd = Random.Range(1, 101);
                        if (rnd <= effect.Chance)
                        {
                            yield return RunMoveEffects(effect, sourceUnit.Monster, targetUnit.Monster, effect.Target);
                        }
                    }
                }
                yield return RunAfterMove(damageDetails, move.Base, sourceUnit, targetUnit);

                hit = i;
                if (targetUnit.Monster.HP <= 0) break;
            }
            yield return ShowEffectiveness(typeEffectiveness);

            if (hit > 1)
            {
                yield return dialogueBox.TypeDialogue($"Hit {hit} times!");
            }

            if (targetUnit.Monster.HP <= 0)
            {
                yield return HandleMonsterDefeat(targetUnit);
            }
        }
        else
        {
            yield return dialogueBox.TypeDialogue(sourceUnit.Monster.Base.Name + "'s attack missed!");
        }
    }

    IEnumerator RunAfterMove(DamageDetails details, MoveBase move, BattleUnit sourceUnit, BattleUnit targetUnit)
    {
        if (details == null)
            yield break;

        if (move.Recoil.recoilType != RecoilType.none)
        {
            int damage = 0;

            switch (move.Recoil.recoilType)
            {
                case RecoilType.RecoilByMaxHP:
                    int maxHp = sourceUnit.Monster.MaxHP;

                    damage = Mathf.FloorToInt(maxHp * (move.Recoil.recoilDamage / 100f));
                    sourceUnit.Monster.TakeRecoilDamage(damage);
                    break;
                case RecoilType.RecoilByCurrentHP:
                    int currentHp = sourceUnit.Monster.HP;

                    damage = Mathf.FloorToInt(currentHp * (move.Recoil.recoilDamage / 100f));
                    sourceUnit.Monster.TakeRecoilDamage(damage);
                    break;
                case RecoilType.RecoilByDamage:
                    damage = Mathf.FloorToInt(details.ActualDamageDealt * (move.Recoil.recoilDamage / 100f));
                    sourceUnit.Monster.TakeRecoilDamage(damage);
                    break;
                default:
                    break;
            }
        }

        if (move.DrainPercentage != 0 && sourceUnit.Monster.HP != sourceUnit.Monster.MaxHP)
        {
            int heal = Mathf.Clamp(Mathf.CeilToInt(details.ActualDamageDealt / 100f * move.DrainPercentage), 1, sourceUnit.Monster.MaxHP);

            sourceUnit.Monster.DrainHealth(heal, targetUnit.Monster);
        }

        yield return ShowStatusChanges(sourceUnit.Monster);
        yield return ShowStatusChanges(targetUnit.Monster);
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Monster source, Monster target, MoveTarget moveTarget)
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
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        // Weather
        if (effects.Weather != ConditionID.none)
        {
            field.SetWeather(effects.Weather);
            field.WeatherDuration = 5;
            yield return dialogueBox.TypeDialogue(field.Weather.StartMessage);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator ShowStatusChanges(Monster monster)
    {
        while (monster.StatusChanges.Count > 0)
        {
            var message = monster.StatusChanges.Dequeue();
            yield return dialogueBox.TypeDialogue(message);
        }
    }

    IEnumerator HandleMonsterDefeat(BattleUnit defeatedUnit)
    {
        yield return dialogueBox.TypeDialogue(defeatedUnit.Monster.Base.Name + " has been defeated!");
        defeatedUnit.PlayDefeatAnimation();
        yield return new WaitForSeconds(2f);

        if (!defeatedUnit.IsPlayerUnit)
        {
            bool battleWon = true;

            if (isMasterBattle)
            {
                battleWon = enemyParty.GetHealthyMonster() == null;
            }
            if (battleWon)
            {
                AudioManager.Instance.PlayMusic(battleSystem.BattleVictoryMusic);
            }

            int expYield = defeatedUnit.Monster.Base.ExpYield;
            int enemyLevel = defeatedUnit.Monster.Level;
            float masterBonus = isMasterBattle ? 1.5f : 1f;
            int expGain = Mathf.FloorToInt(expYield * enemyLevel * masterBonus / 7);

            if (playerUnit.Monster.Level < GlobalSettings.Instance.MaxLevel)
            {
                int maxExp = playerUnit.Monster.Base.GetExpForLevel(GlobalSettings.Instance.MaxLevel);
                int expNeeded = maxExp - playerUnit.Monster.Exp;

                expGain = Mathf.Min(expGain, expNeeded);
                playerUnit.Monster.Exp += expGain;
                yield return dialogueBox.TypeDialogue($"{playerUnit.Monster.Base.Name} gained {expGain} experience!");
                yield return playerUnit.Hud.SetExpSmooth();

                while (playerUnit.Monster.CheckForLevelUp())
                {
                    playerUnit.Hud.SetLevel();
                    yield return dialogueBox.TypeDialogue(playerUnit.Monster.Base.Name + " grew to level " + playerUnit.Monster.Level + "!");

                    var newMove = playerUnit.Monster.GetLearnableMoveAtCurrentLevel();

                    if (newMove != null)
                    {
                        if (playerUnit.Monster.Moves.Count < MonsterBase.MaxMoveCount)
                        {
                            playerUnit.Monster.LearnMove(newMove.Base);
                            yield return dialogueBox.TypeDialogue(playerUnit.Monster.Base.Name + " learned " + newMove.Base.Name + "!");
                            dialogueBox.SetMoveNames(playerUnit.Monster.Moves);
                        }
                        else
                        {
                            yield return dialogueBox.TypeDialogue($"{playerUnit.Monster.Base.Name} is trying to learn {newMove.Base.Name}!");
                            yield return dialogueBox.TypeDialogue($"But {playerUnit.Monster.Base.Name} already knows {MonsterBase.MaxMoveCount} moves!");
                            yield return dialogueBox.TypeDialogue($"Choose a move to forget.");

                            ForgettingMoveState.Instance.CurrentMoves = playerUnit.Monster.Moves.Select(m => m.Base).ToList();
                            ForgettingMoveState.Instance.NewMove = newMove.Base;
                            yield return GameController.Instance.StateMachine.PushAndWait(ForgettingMoveState.Instance);

                            int moveIndex = ForgettingMoveState.Instance.Selection;

                            if (moveIndex == MonsterBase.MaxMoveCount || moveIndex == -1)
                            {
                                yield return dialogueBox.TypeDialogue($"{playerUnit.Monster.Base.Name} did not learn {newMove.Base.Name}!");
                            }
                            else
                            {
                                var selectedMove = playerUnit.Monster.Moves[moveIndex];

                                yield return dialogueBox.TypeDialogue($"{playerUnit.Monster.Base.Name} forgot {selectedMove.Base.Name} and learned {newMove.Base.Name}!");
                                playerUnit.Monster.Moves[moveIndex] = new Move(newMove.Base);
                            }
                        }
                    }
                    yield return playerUnit.Hud.SetExpSmooth(true);
                }
                yield return new WaitForSeconds(1f);
            }

            playerUnit.Monster.GainPvs(defeatedUnit.Monster.Base.PvYield);

            var dropTable = defeatedUnit.Monster.Base.DropTable;

            if (dropTable != null)
            {
                int gpDropped = Random.Range(dropTable.GpDropped.x, dropTable.GpDropped.y + 1);
                List<ItemBase> itemsDropped = new();
                List<int> quantitiesDropped = new();

                foreach (var itemDrop in dropTable.ItemDrops)
                {
                    if (Random.Range(1, 101) <= itemDrop.DropChance)
                    {
                        int quantity = Random.Range(itemDrop.QuantityRange.x, itemDrop.QuantityRange.y + 1);

                        itemsDropped.Add(itemDrop.Item);
                        quantitiesDropped.Add(quantity);
                    }
                }

                if (gpDropped > 0)
                {
                    yield return dialogueBox.TypeDialogue($"{enemyUnit.Monster.Base.Name} dropped {gpDropped} GP!");
                    Wallet.Instance.GetWallet().AddMoney(gpDropped);
                }

                for (int i = 0; i < itemsDropped.Count; i++)
                {
                    var item = itemsDropped[i];
                    var quantity = quantitiesDropped[i];

                    yield return dialogueBox.TypeDialogue($"{enemyUnit.Monster.Base.Name} dropped {quantity}x {item.Name}!");
                    Inventory.GetInventory().AddItem(item, quantity);
                }
            }
        }

        yield return CheckForBattleOver(defeatedUnit);
    }

    IEnumerator CheckForBattleOver(BattleUnit defeatedUnit)
    {
        if (defeatedUnit.IsPlayerUnit)
        {
            var nextMonster = playerParty.GetHealthyMonster();
            if (nextMonster != null)
            {
                yield return GameController.Instance.StateMachine.PushAndWait(PartyState.Instance);
                yield return battleSystem.SwitchMonster(PartyState.Instance.SelectedMonster);
            }
            else
            {
                battleSystem.BattleOver(false);
            }
        }
        else
        {
            if (!isMasterBattle)
            {
                battleSystem.BattleOver(true);
            }
            else
            {
                var nextMonster = enemyParty.GetHealthyMonster();
                if (nextMonster != null)
                {
                    StartCoroutine(battleSystem.SendNextMasterMonster());
                }
                else
                {
                    battleSystem.BattleOver(true);
                }
            }
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogueBox.TypeDialogue("A critical hit!");
        }
    }

    IEnumerator ShowEffectiveness(float typeEffectiveness)
    {
        if (typeEffectiveness > 1f)
        {
            yield return dialogueBox.TypeDialogue("It's super effective!");
        }
        else if (typeEffectiveness < 1f)
        {
            yield return dialogueBox.TypeDialogue("It's not very effective!");
        }
    }

    IEnumerator AttemptEscape()
    {
        if (isMasterBattle)
        {
            yield return dialogueBox.TypeDialogue("You can't run from a Master battle!");
            yield break;
        }

        ++battleSystem.EscapeAttempts;

        int playerAgility = playerUnit.Monster.Agility;
        int enemyAgility = enemyUnit.Monster.Agility;

        if (playerAgility >= enemyAgility)
        {
            yield return dialogueBox.TypeDialogue("You got away safely!");
            battleSystem.BattleOver(true);
        }
        else
        {
            float f = (playerAgility * 128 / enemyAgility + 30 * battleSystem.EscapeAttempts) % 256;

            if (Random.Range(0, 256) < f)
            {
                yield return dialogueBox.TypeDialogue("You got away safely!");
                battleSystem.BattleOver(true);
            }
            else
            {
                yield return dialogueBox.TypeDialogue("You couldn't get away!");
            }
        }
    }
}
