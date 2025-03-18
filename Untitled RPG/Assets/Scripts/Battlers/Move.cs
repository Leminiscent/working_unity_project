using UnityEngine;

public class Move
{
    public MoveBase Base { get; set; }
    public int Sp { get; set; }

    public Move(MoveBase mBase)
    {
        Base = mBase;
        Sp = mBase.SP;
    }

    public Move(MoveSaveData saveData)
    {
        Base = MoveDB.GetObjectByName(saveData.Name);
        Sp = saveData.Sp;
    }

    public MoveSaveData GetSaveData()
    {
        MoveSaveData saveData = new()
        {
            Name = Base.name,
            Sp = Sp
        };
        return saveData;
    }

    public void RestoreSP(int amount)
    {
        Sp = Mathf.Clamp(Sp + amount, 0, Base.SP);
    }
}

[System.Serializable]
public class MoveSaveData
{
    public string Name;
    public int Sp;
}