using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectsLayer;
    [SerializeField] LayerMask encountersLayer;
    [SerializeField] LayerMask interactablesLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask losLayer;
    [SerializeField] LayerMask portalLayer;
    [SerializeField] LayerMask triggersLayer;
    [SerializeField] LayerMask ledgeLayer;
    [SerializeField] LayerMask waterLayer;

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
    public LayerMask WaterLayer => waterLayer;
    public LayerMask TriggerableLayers => encountersLayer | losLayer | portalLayer | triggersLayer | waterLayer;
}
