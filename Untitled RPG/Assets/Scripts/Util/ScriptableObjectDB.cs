using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectDB<T> : MonoBehaviour where T : ScriptableObject
{
    private static Dictionary<string, T> _objects;

    public static void Init()
    {
        _objects = new Dictionary<string, T>();

        T[] objectArray = Resources.LoadAll<T>("");

        foreach (T obj in objectArray)
        {
            if (_objects.ContainsKey(obj.name))
            {
                Debug.LogError($"There are two objects with the name {obj.name} in the database.");
                continue;
            }
            _objects[obj.name] = obj;
        }
    }

    public static T GetObjectByName(string name)
    {
        if (!_objects.ContainsKey(name))
        {
            Debug.LogError($"No object with the name {name} in the database.");
            return null;
        }
        return _objects[name];
    }
}
