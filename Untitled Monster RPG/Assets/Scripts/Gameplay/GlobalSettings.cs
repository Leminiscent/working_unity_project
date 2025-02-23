using System.Collections.Generic;
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

    [Header("Animations")]
    [SerializeField] private List<Sprite> _healAnimationSprites;
    [SerializeField] private List<Sprite> _expGainAnimationSprites;
    [SerializeField] private List<Sprite> _levelUpAnimationSprites;
    [SerializeField] private List<Sprite> _affinityGainAnimationSprites;
    [SerializeField] private List<Sprite> _affinityLossAnimationSprites;

    public Color ActiveColor => _activeColor;
    public Color InactiveColor => _inactiveColor;
    public Color EmptyColor => _emptyColor;
    public Color BgHighlightColor => _bgHighlightColor;
    public int MaxPvs => _maxPvs;
    public int MaxPvPerStat => _maxPvPerStat;
    public int MaxLevel => _maxLevel;
    public MoveBase BackupMove => _backupMove;
    public List<Sprite> HealAnimationSprites => _healAnimationSprites;
    public List<Sprite> ExpGainAnimationSprites => _expGainAnimationSprites;
    public List<Sprite> LevelUpAnimationSprites => _levelUpAnimationSprites;
    public List<Sprite> AffinityGainAnimationSprites => _affinityGainAnimationSprites;
    public List<Sprite> AffinityLossAnimationSprites => _affinityLossAnimationSprites;
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
