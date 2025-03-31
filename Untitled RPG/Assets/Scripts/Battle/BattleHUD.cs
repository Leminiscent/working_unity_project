using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Image _image;

    [Header("Stat Boosts")]
    [SerializeField] private GameObject _strBoost;
    [SerializeField] private GameObject _endBoost;
    [SerializeField] private GameObject _intBoost;
    [SerializeField] private GameObject _forBoost;
    [SerializeField] private GameObject _agiBoost;
    [SerializeField] private GameObject _accBoost;
    [SerializeField] private GameObject _evaBoost;
    [SerializeField] private GameObject _upArrowPrefab;
    [SerializeField] private GameObject _downArrowPrefab;
    [SerializeField] private Color _upBoostColor;
    [SerializeField] private Color _downBoostColor;

    [Header("Status Conditions")]
    [SerializeField] private GameObject _brnText;
    [SerializeField] private GameObject _psnText;
    [SerializeField] private GameObject _frzText;
    [SerializeField] private GameObject _slpText;
    [SerializeField] private GameObject _parText;
    [SerializeField] private GameObject _conText;

    [Header("Bars")]
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private HPBar _hpBar;
    [SerializeField] private TextMeshProUGUI _expText;
    [SerializeField] private GameObject _expBar;
    [SerializeField] private TextMeshProUGUI _affinityText;
    [SerializeField] private GameObject _affinityBar;

    private const float TWEEN_DURATION = 1.15f;
    private const float WAIT_AFTER_HP_UPDATE = 0.33f;

    private Battler _battler;

    private void Oestroy()
    {
        ClearData();
    }

    public void SetData(Battler battler)
    {
        // Clear previous event subscriptions if any.
        if (_battler != null)
        {
            ClearData();
        }

        _battler = battler;
        if (_battler == null)
        {
            Debug.LogWarning("Battler is null in BattleHUD.SetData");
            return;
        }

        if (_nameText != null)
        {
            _nameText.text = _battler.Base.Name;
        }

        SetLevel();

        if (_image != null)
        {
            _image.sprite = _battler.Base.Sprite;
        }

        if (_hpBar != null)
        {
            _hpBar.SetHP((float)_battler.Hp / _battler.MaxHp);
        }

        if (_hpText != null)
        {
            _hpText.text = $"{_battler.Hp} / {_battler.MaxHp}";
        }

        SetExp();
        ToggleAffinityBar(false);
        SetAffinity();

        SetStatusText();
        UpdateStatBoosts();

        _battler.OnStatusChanged += SetStatusText;
        _battler.OnHPChanged += UpdateHP;
    }

    private void SetStatusText() // TODO: Tween in and out the status texts.
    {
        if (_battler == null)
        {
            return;
        }

        if (_brnText != null)
        {
            _brnText.SetActive(_battler.Statuses.ContainsKey(ConditionID.Brn));
        }

        if (_psnText != null)
        {
            _psnText.SetActive(_battler.Statuses.ContainsKey(ConditionID.Psn));
        }

        if (_frzText != null)
        {
            _frzText.SetActive(_battler.Statuses.ContainsKey(ConditionID.Frz));
        }

        if (_slpText != null)
        {
            _slpText.SetActive(_battler.Statuses.ContainsKey(ConditionID.Slp));
        }

        if (_parText != null)
        {
            _parText.SetActive(_battler.Statuses.ContainsKey(ConditionID.Par));
        }

        if (_conText != null)
        {
            _conText.SetActive(_battler.VolatileStatuses.ContainsKey(ConditionID.Con));
        }
    }

    public void SetLevel()
    {
        if (_levelText != null && _battler != null)
        {
            _levelText.text = $"Lvl {_battler.Level}";
        }
    }

    public void SetExp()
    {
        if (_expBar == null || _battler == null)
        {
            return;
        }

        float normalizedExp = _battler.GetNormalizedExp();
        _expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);

        if (_expText == null)
        {
            return;
        }

        if (_battler.Level == GlobalSettings.Instance.MaxLevel)
        {
            _expText.text = "MAX";
        }
        else
        {
            int expForLevel = _battler.Base.GetExpForLevel(_battler.Level);
            int nextLevelExp = _battler.Base.GetExpForLevel(_battler.Level + 1);
            _expText.text = $"{_battler.Exp - expForLevel} / {nextLevelExp - expForLevel}";
        }
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (_expBar == null || _battler == null)
        {
            yield break;
        }

        if (reset)
        {
            _expBar.transform.localScale = new Vector3(0, 1, 1);
        }

        float normalizedExp = _battler.GetNormalizedExp();
        yield return _expBar.transform.DOScaleX(normalizedExp, TWEEN_DURATION).WaitForCompletion();

        if (_expText == null)
        {
            yield break;
        }

        if (_battler.Level == GlobalSettings.Instance.MaxLevel)
        {
            _expText.text = "MAX";
        }
        else
        {
            int expForLevel = _battler.Base.GetExpForLevel(_battler.Level);
            int nextLevelExp = _battler.Base.GetExpForLevel(_battler.Level + 1);
            _expText.text = $"{_battler.Exp - expForLevel} / {nextLevelExp - expForLevel}";
        }
    }

    public void ToggleAffinityBar(bool toggle)
    {
        if (_affinityBar == null)
        {
            return;
        }

        _affinityBar.transform.parent.gameObject.SetActive(toggle); // TODO: Tween in and out the bar.
    }

    public void SetAffinity()
    {
        if (_affinityBar == null || _battler == null)
        {
            return;
        }

        float normalizedAffinity = GetNormalizedAffinity();
        _affinityBar.transform.localScale = new Vector3(normalizedAffinity, 1, 1);
        if (_affinityText != null)
        {
            _affinityText.text = $"{_battler.AffinityLevel} / 6";
        }
    }

    public IEnumerator SetAffinitySmooth()
    {
        if (_affinityBar == null || _battler == null)
        {
            yield break;
        }

        float normalizedAffinity = GetNormalizedAffinity();
        yield return _affinityBar.transform.DOScaleX(normalizedAffinity, TWEEN_DURATION).WaitForCompletion();
        if (_affinityText != null)
        {
            _affinityText.text = $"{_battler.AffinityLevel} / 6";
        }
    }

    private float GetNormalizedAffinity()
    {
        if (_battler == null)
        {
            return 0f;
        }

        float normalizedAffinity = _battler.AffinityLevel / 6f;
        return Mathf.Clamp01(normalizedAffinity);
    }

    public void UpdateHP()
    {
        _ = StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync()
    {
        if (_hpBar != null && _battler != null)
        {
            yield return _hpBar.SetHPSmooth((float)_battler.Hp / _battler.MaxHp);
            if (_hpText != null)
            {
                _hpText.text = $"{_battler.Hp} / {_battler.MaxHp}";
            }
        }
    }

    public IEnumerator WaitForHPUpdate()
    {
        if (_hpBar != null)
        {
            yield return new WaitUntil(() => !_hpBar.IsUpdating);
            yield return new WaitForSeconds(WAIT_AFTER_HP_UPDATE);
        }
    }

    public void UpdateStatBoosts()
    {
        if (_battler == null)
        {
            return;
        }

        UpdateBoost(_strBoost, Stat.Strength);
        UpdateBoost(_endBoost, Stat.Endurance);
        UpdateBoost(_intBoost, Stat.Intelligence);
        UpdateBoost(_forBoost, Stat.Fortitude);
        UpdateBoost(_agiBoost, Stat.Agility);
        UpdateBoost(_accBoost, Stat.Accuracy);
        UpdateBoost(_evaBoost, Stat.Evasion);
    }

    private void UpdateBoost(GameObject boostContainer, Stat stat)
    {
        if (boostContainer == null || _battler == null)
        {
            return;
        }

        int boostValue = _battler.StatBoosts[stat];
        boostContainer.SetActive(boostValue != 0);

        TextMeshProUGUI boostText = boostContainer.GetComponentInChildren<TextMeshProUGUI>();
        if (boostText == null)
        {
            return;
        }

        // The arrow container is the second child (index 1)
        if (boostContainer.transform.childCount < 2)
        {
            return;
        }

        Transform arrowContainer = boostContainer.transform.GetChild(1);

        // Clear existing arrow indicators.
        ClearArrowIndicators(arrowContainer);

        if (boostValue == 0)
        {
            boostText.color = Color.white;
            return;
        }
        else
        {
            boostText.color = boostValue > 0 ? _upBoostColor : _downBoostColor;
        }

        GameObject arrowPrefab = boostValue > 0 ? _upArrowPrefab : _downArrowPrefab;
        int count = Mathf.Abs(boostValue);
        for (int i = 0; i < count; i++)
        {
            if (arrowPrefab != null && arrowContainer != null)
            {
                _ = Instantiate(arrowPrefab, arrowContainer); // TODO: Tween in the arrows.
            }
        }
    }

    private void ClearArrowIndicators(Transform arrowContainer)
    {
        for (int i = arrowContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(arrowContainer.GetChild(i).gameObject); // TODO: Tween out the arrows.
        }
    }

    public void ClearData()
    {
        if (_battler != null)
        {
            _battler.OnStatusChanged -= SetStatusText;
            _battler.OnHPChanged -= UpdateHP;
        }
    }
}