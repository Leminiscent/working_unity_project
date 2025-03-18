using UnityEngine;

/// <summary>
/// Represents an instance of a move that a battler can execute during battle.
/// </summary>
public class Move
{
    /// <summary>
    /// Gets or sets the base move data.
    /// </summary>
    public MoveBase Base { get; set; }

    /// <summary>
    /// Gets or sets the current SP (skill points) for the move.
    /// </summary>
    public int Sp { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Move"/> class with the specified base move.
    /// </summary>
    /// <param name="mBase">The base move data.</param>
    public Move(MoveBase mBase)
    {
        Base = mBase;
        Sp = mBase.SP;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Move"/> class from saved move data.
    /// </summary>
    /// <param name="saveData">The saved move data.</param>
    public Move(MoveSaveData saveData)
    {
        Base = MoveDB.GetObjectByName(saveData.Name);
        Sp = saveData.Sp;
    }

    /// <summary>
    /// Generates a <see cref="MoveSaveData"/> object representing the current state of the move.
    /// </summary>
    /// <returns>A <see cref="MoveSaveData"/> object containing the move's name and current SP.</returns>
    public MoveSaveData GetSaveData()
    {
        MoveSaveData saveData = new MoveSaveData
        {
            Name = Base.name,
            Sp = Sp
        };
        return saveData;
    }

    /// <summary>
    /// Restores SP (skill points) by the specified amount, clamping the result between 0 and the move's base SP.
    /// </summary>
    /// <param name="amount">The amount of SP to restore.</param>
    public void RestoreSP(int amount)
    {
        Sp = Mathf.Clamp(Sp + amount, 0, Base.SP);
    }
}

/// <summary>
/// Contains save data for a move, including its name and current SP.
/// </summary>
[System.Serializable]
public class MoveSaveData
{
    /// <summary>
    /// The name of the move.
    /// </summary>
    public string Name;

    /// <summary>
    /// The current SP (skill points) of the move.
    /// </summary>
    public int Sp;
}