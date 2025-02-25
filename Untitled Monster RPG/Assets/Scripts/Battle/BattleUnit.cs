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

    private IEnumerator PlayAnimation(List<Sprite> sprites, AudioID? sfx = null, bool pauseMusic = false)
    {
        if (sprites == null || sprites.Count == 0)
        {
            yield break;
        }
        GameObject instance = Instantiate(_moveAnimationPrefab, transform);
        instance.transform.localPosition = Vector3.zero;
        float frameRate = 0.0167f;
        if (instance.TryGetComponent(out MoveAnimationController controller))
        {
            controller.Initialize(sprites, frameRate);
        }
        if (sfx.HasValue)
        {
            AudioManager.Instance.PlaySFX(sfx.Value, pauseMusic);
        }
        yield return new WaitForSeconds((sprites.Count * frameRate) + 0.5f);
    }

    private void HandleDamageTaken()
    {
        StartCoroutine(PlayDamageAnimation());
    }

    private void HandleHealed()
    {
        StartCoroutine(PlayHealAnimation());
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
        _image.transform.localPosition = _isPlayerUnit
            ? new Vector3(-offsetX, _originalPos.y)
            : new Vector3(offsetX, _originalPos.y);
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

    public IEnumerator PlayMoveCastAnimation(MoveBase move)
    {
        List<Sprite> castAnimSprites = move.CastAnimationSprites;
        yield return PlayAnimation(castAnimSprites);
        Sequence sequence = DOTween.Sequence();
        Vector3 attackOffset = _isPlayerUnit ? new Vector3(0.75f, 0) : new Vector3(-0.75f, 0);
        sequence.Append(_image.transform.DOLocalMove(_currentPos + attackOffset, 0.3f));
        sequence.Append(_image.transform.DOLocalMove(_currentPos, 0.3f));
        AudioManager.Instance.PlaySFX(AudioID.MoveCast);
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayMoveEffectAnimation(MoveBase move)
    {
        yield return PlayAnimation(move.EffectAnimationSprites);
    }

    public IEnumerator PlayExpGainAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.ExpGainAnimationSprites, AudioID.ExpGain);
    }

    public IEnumerator PlayLevelUpAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.LevelUpAnimationSprites, AudioID.LevelUp, true);
    }

    public IEnumerator PlayStatusUpAnimation(Stat stat)
    {
        List<Sprite> sprites;
        switch (stat)
        {
            case Stat.Strength:
                sprites = GlobalSettings.Instance.StrengthGainAnimationSprites;
                break;
            case Stat.Endurance:
                sprites = GlobalSettings.Instance.EnduranceGainAnimationSprites;
                break;
            case Stat.Intelligence:
                sprites = GlobalSettings.Instance.IntelligenceGainAnimationSprites;
                break;
            case Stat.Fortitude:
                sprites = GlobalSettings.Instance.FortitudeGainAnimationSprites;
                break;
            case Stat.Agility:
                sprites = GlobalSettings.Instance.AgilityGainAnimationSprites;
                break;
            case Stat.Accuracy:
                sprites = GlobalSettings.Instance.AffinityGainAnimationSprites;
                break;
            case Stat.Evasion:
                sprites = GlobalSettings.Instance.AffinityGainAnimationSprites;
                break;
            case Stat.HP:
                yield break;
            default:
                yield break;
        }
        yield return PlayAnimation(sprites, AudioID.StatusUp);
    }

    public IEnumerator PlayStatusDownAnimation(Stat stat)
    {
        List<Sprite> sprites;
        switch (stat)
        {
            case Stat.Strength:
                sprites = GlobalSettings.Instance.StrengthLossAnimationSprites;
                break;
            case Stat.Endurance:
                sprites = GlobalSettings.Instance.EnduranceLossAnimationSprites;
                break;
            case Stat.Intelligence:
                sprites = GlobalSettings.Instance.IntelligenceLossAnimationSprites;
                break;
            case Stat.Fortitude:
                sprites = GlobalSettings.Instance.FortitudeLossAnimationSprites;
                break;
            case Stat.Agility:
                sprites = GlobalSettings.Instance.AgilityLossAnimationSprites;
                break;
            case Stat.Accuracy:
                sprites = GlobalSettings.Instance.AffinityLossAnimationSprites;
                break;
            case Stat.Evasion:
                sprites = GlobalSettings.Instance.AffinityLossAnimationSprites;
                break;
            case Stat.HP:
                yield break;
            default:
                yield break;
        }
        yield return PlayAnimation(sprites, AudioID.StatusDown);
    }

    public IEnumerator PlayAffinityGainAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.AffinityGainAnimationSprites, AudioID.AffinityGain);
    }

    public IEnumerator PlayAffinityLossAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.AffinityLossAnimationSprites, AudioID.AffinityLoss);
    }

    public IEnumerator PlayDamageAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        Vector3 hitOffset = _isPlayerUnit ? new Vector3(-0.15f, 0) : new Vector3(0.15f, 0);
        sequence.Append(_image.transform.DOLocalMove(_currentPos + hitOffset, 0.3f));
        sequence.Join(_image.DOColor(Color.gray, 0.1f));
        sequence.Append(_image.transform.DOLocalMove(_currentPos, 0.3f));
        sequence.Join(_image.DOColor(_currentColor, 0.1f));
        AudioManager.Instance.PlaySFX(AudioID.Damage);
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayHealAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.HealAnimationSprites);
    }

    public IEnumerator PlayDefeatAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_image.transform.DOLocalMoveY(_currentPos.y - 0.5f, 0.75f));
        sequence.Join(_image.DOFade(0f, 0.75f));
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
}