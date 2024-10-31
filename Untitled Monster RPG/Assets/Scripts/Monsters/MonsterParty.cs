using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterParty : MonoBehaviour
{
    [SerializeField] List<Monster> monsters;
    MonsterStorage storage;

    public event Action OnUpdated;

    public List<Monster> Monsters
    {
        get => monsters;
        set
        {
            monsters = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake()
    {
        storage = GetComponent<MonsterStorage>();
        foreach (Monster monster in monsters)
        {
            monster.Init();
        }
    }

    public Monster GetHealthyMonster()
    {
        return monsters.Where(static x => x.HP > 0).FirstOrDefault();
    }

    public void AddMonster(Monster newMonster)
    {
        if (monsters.Count < 6)
        {
            monsters.Add(newMonster);
            OnUpdated?.Invoke();
        }
        else
        {
            storage.AddMonsterToFirstEmptySlot(newMonster);
        }
    }

    public bool CheckForTransformations()
    {
        return monsters.Any(static m => m.CheckForTransformation() != null);
    }

    public IEnumerator RunTransformations()
    {
        foreach (Monster monster in monsters)
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
        foreach (Monster monster in monsters)
        {
            monster.CureStatus();
            monster.CureVolatileStatus();
            monster.Heal();
            monster.ResetStatBoosts();

            foreach (Move mov in monster.Moves)
            {
                mov.SP = mov.Base.SP;
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
