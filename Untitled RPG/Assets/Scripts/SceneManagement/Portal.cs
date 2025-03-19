using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] private int _sceneToLoad = -1;
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

        // Stop player movement before scene transition.
        player.Character.Animator.IsMoving = false;
        _player = player;

        _ = StartCoroutine(SwitchScene());
    }

    private void Start()
    {
        _fader = FindObjectOfType<Fader>();
        if (_fader == null)
        {
            Debug.LogError("Fader component not found in the scene.", this);
        }
    }

    private IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);
        GameController.Instance.PauseGame(true);
        yield return _fader.FadeIn(0.5f);
        yield return SceneManager.LoadSceneAsync(_sceneToLoad);

        // Find the matching destination portal in the new scene.
        Portal destinationPortal = FindDestinationPortal();
        if (destinationPortal == null)
        {
            Debug.LogError("Destination portal not found.", this);
            yield break;
        }

        // Teleport the player to the destination portal's spawn point.
        _player.Character.SnapToTile(destinationPortal.SpawnPoint.position);
        _player.Deputy.SetPosition();

        yield return _fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);
        Destroy(gameObject);
    }

    private Portal FindDestinationPortal()
    {
        return FindObjectsOfType<Portal>()
            .FirstOrDefault(x => x != this && x._destinationPortal == _destinationPortal);
    }
}

public enum DestinationIdentifier
{
    A,
    B,
    C,
    D,
    E
}