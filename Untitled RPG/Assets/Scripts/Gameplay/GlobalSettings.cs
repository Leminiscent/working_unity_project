using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GlobalSettings : MonoBehaviour
{
    [field: Header("Game Colors")]
    [field: SerializeField, FormerlySerializedAs("_activeColor")] public Color ActiveColor { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_inactiveColor")] public Color InactiveColor { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_emptyColor")] public Color EmptyColor { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_bgHighlightColor")] public Color BgHighlightColor { get; private set; }

    [field: Header("Battler Details")]
    [field: SerializeField, FormerlySerializedAs("_maxPvs")] public int MaxPvs { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_maxPvPerStat")] public int MaxPvPerStat { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_maxLevel")] public int MaxLevel { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_backupMove")] public MoveBase BackupMove { get; private set; }

    [field: Header("Battle Animations")]
    [field: SerializeField, FormerlySerializedAs("_healAnimationSprites")] public List<Sprite> HealAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_expGainAnimationSprites")] public List<Sprite> ExpGainAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_levelUpAnimationSprites")] public List<Sprite> LevelUpAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_affinityGainAnimationSprites")] public List<Sprite> AffinityGainAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_affinityLossAnimationSprites;")] public List<Sprite> AffinityLossAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_strengthGainAnimationSprites;")] public List<Sprite> StrengthGainAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_strengthLossAnimationSprites;")] public List<Sprite> StrengthLossAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_enduranceGainAnimationSprites;")] public List<Sprite> EnduranceGainAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_enduranceLossAnimationSprites;")] public List<Sprite> EnduranceLossAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_intelligenceGainAnimationSprites")] public List<Sprite> IntelligenceGainAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_intelligenceLossAnimationSprites")] public List<Sprite> IntelligenceLossAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_fortitudeGainAnimationSprites")] public List<Sprite> FortitudeGainAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_fortitudeLossAnimationSprites")] public List<Sprite> FortitudeLossAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_agilityGainAnimationSprites")] public List<Sprite> AgilityGainAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_agilityLossAnimationSprites")] public List<Sprite> AgilityLossAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_setStatusConditionAnimationSprites")] public List<Sprite> SetStatusConditionAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_cureStatusConditionAnimationSprites")] public List<Sprite> CureStatusConditionAnimationSprites { get; private set; }

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