using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] private DestinationIdentifier _destinationPortal;
    [field: SerializeField, FormerlySerializedAs("_spawnPoint")] public Transform SpawnPoint { get; private set; }

    private PlayerController _player;
    private Fader _fader;

    public bool TriggerRepeatedly => false;

    public void OnPlayerTriggered(PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        // Stop the player's movement before teleportation.
        player.Character.Animator.IsMoving = false;
        _player = player;

        _ = StartCoroutine(Teleport());
    }

    private void Start()
    {
        _fader = FindObjectOfType<Fader>();
        if (_fader == null)
        {
            Debug.LogError("Fader component not found in the scene.", this);
        }
    }

    private IEnumerator Teleport()
    {
        GameController.Instance.PauseGame(true);
        yield return _fader.FadeIn(0.5f);

        LocationPortal destinationPortal = FindDestinationPortal();
        if (destinationPortal == null)
        {
            Debug.LogError("Destination portal not found.", this);
            yield break;
        }

        _player.Character.SnapToTile(destinationPortal.SpawnPoint.position);
        _player.Deputy.SetPosition();
        yield return _fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);
    }

    private LocationPortal FindDestinationPortal()
    {
        return FindObjectsOfType<LocationPortal>()
            .FirstOrDefault(x => x != this && x._destinationPortal == _destinationPortal);
    }
}