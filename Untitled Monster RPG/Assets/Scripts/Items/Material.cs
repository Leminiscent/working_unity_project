using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new material")]
public class Material : ItemBase
{
    public override bool UsableInBattle => false;
    public override bool UsableOutsideBattle => false;
}
