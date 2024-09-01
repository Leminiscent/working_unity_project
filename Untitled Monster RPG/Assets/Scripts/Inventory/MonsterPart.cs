using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new monster part")]
public class MonsterPart : ItemBase
{
    public override bool UsableInBattle => false;
    public override bool UsableOutsideBattle => false;
}
