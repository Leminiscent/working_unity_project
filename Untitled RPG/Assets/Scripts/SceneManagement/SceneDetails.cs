using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] private List<SceneDetails> _connectedScenes;
    [field: SerializeField, FormerlySerializedAs("_sceneMusic")] public AudioClip SceneMusic { get; private set; }

    private List<SavableEntity> _savableEntities;

    public bool IsLoaded { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        HandleSceneEntry();
    }

    private void HandleSceneEntry()
    {
        LoadScene();
        GameController.Instance.SetCurrentScene(this);

        if (SceneMusic != null)
        {
            AudioManager.Instance.PlayMusic(SceneMusic, fade: true);
        }

        LoadConnectedScenes();
        UnloadPreviousScenes();
    }

    private void LoadConnectedScenes()
    {
        foreach (SceneDetails scene in _connectedScenes)
        {
            if (scene != null)
            {
                scene.LoadScene();
            }
        }
    }

    private void UnloadPreviousScenes()
    {
        SceneDetails prevScene = GameController.Instance.PreviousScene;
        if (prevScene != null)
        {
            List<SceneDetails> previousConnectedScenes = prevScene._connectedScenes;

            // Unload each previously loaded connected scene that is not connected to the current scene.
            foreach (SceneDetails scene in previousConnectedScenes)
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

    public void LoadScene()
    {
        if (IsLoaded)
        {
            return;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
        IsLoaded = true;
        operation.completed += op =>
        {
            _savableEntities = GetSavableEntitiesInScene();
            SavingSystem.Instance.RestoreEntityStates(_savableEntities);
        };
    }

    public void UnloadScene()
    {
        if (!IsLoaded)
        {
            return;
        }

        SavingSystem.Instance.CaptureEntityStates(_savableEntities);
        _ = SceneManager.UnloadSceneAsync(gameObject.name);
        IsLoaded = false;
    }

    public List<SavableEntity> GetSavableEntitiesInScene()
    {
        Scene currentScene = SceneManager.GetSceneByName(gameObject.name);
        return FindObjectsOfType<SavableEntity>().Where(entity => entity.gameObject.scene == currentScene).ToList();
    }
}