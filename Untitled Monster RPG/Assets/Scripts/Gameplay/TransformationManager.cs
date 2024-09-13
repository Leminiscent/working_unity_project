using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransformationManager : MonoBehaviour
{
    [SerializeField] GameObject transformationUI;
    [SerializeField] Image monsterImage;
    [SerializeField] AudioClip transformationMusic;

    public event Action OnStartTransformation;
    public event Action OnEndTransformation;

    public static TransformationManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator Transform(Monster monster, Transformation transformation)
    {
        OnStartTransformation?.Invoke();
        transformationUI.SetActive(true);
        AudioManager.Instance.PlayMusic(transformationMusic);
        monsterImage.sprite = monster.Base.Sprite;
        yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} is transforming!");

        var oldMonster = monster.Base;

        monster.Transform(transformation);
        monsterImage.sprite = monster.Base.Sprite;
        yield return DialogueManager.Instance.ShowDialogueText($"{oldMonster.Name} transformed into {monster.Base.Name}!");
        transformationUI.SetActive(false);
        OnEndTransformation?.Invoke();
    }
}
