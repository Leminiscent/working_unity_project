using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] private bool isPlayerUnit;
    [SerializeField] private BattleHUD hud;

    private Image image;
    private Vector3 originalPos;
    private Color originalColor;

    public bool IsPlayerUnit => isPlayerUnit;
    public BattleHUD Hud => hud;
    public Monster Monster { get; set; }

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public void Setup(Monster monster)
    {
        Monster = monster;
        image.sprite = Monster.Base.Sprite;

        float originalSize = image.rectTransform.rect.height;
        float newSize = originalSize;

        originalPos.y = (newSize - originalSize) / 2;
        image.transform.localPosition = new Vector3(originalPos.x, originalPos.y);
        image.transform.localScale = Vector3.one;
        hud.gameObject.SetActive(true);
        hud.SetData(monster);
        image.color = originalColor;
        PlayEnterAnimation();
    }

    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }

    public void PlayEnterAnimation()
    {
        int offsetX = 1500;

        image.transform.localPosition = isPlayerUnit ? new Vector3(-offsetX, originalPos.y) : new Vector3(offsetX, originalPos.y);

        image.transform.DOLocalMoveX(originalPos.x, 1.2f);
    }

    public void PlayExitAnimation()
    {
        int offsetX = 1500;

        image.transform.localPosition = originalPos;

        if (isPlayerUnit)
        {
            image.transform.DOLocalMoveX(-offsetX, 1.2f);
        }
        else
        {
            image.transform.DOLocalMoveX(offsetX, 1.2f);
        }
    }

    public void PlayAttackAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        if (isPlayerUnit)
        {
            sequence.Append(image.transform.DOLocalMove(new Vector3(originalPos.x + 75f, originalPos.y), 0.25f));
        }
        else
        {
            sequence.Append(image.transform.DOLocalMove(new Vector3(originalPos.x - 75f, originalPos.y), 0.25f));
        }

        sequence.Append(image.transform.DOLocalMove(originalPos, 0.25f));
    }

    public void PlayHitAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        if (isPlayerUnit)
        {
            sequence.Append(image.transform.DOLocalMove(new Vector3(originalPos.x - 15f, originalPos.y), 0.25f));
        }
        else
        {
            sequence.Append(image.transform.DOLocalMove(new Vector3(originalPos.x + 15f, originalPos.y), 0.25f));
        }
        sequence.Join(image.DOColor(Color.gray, 0.1f));

        sequence.Append(image.transform.DOLocalMove(originalPos, 0.25f));
        sequence.Join(image.DOColor(originalColor, 0.1f));
    }

    public void PlayDefeatAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 50f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }
}
