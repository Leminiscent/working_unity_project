using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] AudioClip sceneMusic;
    public bool IsLoaded { get; private set; }
    List<SavableEntity> savableEntities;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            LoadScene();
            GameController.Instance.SetCurrentScene(this);

            if (sceneMusic != null)
            {
                AudioManager.Instance.PlayMusic(sceneMusic, fade: true);
            }

            foreach (SceneDetails scene in connectedScenes)
            {
                scene.LoadScene();
            }

            SceneDetails prevScene = GameController.Instance.PreviousScene;

            if (prevScene != null)
            {
                List<SceneDetails> previouslyLoadedScenes = prevScene.connectedScenes;

                foreach (SceneDetails scene in previouslyLoadedScenes)
                {
                    if (!connectedScenes.Contains(scene) && scene != this)
                    {
                        scene.UnloadScene();
                    }
                }

                if (!connectedScenes.Contains(prevScene))
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
            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };
        }
    }

    public void UnloadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);
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

    public AudioClip SceneMusic => sceneMusic;
}