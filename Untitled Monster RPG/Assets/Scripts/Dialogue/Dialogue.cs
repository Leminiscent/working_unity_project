using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    [SerializeField] private List<string> lines;

    public List<string> Lines => lines;
}