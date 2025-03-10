using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private string _message;
    [SerializeField] private Sprite _icon;
    [SerializeField] private float _price;
    [SerializeField] private bool _isSellable;

    public virtual string Name => _name;
    public virtual string Description => _description;
    public virtual bool IsReusable => false;
    public virtual bool DirectlyUsable => true;
    public virtual bool UsableInBattle => true;
    public virtual bool UsableOutsideBattle => true;
    public virtual MoveTarget Target => MoveTarget.Ally;
    public string Message => _message;
    public Sprite Icon => _icon;
    public float Price => _price;
    public bool IsSellable => _isSellable;

    public virtual bool Use(Battler battler)
    {
        return false;
    }
}
