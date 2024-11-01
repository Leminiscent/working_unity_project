using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

public class TransformationState : State<GameController>
{
    [SerializeField] private GameObject _transformationUI;
    [SerializeField] private Image _monsterImage;
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

    public IEnumerator Transform(Monster monster, Transformation transformation)
    {
        GameController.Instance.StateMachine.Push(this);
        _transformationUI.SetActive(true);
        AudioManager.Instance.PlayMusic(_transformationMusic);
        _monsterImage.sprite = monster.Base.Sprite;
        yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} is transforming!");

        MonsterBase oldMonster = monster.Base;

        monster.Transform(transformation);
        _monsterImage.sprite = monster.Base.Sprite;
        yield return DialogueManager.Instance.ShowDialogueText($"{oldMonster.Name} transformed into {monster.Base.Name}!");
        _transformationUI.SetActive(false);
        GameController.Instance.PartyScreen.SetPartyData();
        AudioManager.Instance.PlayMusic(GameController.Instance.CurrentScene.SceneMusic, fade: true);
        GameController.Instance.StateMachine.Pop();
    }
}
