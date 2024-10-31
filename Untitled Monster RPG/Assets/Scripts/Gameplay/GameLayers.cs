using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] private LayerMask solidObjectsLayer;
    [SerializeField] private LayerMask encountersLayer;
    [SerializeField] private LayerMask interactablesLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask losLayer;
    [SerializeField] private LayerMask portalLayer;
    [SerializeField] private LayerMask triggersLayer;
    [SerializeField] private LayerMask ledgeLayer;

    public static GameLayers Instance { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public LayerMask SolidObjectsLayer => solidObjectsLayer;
    public LayerMask EncountersLayer => encountersLayer;
    public LayerMask InteractablesLayer => interactablesLayer;
    public LayerMask PlayerLayer => playerLayer;
    public LayerMask LOSLayer => losLayer;
    public LayerMask PortalLayer => portalLayer;
    public LayerMask LedgeLayer => ledgeLayer;
    public LayerMask TriggerableLayers => encountersLayer | losLayer | portalLayer | triggersLayer;
}
