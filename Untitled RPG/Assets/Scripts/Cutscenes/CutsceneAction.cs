using System.Collections;
using UnityEngine;

[System.Serializable]
public class CutsceneAction
{
    [SerializeField] private string _name;
    [SerializeField] private bool _waitForCompletion = true;

    public string Name { get => _name; set => _name = value; }
    public bool WaitForCompletion => _waitForCompletion;

    public virtual IEnumerator Play()
    {
        yield break;
    }
}
