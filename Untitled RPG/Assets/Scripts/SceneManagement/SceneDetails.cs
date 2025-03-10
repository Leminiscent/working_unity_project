using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] private List<SceneDetails> _connectedScenes;
    [SerializeField] private AudioClip _sceneMusic;

    private List<SavableEntity> _savableEntities;

    public bool IsLoaded { get; private set; }
    public AudioClip SceneMusic => _sceneMusic;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            LoadScene();
            GameController.Instance.SetCurrentScene(this);

            if (_sceneMusic != null)
            {
                AudioManager.Instance.PlayMusic(_sceneMusic, fade: true);
            }

            foreach (SceneDetails scene in _connectedScenes)
            {
                scene.LoadScene();
            }

            SceneDetails prevScene = GameController.Instance.PreviousScene;

            if (prevScene != null)
            {
                List<SceneDetails> previouslyLoadedScenes = prevScene._connectedScenes;

                foreach (SceneDetails scene in previouslyLoadedScenes)
                {
                    if (!_connectedScenes.Contains(scene) && scene != this)
                    {
                        scene.UnloadScene();
                    }
                }

                if (!_connectedScenes.Contains(prevScene))
                {
                    prevScene.UnloadScene();
                }

            }
        }
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);

            IsLoaded = true;
            operation.completed += op =>
            {
                _savableEntities = GetSavableEntitiesInScene();
                SavingSystem.Instance.RestoreEntityStates(_savableEntities);
            };
        }
    }

    public void UnloadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.Instance.CaptureEntityStates(_savableEntities);
            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    public List<SavableEntity> GetSavableEntitiesInScene()
    {
        Scene currscene = SceneManager.GetSceneByName(gameObject.name);
        List<SavableEntity> savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currscene).ToList();

        return savableEntities;
    }
}