using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] private new string name;
    [SerializeField] private string description;
    [SerializeField] private string message;
    [SerializeField] private Sprite icon;
    [SerializeField] private float price;
    [SerializeField] private bool isSellable;

    public virtual string Name => name;
    public virtual string Description => description;
    public string Message => message;
    public Sprite Icon => icon;
    public float Price => price;
    public bool IsSellable => isSellable;

    public virtual bool Use(Monster monster) => false;
    public virtual bool IsReusable => false;
    public virtual bool DirectlyUsable => true;
    public virtual bool UsableInBattle => true;
    public virtual bool UsableOutsideBattle => true;
}
