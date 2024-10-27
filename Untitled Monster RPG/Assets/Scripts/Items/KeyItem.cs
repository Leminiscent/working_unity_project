using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new key item")]
public class KeyItem : ItemBase
{
    public override bool DirectlyUsable => false;
    public override bool UsableInBattle => false;
    public override bool UsableOutsideBattle => false;
}
