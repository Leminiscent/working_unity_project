using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new material")]
public class Material : ItemBase
{
    public override bool DirectlyUsable => false;
    public override bool UsableInBattle => false;
    public override bool UsableOutsideBattle => false;
}