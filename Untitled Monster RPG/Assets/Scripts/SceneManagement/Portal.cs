using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] private int sceneToLoad = -1;
    [SerializeField] private DestinationIdentifier destinationPortal;
    [SerializeField] private Transform spawnPoint;
    private PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;

        StartCoroutine(SwitchScene());
    }

    public bool TriggerRepeatedly => false;

    private Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    private IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);
        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);
        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        Portal destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);

        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);
        player.Deputy.SetPosition();
        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);
        Destroy(gameObject);
    }

    public Transform SpawnPoint => spawnPoint;
}

public enum DestinationIdentifier
{
    A, B, C, D, E
}