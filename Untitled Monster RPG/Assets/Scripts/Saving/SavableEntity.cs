using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class SavableEntity : MonoBehaviour
{
    [SerializeField] private string _uniqueId = "";

    private static Dictionary<string, SavableEntity> _globalLookup = new();

    public string UniqueId => _uniqueId;

    // Used to capture state of the gameobject on which the savableEntity is attached
    public object CaptureState()
    {
        Dictionary<string, object> state = new();
        foreach (ISavable savable in GetComponents<ISavable>())
        {
            state[savable.GetType().ToString()] = savable.CaptureState();
        }
        return state;
    }

    // Used to restore state of the gameobject on which the savableEntity is attached
    public void RestoreState(object state)
    {
        Dictionary<string, object> stateDict = (Dictionary<string, object>)state;
        foreach (ISavable savable in GetComponents<ISavable>())
        {
            string id = savable.GetType().ToString();

            if (stateDict.ContainsKey(id))
            {
                savable.RestoreState(stateDict[id]);
            }
        }
    }

#if UNITY_EDITOR
    // Update method used for generating UUID of the SavableEntity
    private void Update()
    {
        // don't execute in playmode
        if (Application.IsPlaying(gameObject))
        {
            return;
        }

        // don't generate Id for prefabs (prefab scene will have path as null)
        if (string.IsNullOrEmpty(gameObject.scene.path))
        {
            return;
        }

        SerializedObject serializedObject = new(this);
        SerializedProperty property = serializedObject.FindProperty("_uniqueId");

        if (string.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue))
        {
            property.stringValue = Guid.NewGuid().ToString();
            serializedObject.ApplyModifiedProperties();
        }

        _globalLookup[property.stringValue] = this;
    }
#endif

    private bool IsUnique(string candidate)
    {
        if (!_globalLookup.ContainsKey(candidate))
        {
            return true;
        }

        if (_globalLookup[candidate] == this)
        {
            return true;
        }

        // Handle scene unloading cases
        if (_globalLookup[candidate] == null)
        {
            _globalLookup.Remove(candidate);
            return true;
        }

        // Handle edge cases like designer manually changing the UUID
        if (_globalLookup[candidate].UniqueId != candidate)
        {
            _globalLookup.Remove(candidate);
            return true;
        }

        return false;
    }
}