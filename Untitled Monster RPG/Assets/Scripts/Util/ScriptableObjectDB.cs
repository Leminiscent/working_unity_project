using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectDB<T> : MonoBehaviour where T : ScriptableObject
{
    static Dictionary<string, T> objects;

    public static void Init()
    {
        objects = new Dictionary<string, T>();

        var objectArray = Resources.LoadAll<T>("");

        foreach (var obj in objectArray)
        {
            if (objects.ContainsKey(obj.name))
            {
                Debug.LogError($"There are two objects with the name {obj.name} in the database.");
                continue;
            }
            objects[obj.name] = obj;
        }
    }

    public static T GetObjectByName(string name)
    {
        if (!objects.ContainsKey(name))
        {
            Debug.LogError($"No object with the name {name} in the database.");
            return null;
        }
        return objects[name];
    }
}
