using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a unit in battle, handling setup, animations, and visual feedback.
/// </summary>
public class BattleUnit : MonoBehaviour
{
    [SerializeField] private bool _isPlayerUnit;
    [SerializeField] private BattleHUD _hud;
    [SerializeField] private GameObject _moveAnimationPrefab;

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

    public bool IsPlayerUnit => _isPlayerUnit;
    public BattleHUD Hud => _hud;
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

    /// <summary>
    /// Configures the unit with the given battler data and starts the entry animation.
    /// </summary>
    /// <param name="battler">The battler data to set up.</param>
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
        if (_hud != null)
        {
            _hud.gameObject.SetActive(true);
            _hud.SetData(battler);
        }
        _currentColor = _originalColor;
        _currentPos = _originalPos;
        StartCoroutine(PlayEnterAnimation());
    }

    /// <summary>
    /// Clears the unit's data and hides its HUD.
    /// </summary>
    public void ClearData()
    {
        if (Battler != null && _hud != null)
        {
            _hud.ClearData();
        }
        if (_hud != null)
        {
            _hud.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Highlights or unhighlights the unit to indicate selection.
    /// </summary>
    /// <param name="selected">True to select the unit; false to deselect.</param>
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

    /// <summary>
    /// Changes the unit's color to indicate it is targeted.
    /// </summary>
    /// <param name="selected">True to highlight; false to revert to the original color.</param>
    public void SetTargeted(bool selected)
    {
        if (_image != null)
        {
            _image.color = selected ? GlobalSettings.Instance.BgHighlightColor : _originalColor;
        }
    }

    /// <summary>
    /// Plays a custom animation using a sequence of sprites.
    /// </summary>
    /// <param name="sprites">The list of sprites for the animation.</param>
    /// <param name="sfx">Optional sound effect to play.</param>
    /// <param name="pauseMusic">Whether to pause the background music while playing the SFX.</param>
    /// <returns>An IEnumerator for coroutine handling.</returns>
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

    /// <summary>
    /// Plays the entry animation for the unit.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator PlayEnterAnimation()
    {
        if (_image == null)
        {
            yield break;
        }

        _image.transform.localPosition = _isPlayerUnit
            ? new Vector3(-ENTER_EXIT_OFFSET, _originalPos.y)
            : new Vector3(ENTER_EXIT_OFFSET, _originalPos.y);
        yield return _image.transform.DOLocalMoveX(_originalPos.x, ENTER_EXIT_DURATION).WaitForCompletion();
    }

    /// <summary>
    /// Plays the exit animation for the unit.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator PlayExitAnimation()
    {
        if (_image == null)
        {
            yield break;
        }

        _image.transform.localPosition = _originalPos;
        yield return _isPlayerUnit
            ? _image.transform.DOLocalMoveX(-ENTER_EXIT_OFFSET, ENTER_EXIT_DURATION).WaitForCompletion()
            : _image.transform.DOLocalMoveX(ENTER_EXIT_OFFSET, ENTER_EXIT_DURATION).WaitForCompletion();
    }

    /// <summary>
    /// Plays the move cast animation, including an optional cast animation from the move data.
    /// </summary>
    /// <param name="move">The move whose cast animation to play.</param>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator PlayMoveCastAnimation(MoveBase move = null)
    {
        if (move != null)
        {
            List<Sprite> castAnimSprites = move.CastAnimationSprites;
            yield return PlayAnimation(castAnimSprites);
        }

        Sequence sequence = DOTween.Sequence();
        Vector3 attackOffset = _isPlayerUnit ? new Vector3(ATTACK_OFFSET, 0) : new Vector3(-ATTACK_OFFSET, 0);
        sequence.Append(_image.transform.DOLocalMove(_currentPos + attackOffset, ATTACK_MOVE_DURATION));
        sequence.Append(_image.transform.DOLocalMove(_currentPos, ATTACK_MOVE_DURATION));
        AudioManager.Instance.PlaySFX(AudioID.MoveCast);
        yield return sequence.WaitForCompletion();
    }

    /// <summary>
    /// Plays the effect animation for a move.
    /// </summary>
    /// <param name="move">The move whose effect animation to play.</param>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator PlayMoveEffectAnimation(MoveBase move)
    {
        if (move == null)
        {
            yield break;
        }

        yield return PlayAnimation(move.EffectAnimationSprites);
    }

    /// <summary>
    /// Plays the experience gain animation.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator PlayExpGainAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.ExpGainAnimationSprites, AudioID.ExpGain);
    }

    /// <summary>
    /// Plays the level-up animation.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator PlayLevelUpAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.LevelUpAnimationSprites, AudioID.LevelUp, true);
    }

    /// <summary>
    /// Plays the stat gain animation for the specified stat.
    /// </summary>
    /// <param name="stat">The stat that was gained.</param>
    /// <returns>An IEnumerator for coroutine handling.</returns>
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

    /// <summary>
    /// Plays the stat loss animation for the specified stat.
    /// </summary>
    /// <param name="stat">The stat that was lost.</param>
    /// <returns>An IEnumerator for coroutine handling.</returns>
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

    /// <summary>
    /// Plays the affinity gain animation.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator PlayAffinityGainAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.AffinityGainAnimationSprites, AudioID.AffinityGain);
    }

    /// <summary>
    /// Plays the affinity loss animation.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator PlayAffinityLossAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.AffinityLossAnimationSprites, AudioID.AffinityLoss);
    }

    /// <summary>
    /// Plays the status condition set animation.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator PlayStatusSetAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.SetStatusConditionAnimationSprites, AudioID.SetStatus);
    }

    /// <summary>
    /// Plays the status condition cure animation.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator PlayStatusCureAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.CureStatusConditionAnimationSprites);
    }

    /// <summary>
    /// Plays the damage reaction animation.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator PlayDamageAnimation()
    {
        if (_image == null)
        {
            yield break;
        }

        Sequence sequence = DOTween.Sequence();
        Vector3 hitOffset = _isPlayerUnit ? new Vector3(-HIT_OFFSET, 0) : new Vector3(HIT_OFFSET, 0);
        sequence.Append(_image.transform.DOLocalMove(_currentPos + hitOffset, HIT_MOVE_DURATION));
        sequence.Join(_image.DOColor(Color.gray, HIT_COLOR_DURATION));
        sequence.Append(_image.transform.DOLocalMove(_currentPos, HIT_MOVE_DURATION));
        sequence.Join(_image.DOColor(_currentColor, HIT_COLOR_DURATION));
        AudioManager.Instance.PlaySFX(AudioID.Damage);
        yield return sequence.WaitForCompletion();
    }

    /// <summary>
    /// Plays the healing animation.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator PlayHealAnimation()
    {
        yield return PlayAnimation(GlobalSettings.Instance.HealAnimationSprites);
    }

    /// <summary>
    /// Plays the defeat animation.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator PlayDefeatAnimation()
    {
        if (_image == null)
        {
            yield break;
        }

        Sequence sequence = DOTween.Sequence();
        sequence.Append(_image.transform.DOLocalMoveY(_currentPos.y - DEFEAT_MOVE_Y_OFFSET, DEFEAT_DURATION));
        sequence.Join(_image.DOFade(0f, DEFEAT_DURATION));
        AudioManager.Instance.PlaySFX(AudioID.UnitDefeat);
        yield return sequence.WaitForCompletion();
    }

    /// <summary>
    /// Initiates the guarding animation, moving the unit into a guarded position.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator StartGuarding()
    {
        if (Battler == null)
        {
            yield break;
        }

        Battler.IsGuarding = true;
        Sequence sequence = DOTween.Sequence();
        _currentColor = Color.gray;
        _currentPos = _isPlayerUnit
            ? new Vector3(_currentPos.x - GUARD_OFFSET_X, _originalPos.y - GUARD_OFFSET_Y, _currentPos.z)
            : new Vector3(_currentPos.x + GUARD_OFFSET_X, _originalPos.y - GUARD_OFFSET_Y, _currentPos.z);
        sequence.Append(_image.DOColor(_currentColor, GUARD_COLOR_DURATION));
        sequence.Join(_image.transform.DOLocalMove(_currentPos, GUARD_MOVE_DURATION));
        AudioManager.Instance.PlaySFX(AudioID.Guard);
        yield return sequence.WaitForCompletion();
    }

    /// <summary>
    /// Ends the guarding animation, returning the unit to its original position and color.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
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
        sequence.Append(_image.DOColor(_currentColor, GUARD_COLOR_DURATION));
        sequence.Join(_image.transform.DOLocalMove(_currentPos, GUARD_MOVE_DURATION));
        yield return sequence.WaitForCompletion();
    }
}