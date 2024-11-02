using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterParty : MonoBehaviour
{
    [SerializeField] private List<Monster> _monsters;

    private MonsterStorage _storage;

    public event Action OnUpdated;
    public List<Monster> Monsters
    {
        get => _monsters;
        set
        {
            _monsters = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake()
    {
        _storage = GetComponent<MonsterStorage>();
        foreach (Monster monster in _monsters)
        {
            monster.Init();
        }
    }

    public Monster GetHealthyMonster()
    {
        return _monsters.FirstOrDefault(static x => x.Hp > 0);
    }

    public void AddMonster(Monster newMonster)
    {
        if (_monsters.Count < 6)
        {
            _monsters.Add(newMonster);
            OnUpdated?.Invoke();
        }
        else
        {
            _storage.AddMonsterToFirstEmptySlot(newMonster);
        }
    }

    public bool CheckForTransformations()
    {
        return _monsters.Any(static m => m.CheckForTransformation() != null);
    }

    public IEnumerator RunTransformations()
    {
        foreach (Monster monster in _monsters)
        {
            Transformation transformation = monster.CheckForTransformation();

            if (transformation != null)
            {
                yield return TransformationState.Instance.Transform(monster, transformation);
            }
        }
    }

    public void RestoreParty()
    {
        foreach (Monster monster in _monsters)
        {
            monster.CureStatus();
            monster.CureVolatileStatus();
            monster.Heal();
            monster.ResetStatBoosts();

            foreach (Move mov in monster.Moves)
            {
                mov.Sp = mov.Base.SP;
            }
        }
    }

    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }

    public static MonsterParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<MonsterParty>();
    }
}
