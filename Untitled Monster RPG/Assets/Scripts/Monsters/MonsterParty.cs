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
        foreach (var monster in monsters)
        {
            monster.Init();
        }
    }

    public Monster GetHealthyMonster()
    {
        return monsters.Where(x => x.HP > 0).FirstOrDefault();
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
       return monsters.Any(m => m.CheckForTransformation() != null);
    }

    public IEnumerator RunTransformations()
    {
        foreach (var monster in monsters)
        {
            var transformation = monster.CheckForTransformation();

            if (transformation != null)
            {
                yield return TransformationState.Instance.Transform(monster, transformation);
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
