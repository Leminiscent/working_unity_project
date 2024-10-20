using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color activeColor;
    [SerializeField] Color inactiveColor;
    [SerializeField] Color emptyColor;
    [SerializeField] Color bgHighlightColor;
    [SerializeField] MoveBase backupMove;

    public Color ActiveColor => activeColor;
    public Color InactiveColor => inactiveColor;
    public Color EmptyColor => emptyColor;
    public Color BgHighlightColor => bgHighlightColor;
    public MoveBase BackupMove => backupMove;
    public static GlobalSettings Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
