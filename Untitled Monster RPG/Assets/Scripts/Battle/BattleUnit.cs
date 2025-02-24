using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] private bool _isPlayerUnit;
    [SerializeField] private BattleHUD _hud;
    [SerializeField] private GameObject _moveAnimationPrefab;

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
        if (Monster != null)
        {
            ClearData();
        }

        Monster = monster;
        _image.sprite = Monster.Base.Sprite;
        _image.SetNativeSize();
        _hud.gameObject.SetActive(true);
        _hud.SetData(monster);
        _image.color = _originalColor;
        _currentColor = _originalColor;
        _currentPos = _originalPos;
        Monster.OnDamageTaken += HandleDamageTaken;
        Monster.OnHealed += HandleHealed;
        StartCoroutine(PlayEnterAnimation());
    }

    public void ClearData()
    {
        if (Monster != null)
        {
            Monster.OnDamageTaken -= HandleDamageTaken;
            Monster.OnHealed -= HandleHealed;
            _hud.ClearData();
        }
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

    public IEnumerator PlayMoveCastAnimation(MoveBase move, float frameRate = 0.0167f)
    {
        List<Sprite> castAnimSprites = move.CastAnimationSprites;
        if (castAnimSprites == null || castAnimSprites.Count == 0)
        {
            yield break;
        }

        GameObject instance = Instantiate(_moveAnimationPrefab, transform);
        instance.transform.localPosition = Vector3.zero;
        if (instance.TryGetComponent(out MoveAnimationController controller))
        {
            controller.Initialize(castAnimSprites, frameRate);
        }
        yield return new WaitForSeconds((castAnimSprites.Count * frameRate) + 0.5f);

        Sequence sequence = DOTween.Sequence();
        Vector3 attackOffset = _isPlayerUnit
            ? new Vector3(0.75f, 0)
            : new Vector3(-0.75f, 0);

        sequence.Append(_image.transform.DOLocalMove(_currentPos + attackOffset, 0.3f));
        sequence.Append(_image.transform.DOLocalMove(_currentPos, 0.3f));
        AudioManager.Instance.PlaySFX(AudioID.MoveCast);
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayMoveEffectAnimation(MoveBase move, float frameRate = 0.0167f)
    {
        List<Sprite> effectAnimSprites = move.EffectAnimationSprites;

        if (effectAnimSprites == null || effectAnimSprites.Count == 0)
        {
            yield break;
        }

        GameObject instance = Instantiate(_moveAnimationPrefab, transform);
        instance.transform.localPosition = Vector3.zero;
        if (instance.TryGetComponent(out MoveAnimationController controller))
        {
            controller.Initialize(effectAnimSprites, frameRate);
        }

        yield return new WaitForSeconds((effectAnimSprites.Count * frameRate) + 0.5f);
    }

    public IEnumerator PlayDefeatAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_image.transform.DOLocalMoveY(_currentPos.y - 0.5f, 0.5f));
        sequence.Join(_image.DOFade(0f, 0.5f));
        sequence.Play();
        AudioManager.Instance.PlaySFX(AudioID.MonsterDefeat);
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator StartGuarding()
    {
        Monster.IsGuarding = true;
        float guardOffsetX = 0.25f;
        float guardOffsetY = 0.025f;
        Sequence sequence = DOTween.Sequence();
        _currentColor = Color.gray;
        _currentPos = _isPlayerUnit
            ? new Vector3(_currentPos.x - guardOffsetX, _originalPos.y - guardOffsetY, _currentPos.z)
            : new Vector3(_currentPos.x + guardOffsetX, _originalPos.y - guardOffsetY, _currentPos.z);

        sequence.Append(_image.DOColor(_currentColor, 0.1f));
        sequence.Join(_image.transform.DOLocalMove(_currentPos, 0.325f));
        AudioManager.Instance.PlaySFX(AudioID.Guard);
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator StopGuarding()
    {
        Monster.IsGuarding = false;
        Sequence sequence = DOTween.Sequence();
        _currentColor = _originalColor;
        _currentPos = _originalPos;
        sequence.Append(_image.DOColor(_currentColor, 0.1f));
        sequence.Join(_image.transform.DOLocalMove(_currentPos, 0.325f));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayExpGainAnimation(float frameRate = 0.0167f)
    {
        List<Sprite> expGainAnimSprites = GlobalSettings.Instance.ExpGainAnimationSprites;
        GameObject instance = Instantiate(_moveAnimationPrefab, transform);
        instance.transform.localPosition = Vector3.zero;
        if (instance.TryGetComponent(out MoveAnimationController controller))
        {
            controller.Initialize(expGainAnimSprites, frameRate);
        }
        AudioManager.Instance.PlaySFX(AudioID.ExpGain);
        yield return new WaitForSeconds((expGainAnimSprites.Count * frameRate) + 0.5f);
    }

    public IEnumerator PlayLevelUpAnimation(float frameRate = 0.0167f)
    {
        List<Sprite> levelUpAnimSprites = GlobalSettings.Instance.LevelUpAnimationSprites;
        GameObject instance = Instantiate(_moveAnimationPrefab, transform);
        instance.transform.localPosition = Vector3.zero;
        if (instance.TryGetComponent(out MoveAnimationController controller))
        {
            controller.Initialize(levelUpAnimSprites, frameRate);
        }
        AudioManager.Instance.PlaySFX(AudioID.LevelUp, pauseMusic: true);
        yield return new WaitForSeconds((levelUpAnimSprites.Count * frameRate) + 0.5f);
    }

    public IEnumerator PlayAffinityGainAnimation(float frameRate = 0.0167f)
    {
        List<Sprite> affinityGainAnimSprites = GlobalSettings.Instance.AffinityGainAnimationSprites;
        GameObject instance = Instantiate(_moveAnimationPrefab, transform);
        instance.transform.localPosition = Vector3.zero;
        if (instance.TryGetComponent(out MoveAnimationController controller))
        {
            controller.Initialize(affinityGainAnimSprites, frameRate);
        }
        AudioManager.Instance.PlaySFX(AudioID.AffinityGain);
        yield return new WaitForSeconds((affinityGainAnimSprites.Count * frameRate) + 0.5f);
    }

    public IEnumerator PlayAffinityLossAnimation(float frameRate = 0.0167f)
    {
        List<Sprite> affinityLossAnimSprites = GlobalSettings.Instance.AffinityLossAnimationSprites;
        GameObject instance = Instantiate(_moveAnimationPrefab, transform);
        instance.transform.localPosition = Vector3.zero;
        if (instance.TryGetComponent(out MoveAnimationController controller))
        {
            controller.Initialize(affinityLossAnimSprites, frameRate);
        }
        AudioManager.Instance.PlaySFX(AudioID.AffinityLoss);
        yield return new WaitForSeconds((affinityLossAnimSprites.Count * frameRate) + 0.5f);
    }

    public IEnumerator PlayDamageAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        Vector3 hitOffset = _isPlayerUnit
            ? new Vector3(-0.15f, 0)
            : new Vector3(0.15f, 0);

        sequence.Append(_image.transform.DOLocalMove(_currentPos + hitOffset, 0.3f));
        sequence.Join(_image.DOColor(Color.gray, 0.1f));
        sequence.Append(_image.transform.DOLocalMove(_currentPos, 0.3f));
        sequence.Join(_image.DOColor(_currentColor, 0.1f));
        AudioManager.Instance.PlaySFX(AudioID.Damage);
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayHealAnimation(float frameRate = 0.0167f)
    {
        List<Sprite> healAnimSprites = GlobalSettings.Instance.HealAnimationSprites;
        GameObject instance = Instantiate(_moveAnimationPrefab, transform);
        instance.transform.localPosition = Vector3.zero;
        if (instance.TryGetComponent(out MoveAnimationController controller))
        {
            controller.Initialize(healAnimSprites, frameRate);
        }
        yield return new WaitForSeconds((healAnimSprites.Count * frameRate) + 0.5f);
    }

    private void HandleDamageTaken()
    {
        StartCoroutine(PlayDamageAnimation());
    }

    private void HandleHealed()
    {
        StartCoroutine(PlayHealAnimation());
    }
}