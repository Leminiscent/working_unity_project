using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] private GameObject _moveAnimationPrefab;

    [field: SerializeField, FormerlySerializedAs("_isPlayerUnit")] public bool IsPlayerUnit { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_hud")] public BattleHUD Hud { get; private set; }

    private const int ENTER_EXIT_OFFSET = 15;
    private const float ANIMATION_FRAME_RATE = 0.0167f;
    private const float ANIMATION_EXTRA_WAIT = 0.5f;
    private const float ATTACK_OFFSET = 0.75f;
    private const float ATTACK_MOVE_DURATION = 0.3f;
    private const float ENTER_EXIT_DURATION = 1.2f;
    private const float HIT_OFFSET = 0.15f;
    private const float HIT_COLOR_DURATION = 0.1f;
    private const float HIT_MOVE_DURATION = 0.3f;
    private const float GUARD_OFFSET_X = 0.25f;
    private const float GUARD_OFFSET_Y = 0.025f;
    private const float GUARD_COLOR_DURATION = 0.1f;
    private const float GUARD_MOVE_DURATION = 0.325f;
    private const float DEFEAT_MOVE_Y_OFFSET = 0.5f;
    private const float DEFEAT_DURATION = 0.75f;

    private Image _image;
    private Vector3 _originalPos;
    private Color _originalColor;
    private Vector3 _currentPos;
    private Color _currentColor;

    public Battler Battler { get; set; }

    private void Awake()
    {
        _image = GetComponent<Image>();
        if (_image == null)
        {
            Debug.LogError("BattleUnit requires an Image component.");
            return;
        }
        _originalPos = _image.transform.localPosition;
        _originalColor = _image.color;
        _currentPos = _originalPos;
        _currentColor = _originalColor;
    }

    public void Setup(Battler battler)
    {
        if (Battler != null)
        {
            ClearData();
        }

        Battler = battler;
        if (_image != null)
        {
            _image.sprite = Battler.Base.Sprite;
            _image.SetNativeSize();
            _image.color = _originalColor;
        }
        if (Hud != null)
        {
            Hud.SetData(battler);
            _ = StartCoroutine(ObjectUtil.ScaleIn(Hud.gameObject));
        }
        _currentColor = _originalColor;
        _currentPos = _originalPos;
        _ = StartCoroutine(PlayEnterAnimation());
    }

    public void ClearData(bool scaleHUD = true)
    {
        if (Battler != null && Hud != null)
        {
            Hud.ClearData();
        }
        if (scaleHUD && Hud != null)
        {
            _ = StartCoroutine(ObjectUtil.ScaleOut(Hud.gameObject));
        }
    }

    public void SetSelected(bool selected)
    {
        Outline outline = GetComponent<Outline>();
        if (selected)
        {
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
            }
            outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
            outline.effectDistance = new Vector2(1f, 1f);
            outline.enabled = true;
        }
        else
        {
            if (outline != null)
            {
                outline.enabled = false;
            }
        }
    }

    public void SetTargeted(bool selected)
    {
        if (_image != null)
        {
            _image.color = selected ? GlobalSettings.Instance.BgHighlightColor : _originalColor;
        }
    }

    private IEnumerator PlayAnimation(List<Sprite> sprites, AudioID? sfx = null, bool pauseMusic = false)
    {
        if (sprites == null || sprites.Count == 0)
        {
            yield break;
        }
        if (_moveAnimationPrefab == null)
        {
            Debug.LogWarning("MoveAnimationPrefab is not assigned.");
            yield break;
        }
        GameObject instance = Instantiate(_moveAnimationPrefab, transform);
        instance.transform.localPosition = Vector3.zero;
        if (instance.TryGetComponent(out MoveAnimationController controller))
        {
            controller.Initialize(sprites, ANIMATION_FRAME_RATE);
        }
        if (sfx.HasValue)
        {
            AudioManager.Instance.PlaySFX(sfx.Value, pauseMusic);
        }
        yield return new WaitForSeconds((sprites.Count * ANIMATION_FRAME_RATE) + ANIMATION_EXTRA_WAIT);
    }

    public IEnumerator PlayEnterAnimation()
    {
        if (_image == null)
        {
            yield break;
        }

        _image.transform.localPosition = IsPlayerUnit
            ? new Vector3(-ENTER_EXIT_OFFSET, _originalPos.y)
            : new Vector3(ENTER_EXIT_OFFSET, _originalPos.y);
        yield return _image.transform.DOLocalMoveX(_originalPos.x, ENTER_EXIT_DURATION).WaitForCompletion();
    }

    public IEnumerator PlayExitAnimation()
    {
        if (_image == null)
        {
            yield break;
        }

        _image.transform.localPosition = _originalPos;
        yield return IsPlayerUnit
            ? _image.transform.DOLocalMoveX(-ENTER_EXIT_OFFSET, ENTER_EXIT_DURATION).WaitForCompletion()
            : _image.transform.DOLocalMoveX(ENTER_EXIT_OFFSET, ENTER_EXIT_DURATION).WaitForCompletion();
    }

    public IEnumerator PlayMoveCastAnimation(MoveBase move = null)
    {
        if (move != null)
        {
            List<Sprite> castAnimSprites = move.CastAnimationSprites;
            yield return PlayAnimation(castAnimSprites);
        }

        Sequence sequence = DOTween.Sequence();
        Vector3 attackOffset = IsPlayerUnit ? new Vector3(ATTACK_OFFSET, 0) : new Vector3(-ATTACK_OFFSET, 0);
        _ = sequence.Append(_image.transform.DOLocalMove(_currentPos + attackOffset, ATTACK_MOVE_DURATION));
        _ = sequence.Append(_image.transform.DOLocalMove(_currentPos, ATTACK_MOVE_DURATION));
        AudioManager.Instance.PlaySFX(AudioID.MoveCast);
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayMoveEffectAnimation(MoveBase move)
    {
        if (move == null)
        {
            yield break;
        }

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

    public IEnumerator PlayStatGainAnimation(Stat stat)
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

    public IEnumerator PlayStatLossAnimation(Stat stat)
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

    public IEnumerator PlayStatusSetAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.SetStatusConditionAnimationSprites, AudioID.SetStatus);
    }

    public IEnumerator PlayStatusCureAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.CureStatusConditionAnimationSprites);
    }

    public IEnumerator PlayDamageAnimation()
    {
        if (_image == null)
        {
            yield break;
        }

        Sequence sequence = DOTween.Sequence();
        Vector3 hitOffset = IsPlayerUnit ? new Vector3(-HIT_OFFSET, 0) : new Vector3(HIT_OFFSET, 0);
        _ = sequence.Append(_image.transform.DOLocalMove(_currentPos + hitOffset, HIT_MOVE_DURATION));
        _ = sequence.Join(_image.DOColor(Color.gray, HIT_COLOR_DURATION));
        _ = sequence.Append(_image.transform.DOLocalMove(_currentPos, HIT_MOVE_DURATION));
        _ = sequence.Join(_image.DOColor(_currentColor, HIT_COLOR_DURATION));
        AudioManager.Instance.PlaySFX(AudioID.Damage);
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayHealAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.HealAnimationSprites);
    }

    public IEnumerator PlayDefeatAnimation()
    {
        if (_image == null)
        {
            yield break;
        }

        Sequence sequence = DOTween.Sequence();
        _ = sequence.Append(_image.transform.DOLocalMoveY(_currentPos.y - DEFEAT_MOVE_Y_OFFSET, DEFEAT_DURATION));
        _ = sequence.Join(_image.DOFade(0f, DEFEAT_DURATION));
        AudioManager.Instance.PlaySFX(AudioID.UnitDefeat);
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator StartGuarding()
    {
        if (Battler == null)
        {
            yield break;
        }

        Battler.IsGuarding = true;
        Sequence sequence = DOTween.Sequence();
        _currentColor = Color.gray;
        _currentPos = IsPlayerUnit
            ? new Vector3(_currentPos.x - GUARD_OFFSET_X, _originalPos.y - GUARD_OFFSET_Y, _currentPos.z)
            : new Vector3(_currentPos.x + GUARD_OFFSET_X, _originalPos.y - GUARD_OFFSET_Y, _currentPos.z);

        AudioManager.Instance.PlaySFX(AudioID.Guard);
        _ = sequence.Append(_image.DOColor(_currentColor, GUARD_COLOR_DURATION));
        _ = sequence.Join(_image.transform.DOLocalMove(_currentPos, GUARD_MOVE_DURATION));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator StopGuarding()
    {
        if (Battler == null)
        {
            yield break;
        }

        Battler.IsGuarding = false;
        Sequence sequence = DOTween.Sequence();
        _currentColor = _originalColor;
        _currentPos = _originalPos;
        _ = sequence.Append(_image.DOColor(_currentColor, GUARD_COLOR_DURATION));
        _ = sequence.Join(_image.transform.DOLocalMove(_currentPos, GUARD_MOVE_DURATION));
        yield return sequence.WaitForCompletion();
    }
}