using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHUD hud;

    public bool IsPlayerUnit => isPlayerUnit;
    public BattleHUD Hud => hud;

    public Monster Monster { get; set; }
    Image image;
    Vector3 originalPos;
    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public void Setup(Monster monster)
    {
        Monster = monster;
        if (isPlayerUnit)
        {
            image.sprite = Monster.Base.BackSprite;
        }
        else
        {
            image.sprite = Monster.Base.FrontSprite;
        }
        hud.SetData(monster);
        image.color = originalColor;
        PlayEnterAnimation();
    }

    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
        {
            image.transform.localPosition = new Vector3(-472f, originalPos.y);
        }
        else
        {
            image.transform.localPosition = new Vector3(472f, originalPos.y);
        }

        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    public void PlayExitAnimation()
    {
        image.transform.localPosition = originalPos;

        if (isPlayerUnit)
        {
            image.transform.DOLocalMoveX(-472f, 1f);
        }
        else
        {
            image.transform.DOLocalMoveX(472f, 1f);
        }
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();

        if (isPlayerUnit)
        {
            sequence.Append(image.transform.DOLocalMove(new Vector3(originalPos.x + 75f, originalPos.y + 25f), 0.25f));
        }
        else
        {
            sequence.Append(image.transform.DOLocalMove(new Vector3(originalPos.x - 75f, originalPos.y - 25f), 0.25f));
        }

        sequence.Append(image.transform.DOLocalMove(originalPos, 0.25f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();

        if (isPlayerUnit)
        {
            sequence.Append(image.transform.DOLocalMove(new Vector3(originalPos.x - 15f, originalPos.y - 5f), 0.25f));
        }
        else
        {
            sequence.Append(image.transform.DOLocalMove(new Vector3(originalPos.x + 15f, originalPos.y + 5f), 0.25f));
        }
        sequence.Join(image.DOColor(Color.gray, 0.1f));

        sequence.Append(image.transform.DOLocalMove(originalPos, 0.25f));
        sequence.Join(image.DOColor(originalColor, 0.1f));
    }

    public void PlayDefeatAnimation()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 50f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }
}
