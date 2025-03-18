using UnityEngine;
using UnityEngine.Serialization;

public class GameLayers : MonoBehaviour
{
    [field: SerializeField, FormerlySerializedAs("_solidObjectsLayer")] public LayerMask SolidObjectsLayer { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_encountersLayer")] public LayerMask EncountersLayer { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_interactablesLayer")] public LayerMask InteractablesLayer { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_playerLayer")] public LayerMask PlayerLayer { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_losLayer")] public LayerMask LOSLayer { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_portalLayer")] public LayerMask PortalLayer { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_triggersLayer")] public LayerMask TriggersLayer { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_ledgeLayer")] public LayerMask LedgeLayer { get; private set; }

    public LayerMask TriggerableLayers => EncountersLayer | LOSLayer | PortalLayer | TriggersLayer;

    public static GameLayers Instance { get; set; }

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