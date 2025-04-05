using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new ingredient")]
public class Ingredient : ItemBase
{
    public override bool DirectlyUsable => false;
    public override bool UsableInBattle => false;
    public override bool UsableOutsideBattle => false;
}