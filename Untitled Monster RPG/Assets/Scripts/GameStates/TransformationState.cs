using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

public class TransformationState : State<GameController>
{
    [SerializeField] private GameObject transformationUI;
    [SerializeField] private Image monsterImage;
    [SerializeField] private AudioClip transformationMusic;

    public static TransformationState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator Transform(Monster monster, Transformation transformation)
    {
        GameController.Instance.StateMachine.Push(this);
        transformationUI.SetActive(true);
        AudioManager.Instance.PlayMusic(transformationMusic);
        monsterImage.sprite = monster.Base.Sprite;
        yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} is transforming!");

        MonsterBase oldMonster = monster.Base;

        monster.Transform(transformation);
        monsterImage.sprite = monster.Base.Sprite;
        yield return DialogueManager.Instance.ShowDialogueText($"{oldMonster.Name} transformed into {monster.Base.Name}!");
        transformationUI.SetActive(false);
        GameController.Instance.PartyScreen.SetPartyData();
        AudioManager.Instance.PlayMusic(GameController.Instance.CurrentScene.SceneMusic, fade: true);
        GameController.Instance.StateMachine.Pop();
    }
}
