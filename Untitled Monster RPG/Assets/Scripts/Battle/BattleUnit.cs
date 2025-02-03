using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] private bool _isPlayerUnit;
    [SerializeField] private BattleHUD _hud;

    private Image _image;
    private Vector3 _originalPos;
    private Color _originalColor;

    public bool IsPlayerUnit => _isPlayerUnit;
    public BattleHUD Hud => _hud;
    public Monster Monster { get; set; }

    private void Awake()
    {
        _image = GetComponent<Image>();
        _originalPos = _image.transform.localPosition;
        _originalColor = _image.color;
    }

    public void Setup(Monster monster)
    {
        Monster = monster;
        _image.sprite = Monster.Base.Sprite;
        _image.SetNativeSize();
        _hud.gameObject.SetActive(true);
        _hud.SetData(monster);
        _image.color = _originalColor;
        PlayEnterAnimation();
    }

    public void Clear()
    {
        _hud.gameObject.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        _image.color = selected ? GlobalSettings.Instance.BgHighlightColor : _originalColor;
    }

    public void PlayEnterAnimation()
    {
        int offsetX = 15;

        _image.transform.localPosition = _isPlayerUnit ? new Vector3(-offsetX, _originalPos.y) : new Vector3(offsetX, _originalPos.y);

        _image.transform.DOLocalMoveX(_originalPos.x, 1.2f);
    }

    public void PlayExitAnimation()
    {
        int offsetX = 15;

        _image.transform.localPosition = _originalPos;

        if (_isPlayerUnit)
        {
            _image.transform.DOLocalMoveX(-offsetX, 1.2f);
        }
        else
        {
            _image.transform.DOLocalMoveX(offsetX, 1.2f);
        }
    }

    public void PlayAttackAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        if (_isPlayerUnit)
        {
            sequence.Append(_image.transform.DOLocalMove(new Vector3(_originalPos.x + 0.75f, _originalPos.y), 0.25f));
        }
        else
        {
            sequence.Append(_image.transform.DOLocalMove(new Vector3(_originalPos.x - 0.75f, _originalPos.y), 0.25f));
        }

        sequence.Append(_image.transform.DOLocalMove(_originalPos, 0.25f));
    }

    public void PlayHitAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        if (_isPlayerUnit)
        {
            sequence.Append(_image.transform.DOLocalMove(new Vector3(_originalPos.x - 0.15f, _originalPos.y), 0.25f));
        }
        else
        {
            sequence.Append(_image.transform.DOLocalMove(new Vector3(_originalPos.x + 0.15f, _originalPos.y), 0.25f));
        }
        sequence.Join(_image.DOColor(Color.gray, 0.1f));

        sequence.Append(_image.transform.DOLocalMove(_originalPos, 0.25f));
        sequence.Join(_image.DOColor(_originalColor, 0.1f));
    }

    public void PlayDefeatAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(_image.transform.DOLocalMoveY(_originalPos.y - 0.5f, 0.5f));
        sequence.Join(_image.DOFade(0f, 0.5f));
    }
}
