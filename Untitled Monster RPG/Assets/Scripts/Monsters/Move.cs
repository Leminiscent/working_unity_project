using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public MoveBase Base { get; set; }
    public int AP { get; set; }

    public Move(MoveBase mBase)
    {
        Base = mBase;
        AP = mBase.AP;
    }

    public Move(MoveSaveData saveData)
    {
        Base = MoveDB.GetMoveByName(saveData.name);
        AP = saveData.ap;
    }

    public MoveSaveData GetSaveData()
    {
        var saveData = new MoveSaveData
        {
            name = Base.Name,
            ap = AP
        };
        return saveData;
    }
}

[System.Serializable]
public class MoveSaveData
{
    public string name;
    public int ap;
}