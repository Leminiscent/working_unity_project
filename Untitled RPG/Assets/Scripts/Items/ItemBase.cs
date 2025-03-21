using UnityEngine;
using UnityEngine.Serialization;

public class ItemBase : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [field: SerializeField, FormerlySerializedAs("_message")] public string Message { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_icon")] public Sprite Icon { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_price")] public int Price { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_isSellable")] public bool IsSellable { get; private set; }

    public virtual string Name => _name;
    public virtual string Description => _description;
    public virtual bool IsReusable => false;
    public virtual bool DirectlyUsable => true;
    public virtual bool UsableInBattle => true;
    public virtual bool UsableOutsideBattle => true;
    public virtual MoveTarget Target => MoveTarget.Ally;

    public virtual bool Use(Battler battler)
    {
        return false;
    }
}