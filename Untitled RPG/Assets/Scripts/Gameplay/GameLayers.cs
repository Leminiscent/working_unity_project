using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] private LayerMask _solidObjectsLayer;
    [SerializeField] private LayerMask _encountersLayer;
    [SerializeField] private LayerMask _interactablesLayer;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _losLayer;
    [SerializeField] private LayerMask _portalLayer;
    [SerializeField] private LayerMask _triggersLayer;
    [SerializeField] private LayerMask _ledgeLayer;

    public LayerMask SolidObjectsLayer => _solidObjectsLayer;
    public LayerMask EncountersLayer => _encountersLayer;
    public LayerMask InteractablesLayer => _interactablesLayer;
    public LayerMask PlayerLayer => _playerLayer;
    public LayerMask LOSLayer => _losLayer;
    public LayerMask PortalLayer => _portalLayer;
    public LayerMask LedgeLayer => _ledgeLayer;
    public LayerMask TriggerableLayers => _encountersLayer | _losLayer | _portalLayer | _triggersLayer;
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
