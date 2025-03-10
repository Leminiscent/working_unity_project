using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleParty : MonoBehaviour
{
    [SerializeField] private List<Battler> _battlers;

    private BattlerStorage _storage;

    public event Action OnUpdated;
    public List<Battler> Battlers
    {
        get => _battlers;
        set
        {
            _battlers = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake()
    {
        _storage = GetComponent<BattlerStorage>();
        foreach (Battler monster in _battlers)
        {
            monster.Init();
        }
    }

    public Battler GetHealthyBattlers(List<Battler> excludedBattlers = null)
    {
        List<Battler> healthyMonsters = _battlers.Where(static x => x.Hp > 0).ToList();

        if (excludedBattlers != null)
        {
            healthyMonsters = healthyMonsters.Where(x => !excludedBattlers.Contains(x)).ToList();
        }

        return healthyMonsters.FirstOrDefault();
    }

    public List<Battler> GetHealthyBattlers(int count)
    {
        return _battlers.Where(static x => x.Hp > 0).Take(count).ToList();
    }

    public void AddMember(Battler newMonster)
    {
        if (_battlers.Count < 6)
        {
            _battlers.Add(newMonster);
            OnUpdated?.Invoke();
        }
        else
        {
            _storage.AddBattlerToFirstEmptySlot(newMonster);
        }
    }

    public bool CheckForTransformations()
    {
        return _battlers.Any(static m => m.HasJustLeveledUp && m.CheckForTransformation() != null);
    }


    public IEnumerator RunTransformations()
    {
        foreach (Battler monster in _battlers)
        {
            if (monster.HasJustLeveledUp)
            {
                Transformation transformation = monster.CheckForTransformation();
                if (transformation != null)
                {
                    yield return TransformationState.Instance.Transform(monster, transformation);
                }
            }
        }
    }

    public void RestoreParty()
    {
        foreach (Battler monster in _battlers)
        {
            monster.RestoreMonster();
        }
    }

    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }

    public static BattleParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<BattleParty>();
    }
}
