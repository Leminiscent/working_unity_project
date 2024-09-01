using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] new string name;
    [SerializeField] string description;
    [SerializeField] string message;
    [SerializeField] Sprite icon;

    public virtual string Name => name;
    public virtual string Description => description;
    public string Message => message;
    public Sprite Icon => icon;
    public virtual bool Use(Monster monster) => false;

    public virtual bool IsReusable => false;
    public virtual bool UsableInBattle => true;
    public virtual bool UsableOutsideBattle => true;
}
