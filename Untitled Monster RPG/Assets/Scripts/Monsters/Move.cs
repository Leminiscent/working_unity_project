using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public MoveBase Base { get; set; }
    public int SP { get; set; }

    public Move(MoveBase mBase)
    {
        Base = mBase;
        SP = mBase.SP;
    }

    public Move(MoveSaveData saveData)
    {
        Base = MoveDB.GetMoveByName(saveData.name);
        SP = saveData.sp;
    }

    public MoveSaveData GetSaveData()
    {
        var saveData = new MoveSaveData
        {
            name = Base.Name,
            sp = SP
        };
        return saveData;
    }
}

[System.Serializable]
public class MoveSaveData
{
    public string name;
    public int sp;
}