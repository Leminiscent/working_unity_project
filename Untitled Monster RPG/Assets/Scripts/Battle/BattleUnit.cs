using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] private bool _isPlayerUnit;
    [SerializeField] private BattleHUD _hud;
    [SerializeField] private GameObject _hitAnimationPrefab;

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
        StartCoroutine(PlayEnterAnimation());
    }

    public void Clear()
    {
        _hud.gameObject.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        _image.color = selected ? GlobalSettings.Instance.BgHighlightColor : _originalColor;
    }

    public IEnumerator PlayEnterAnimation()
    {
        int offsetX = 15;

        _image.transform.localPosition = _isPlayerUnit ? new Vector3(-offsetX, _originalPos.y) : new Vector3(offsetX, _originalPos.y);

        yield return _image.transform.DOLocalMoveX(_originalPos.x, 1.2f).WaitForCompletion();
    }

    public IEnumerator PlayExitAnimation()
    {
        int offsetX = 15;

        _image.transform.localPosition = _originalPos;

        yield return _isPlayerUnit
            ? _image.transform.DOLocalMoveX(-offsetX, 1.2f).WaitForCompletion()
            : _image.transform.DOLocalMoveX(offsetX, 1.2f).WaitForCompletion();
    }

    public IEnumerator PlayAttackAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        Vector3 attackOffset = _isPlayerUnit
            ? new Vector3(0.75f, 0)
            : new Vector3(-0.75f, 0);

        sequence.Append(_image.transform.DOLocalMove(_currentPos + attackOffset, 0.3f));
        sequence.Append(_image.transform.DOLocalMove(_currentPos, 0.3f));

        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayHitAnimation(List<Sprite> animSprites, float frameRate = 0.0167f)
    {
        if (animSprites == null || animSprites.Count == 0)
        {
            yield break;
        }

        GameObject instance = Instantiate(_hitAnimationPrefab, transform);
        instance.transform.localPosition = Vector3.zero;

        if (instance.TryGetComponent(out MoveAnimationController controller))
        {
            controller.Initialize(animSprites, frameRate);
        }

        yield return new WaitForSeconds(animSprites.Count * frameRate);

        Sequence sequence = DOTween.Sequence();

        Vector3 hitOffset = _isPlayerUnit
            ? new Vector3(-0.15f, 0)
            : new Vector3(0.15f, 0);

        sequence.Append(_image.transform.DOLocalMove(_currentPos + hitOffset, 0.3f));
        sequence.Join(_image.DOColor(Color.gray, 0.1f));
        sequence.Append(_image.transform.DOLocalMove(_currentPos, 0.3f));
        sequence.Join(_image.DOColor(_currentColor, 0.1f));

        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayDefeatAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(_image.transform.DOLocalMoveY(_currentPos.y - 0.5f, 0.5f));
        sequence.Join(_image.DOFade(0f, 0.5f));

        yield return sequence.WaitForCompletion();
    }

    public IEnumerator StartGuarding()
    {
        Monster.IsGuarding = true;
        float guardOffset = 0.1f;

        Sequence sequence = DOTween.Sequence();

        _currentColor = Color.gray;
        _currentPos = _isPlayerUnit
            ? new Vector3(_currentPos.x - guardOffset, _originalPos.y - guardOffset, _currentPos.z)
            : new Vector3(_currentPos.x + guardOffset, _originalPos.y - guardOffset, _currentPos.z);

        sequence.Append(_image.DOColor(_currentColor, 0.1f));
        sequence.Join(_image.transform.DOLocalMove(_currentPos, 0.3f));

        yield return sequence.WaitForCompletion();
    }

    public IEnumerator StopGuarding()
    {
        Monster.IsGuarding = false;

        Sequence sequence = DOTween.Sequence();

        _currentColor = _originalColor;
        _currentPos = _originalPos;

        sequence.Append(_image.DOColor(_currentColor, 0.1f));
        sequence.Join(_image.transform.DOLocalMoveY(_currentPos.y, 0.3f));

        yield return sequence.WaitForCompletion();
    }
}
