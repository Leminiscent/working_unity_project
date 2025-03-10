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

    private Battler _battler;

    public void SetData(Battler battler)
    {
        if (_battler != null)
        {
            ClearData();
        }

        _battler = battler;

        _nameText.text = battler.Base.Name;
        SetLevel();
        _image.sprite = battler.Base.Sprite;

        _hpBar.SetHP((float)battler.Hp / battler.MaxHp);
        _hpText.text = $"{battler.Hp} / {battler.MaxHp}";
        SetExp();
        ToggleAffinityBar(false);
        SetAffinity();

        SetStatusText();
        UpdateStatBoosts();

        _battler.OnStatusChanged += SetStatusText;
        _battler.OnHPChanged += UpdateHP;
    }

    private void SetStatusText()
    {
        _brnText.SetActive(_battler.Statuses.ContainsKey(ConditionID.Brn));
        _psnText.SetActive(_battler.Statuses.ContainsKey(ConditionID.Psn));
        _frzText.SetActive(_battler.Statuses.ContainsKey(ConditionID.Frz));
        _slpText.SetActive(_battler.Statuses.ContainsKey(ConditionID.Slp));
        _parText.SetActive(_battler.Statuses.ContainsKey(ConditionID.Par));
        _conText.SetActive(_battler.VolatileStatuses.ContainsKey(ConditionID.Con));
    }

    public void SetLevel()
    {
        _levelText.text = "Lvl " + _battler.Level;
    }

    public void SetExp()
    {
        if (_expBar == null)
        {
            return;
        }

        float normalizedExp = _battler.GetNormalizedExp();
        _expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
        if (_battler.Level == GlobalSettings.Instance.MaxLevel)
        {
            _expText.text = "MAX";
        }
        else
        {
            int expForLevel = _battler.Base.GetExpForLevel(_battler.Level);
            _expText.text = $"{_battler.Exp - expForLevel} / {_battler.Base.GetExpForLevel(_battler.Level + 1) - expForLevel}";
        }
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (_expBar == null)
        {
            yield break;
        }

        if (reset)
        {
            _expBar.transform.localScale = new Vector3(0, 1, 1);
        }

        float normalizedExp = _battler.GetNormalizedExp();
        yield return _expBar.transform.DOScaleX(normalizedExp, 1.15f).WaitForCompletion();

        if (_battler.Level == GlobalSettings.Instance.MaxLevel)
        {
            _expText.text = "MAX";
        }
        else
        {
            int expForLevel = _battler.Base.GetExpForLevel(_battler.Level);
            _expText.text = $"{_battler.Exp - expForLevel} / {_battler.Base.GetExpForLevel(_battler.Level + 1) - expForLevel}";

        }
    }

    public void ToggleExpBar(bool toggle)
    {
        if (_expBar == null)
        {
            return;
        }

        _expBar.transform.parent.gameObject.SetActive(toggle);
    }

    public void ToggleAffinityBar(bool toggle)
    {
        if (_affinityBar == null)
        {
            return;
        }

        _affinityBar.transform.parent.gameObject.SetActive(toggle);
    }

    public void SetAffinity()
    {
        if (_affinityBar == null)
        {
            return;
        }

        float normalizedAffinity = GetNormalizedAffinity();
        _affinityBar.transform.localScale = new Vector3(normalizedAffinity, 1, 1);
        _affinityText.text = $"{_battler.AffinityLevel} / 6";
    }

    public IEnumerator SetAffinitySmooth()
    {
        if (_affinityBar == null)
        {
            yield break;
        }

        float normalizedAffinity = GetNormalizedAffinity();

        yield return _affinityBar.transform.DOScaleX(normalizedAffinity, 1.15f).WaitForCompletion();
        _affinityText.text = $"{_battler.AffinityLevel} / 6";
    }

    private float GetNormalizedAffinity()
    {
        float normalizedAffinity = (float)_battler.AffinityLevel / 6;

        return Mathf.Clamp01(normalizedAffinity);
    }

    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync()
    {
        yield return _hpBar.SetHPSmooth((float)_battler.Hp / _battler.MaxHp);
        _hpText.text = $"{_battler.Hp} / {_battler.MaxHp}";
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => !_hpBar.IsUpdating);
        yield return new WaitForSeconds(0.33f);
    }

    public void UpdateStatBoosts()
    {
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
        int boostValue = _battler.StatBoosts[stat];
        boostContainer.SetActive(boostValue != 0);

        TextMeshProUGUI boostText = boostContainer.GetComponentInChildren<TextMeshProUGUI>();
        Transform arrowContainer = boostContainer.transform.GetChild(1);

        for (int i = arrowContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(arrowContainer.GetChild(i).gameObject);
        }

        if (boostValue == 0)
        {
            boostText.color = Color.white;
            return;
        }
        else
        {
            boostText.color = boostValue > 0 ? _upBoostColor : _downBoostColor;
        }

        GameObject arrowPrefab = (boostValue > 0) ? _upArrowPrefab : _downArrowPrefab;
        int count = Mathf.Abs(boostValue);
        for (int i = 0; i < count; i++)
        {
            Instantiate(arrowPrefab, arrowContainer);
        }
    }

    public void ClearData()
    {
        _battler.OnStatusChanged -= SetStatusText;
        _battler.OnHPChanged -= UpdateHP;
    }
}