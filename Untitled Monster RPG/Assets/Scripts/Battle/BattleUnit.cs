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
    private Vector3 _currentPos;
    private Color _currentColor;

    public bool IsPlayerUnit => _isPlayerUnit;
    public BattleHUD Hud => _hud;
    public Monster Monster { get; set; }

    private void Awake()
    {
        _image = GetComponent<Image>();
        _originalPos = _image.transform.localPosition;
        _originalColor = _image.color;
        _currentPos = _originalPos;
        _currentColor = _originalColor;
    }

    public void Setup(Monster monster)
    {
        Monster = monster;
        _image.sprite = Monster.Base.Sprite;
        _image.SetNativeSize();
        _hud.gameObject.SetActive(true);
        _hud.SetData(monster);
        _image.color = _originalColor;
        _currentColor = _originalColor;
        _currentPos = _originalPos;
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

        Vector3 attackOffset = _isPlayerUnit
            ? new Vector3(0.75f, 0)
            : new Vector3(-0.75f, 0);

        sequence.Append(_image.transform.DOLocalMove(_currentPos + attackOffset, 0.25f));
        sequence.Append(_image.transform.DOLocalMove(_currentPos, 0.25f));
    }

    public void PlayHitAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        Vector3 hitOffset = _isPlayerUnit
            ? new Vector3(-0.15f, 0)
            : new Vector3(0.15f, 0);

        sequence.Append(_image.transform.DOLocalMove(_currentPos + hitOffset, 0.25f));
        sequence.Join(_image.DOColor(Color.gray, 0.1f));
        sequence.Append(_image.transform.DOLocalMove(_currentPos, 0.25f));
        sequence.Join(_image.DOColor(_currentColor, 0.1f));
    }

    public void PlayDefeatAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(_image.transform.DOLocalMoveY(_currentPos.y - 0.5f, 0.5f));
        sequence.Join(_image.DOFade(0f, 0.5f));
    }

    public void StartGuarding()
    {
        Monster.IsGuarding = true;

        float guardOffset = 0.1f;

        _currentColor = Color.gray;
        _currentPos = _isPlayerUnit
            ? new Vector3(_currentPos.x - guardOffset, _originalPos.y - guardOffset, _currentPos.z)
            : new Vector3(_currentPos.x + guardOffset, _originalPos.y - guardOffset, _currentPos.z);

        _image.DOColor(_currentColor, 0.2f);
        _image.transform.DOLocalMove(_currentPos, 0.2f);
    }

    public void StopGuarding()
    {
        Monster.IsGuarding = false;

        _currentColor = _originalColor;
        _currentPos = _originalPos;

        _image.DOColor(_currentColor, 0.2f);
        _image.transform.DOLocalMoveY(_currentPos.y, 0.2f);
    }
}
