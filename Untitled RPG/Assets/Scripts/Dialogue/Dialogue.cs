using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    [SerializeField] private List<string> _lines;

    public List<string> Lines => _lines;
}