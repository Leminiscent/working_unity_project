using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new transformation item")]
public class TransformationItem : ItemBase
{
    public override bool UsableInBattle => false;
    public override bool Use(Battler battler)
    {
        return true;
    }
}