using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SavingSystem : MonoBehaviour
{
    private Dictionary<string, object> _gameState = new();

    public static SavingSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void CaptureEntityStates(List<SavableEntity> savableEntities)
    {
        foreach (SavableEntity savable in savableEntities)
        {
            _gameState[savable.UniqueId] = savable.CaptureState();
        }
    }

    public void RestoreEntityStates(List<SavableEntity> savableEntities)
    {
        foreach (SavableEntity savable in savableEntities)
        {
            string id = savable.UniqueId;
            if (_gameState.ContainsKey(id))
            {
                savable.RestoreState(_gameState[id]);
            }
        }
    }

    public void Save(string saveFile)
    {
        CaptureState(_gameState);
        SaveFile(saveFile, _gameState);
    }

    public void Load(string saveFile)
    {
        _gameState = LoadFile(saveFile);
        RestoreState(_gameState);
    }

    public void Delete(string saveFile)
    {
        File.Delete(GetPath(saveFile));
    }

    // Used to capture states of all savable objects in the game
    private void CaptureState(Dictionary<string, object> state)
    {
        foreach (SavableEntity savable in FindObjectsOfType<SavableEntity>())
        {
            state[savable.UniqueId] = savable.CaptureState();
        }
    }

    // Used to restore states of all savable objects in the game
    private void RestoreState(Dictionary<string, object> state)
    {
        foreach (SavableEntity savable in FindObjectsOfType<SavableEntity>())
        {
            string id = savable.UniqueId;
            if (state.ContainsKey(id))
            {
                savable.RestoreState(state[id]);
            }
        }
    }

    public void RestoreEntity(SavableEntity entity)
    {
        if (_gameState.ContainsKey(entity.UniqueId))
        {
            entity.RestoreState(_gameState[entity.UniqueId]);
        }
    }

    private void SaveFile(string saveFile, Dictionary<string, object> state)
    {
        string path = GetPath(saveFile);
        print($"saving to {path}");

        using FileStream fs = File.Open(path, FileMode.Create);
        // Serialize our object
        BinaryFormatter binaryFormatter = new();
        binaryFormatter.Serialize(fs, state);
    }

    private Dictionary<string, object> LoadFile(string saveFile)
    {
        string path = GetPath(saveFile);
        if (!File.Exists(path))
        {
            return new Dictionary<string, object>();
        }

        using FileStream fs = File.Open(path, FileMode.Open);
        // Deserialize our object
        BinaryFormatter binaryFormatter = new();
        return (Dictionary<string, object>)binaryFormatter.Deserialize(fs);
    }

    private string GetPath(string saveFile)
    {
        return Path.Combine(Application.persistentDataPath, saveFile);
    }
}
