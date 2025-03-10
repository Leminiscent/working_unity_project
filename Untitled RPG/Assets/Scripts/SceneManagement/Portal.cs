using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] private int _sceneToLoad = -1;
    [SerializeField] private DestinationIdentifier _destinationPortal;
    [SerializeField] private Transform _spawnPoint;

    private PlayerController _player;
    private Fader _fader;

    public bool TriggerRepeatedly => false;
    public Transform SpawnPoint => _spawnPoint;

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        _player = player;

        StartCoroutine(SwitchScene());
    }

    private void Start()
    {
        _fader = FindObjectOfType<Fader>();
    }

    private IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);
        GameController.Instance.PauseGame(true);
        yield return _fader.FadeIn(0.5f);
        yield return SceneManager.LoadSceneAsync(_sceneToLoad);

        Portal destPortal = FindObjectsOfType<Portal>().First(x => x != this && x._destinationPortal == _destinationPortal);

        _player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);
        _player.Deputy.SetPosition();
        yield return _fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);
        Destroy(gameObject);
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
