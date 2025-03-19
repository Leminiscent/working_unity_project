using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

public class TransformationState : State<GameController>
{
    [SerializeField] private GameObject _transformationUI;
    [SerializeField] private Image _battlerImage;
    [SerializeField] private AudioClip _transformationMusic;

    public static TransformationState Instance { get; private set; }

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

    public IEnumerator PerformTransformation(Battler battler, Transformation transformation)
    {
        if (battler == null)
        {
            Debug.LogError("Battler is null in PerformTransformation.");
            yield break;
        }
        if (transformation == null)
        {
            Debug.LogError("Transformation is null in PerformTransformation.");
            yield break;
        }

        GameController.Instance.StateMachine.Push(this);

        if (_transformationUI != null)
        {
            _transformationUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Transformation UI is not assigned.");
        }

        if (_transformationMusic != null)
        {
            AudioManager.Instance.PlayMusic(_transformationMusic);
        }
        else
        {
            Debug.LogWarning("Transformation music is not assigned.");
        }

        // Set the battler image to the current battler sprite.
        if (_battlerImage != null)
        {
            _battlerImage.sprite = battler.Base.Sprite;
        }
        else
        {
            Debug.LogWarning("Battler image is not assigned.");
        }

        yield return DialogueManager.Instance.ShowDialogueText($"{battler.Base.Name} is transforming!");

        // Store current battler data for reference.
        BattlerBase oldBattler = battler.Base;

        // Perform the transformation.
        battler.Transform(transformation);

        // Update the battler image to reflect the new form.
        if (_battlerImage != null)
        {
            _battlerImage.sprite = battler.Base.Sprite;
        }

        yield return DialogueManager.Instance.ShowDialogueText($"{oldBattler.Name} transformed into {battler.Base.Name}!");

        if (_transformationUI != null)
        {
            _transformationUI.SetActive(false);
        }

        // Update the party screen with the latest battler data.
        GameController.Instance.PartyScreen.SetPartyData();

        // Resume the scene's background music with a fade.
        if (GameController.Instance.CurrentScene != null)
        {
            AudioManager.Instance.PlayMusic(GameController.Instance.CurrentScene.SceneMusic, fade: true);
        }
        else
        {
            Debug.LogWarning("Current scene is null in TransformationState.");
        }

        GameController.Instance.StateMachine.Pop();
    }
}