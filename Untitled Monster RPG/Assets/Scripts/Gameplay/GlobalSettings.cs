using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;
    [SerializeField] private Color emptyColor;
    [SerializeField] private Color bgHighlightColor;

    [Header("Monsters")]
    [SerializeField] private int maxPvs;
    [SerializeField] private int maxPvPerStat;
    [SerializeField] private int maxLevel;
    [SerializeField] private MoveBase backupMove;

    public Color ActiveColor => activeColor;
    public Color InactiveColor => inactiveColor;
    public Color EmptyColor => emptyColor;
    public Color BgHighlightColor => bgHighlightColor;
    public int MaxPvs => maxPvs;
    public int MaxPvPerStat => maxPvPerStat;
    public int MaxLevel => maxLevel;
    public MoveBase BackupMove => backupMove;
    public static GlobalSettings Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
