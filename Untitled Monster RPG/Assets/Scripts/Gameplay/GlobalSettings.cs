using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [Header("Game Colors")]
    [SerializeField] private Color _activeColor;
    [SerializeField] private Color _inactiveColor;
    [SerializeField] private Color _emptyColor;
    [SerializeField] private Color _bgHighlightColor;

    [Header("Battler Details")]
    [SerializeField] private int _maxPvs;
    [SerializeField] private int _maxPvPerStat;
    [SerializeField] private int _maxLevel;
    [SerializeField] private MoveBase _backupMove;

    [Header("Battle Animations")]
    [SerializeField] private List<Sprite> _healAnimationSprites;
    [SerializeField] private List<Sprite> _expGainAnimationSprites;
    [SerializeField] private List<Sprite> _levelUpAnimationSprites;
    [SerializeField] private List<Sprite> _affinityGainAnimationSprites;
    [SerializeField] private List<Sprite> _affinityLossAnimationSprites;
    [SerializeField] private List<Sprite> _strengthGainAnimationSprites;
    [SerializeField] private List<Sprite> _strengthLossAnimationSprites;
    [SerializeField] private List<Sprite> _enduranceGainAnimationSprites;
    [SerializeField] private List<Sprite> _enduranceLossAnimationSprites;
    [SerializeField] private List<Sprite> _intelligenceGainAnimationSprites;
    [SerializeField] private List<Sprite> _intelligenceLossAnimationSprites;
    [SerializeField] private List<Sprite> _fortitudeGainAnimationSprites;
    [SerializeField] private List<Sprite> _fortitudeLossAnimationSprites;
    [SerializeField] private List<Sprite> _agilityGainAnimationSprites;
    [SerializeField] private List<Sprite> _agilityLossAnimationSprites;
    [SerializeField] private List<Sprite> _setStatusConditionAnimationSprites;
    [SerializeField] private List<Sprite> _cureStatusConditionAnimationSprites;

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
    public List<Sprite> StrengthGainAnimationSprites => _strengthGainAnimationSprites;
    public List<Sprite> StrengthLossAnimationSprites => _strengthLossAnimationSprites;
    public List<Sprite> EnduranceGainAnimationSprites => _enduranceGainAnimationSprites;
    public List<Sprite> EnduranceLossAnimationSprites => _enduranceLossAnimationSprites;
    public List<Sprite> IntelligenceGainAnimationSprites => _intelligenceGainAnimationSprites;
    public List<Sprite> IntelligenceLossAnimationSprites => _intelligenceLossAnimationSprites;
    public List<Sprite> FortitudeGainAnimationSprites => _fortitudeGainAnimationSprites;
    public List<Sprite> FortitudeLossAnimationSprites => _fortitudeLossAnimationSprites;
    public List<Sprite> AgilityGainAnimationSprites => _agilityGainAnimationSprites;
    public List<Sprite> AgilityLossAnimationSprites => _agilityLossAnimationSprites;
    public List<Sprite> SetStatusConditionAnimationSprites => _setStatusConditionAnimationSprites;
    public List<Sprite> CureStatusConditionAnimationSprites => _cureStatusConditionAnimationSprites;

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
