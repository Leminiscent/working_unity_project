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

    public IEnumerator Transform(Battler battler, Transformation transformation)
    {
        GameController.Instance.StateMachine.Push(this);
        _transformationUI.SetActive(true);
        AudioManager.Instance.PlayMusic(_transformationMusic);
        _battlerImage.sprite = battler.Base.Sprite;
        yield return DialogueManager.Instance.ShowDialogueText($"{battler.Base.Name} is transforming!");

        BattlerBase oldBattler = battler.Base;

        battler.Transform(transformation);
        _battlerImage.sprite = battler.Base.Sprite;
        yield return DialogueManager.Instance.ShowDialogueText($"{oldBattler.Name} transformed into {battler.Base.Name}!");
        _transformationUI.SetActive(false);
        GameController.Instance.PartyScreen.SetPartyData();
        AudioManager.Instance.PlayMusic(GameController.Instance.CurrentScene.SceneMusic, fade: true);
        GameController.Instance.StateMachine.Pop();
    }
}
