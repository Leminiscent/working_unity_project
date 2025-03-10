using System.Collections;
using System.Linq;
using UnityEngine;

public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] private DestinationIdentifier destinationPortal;
    [SerializeField] private Transform spawnPoint;
    private PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;

        StartCoroutine(Teleport());
    }

    public bool TriggerRepeatedly => false;

    private Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    private IEnumerator Teleport()
    {
        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        LocationPortal destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == destinationPortal);

        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);
        player.Deputy.SetPosition();
        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);
    }

    public Transform SpawnPoint => spawnPoint;
}
