using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color _activeColor;
    [SerializeField] private Color _inactiveColor;
    [SerializeField] private Color _emptyColor;
    [SerializeField] private Color _bgHighlightColor;

    [Header("Monsters")]
    [SerializeField] private int _maxPvs;
    [SerializeField] private int _maxPvPerStat;
    [SerializeField] private int _maxLevel;
    [SerializeField] private MoveBase _backupMove;

    public Color ActiveColor => _activeColor;
    public Color InactiveColor => _inactiveColor;
    public Color EmptyColor => _emptyColor;
    public Color BgHighlightColor => _bgHighlightColor;
    public int MaxPvs => _maxPvs;
    public int MaxPvPerStat => _maxPvPerStat;
    public int MaxLevel => _maxLevel;
    public MoveBase BackupMove => _backupMove;
    public static GlobalSettings Instance { get; private set; }

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
}
