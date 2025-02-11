using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private HPBar _hpBar;
    [SerializeField] private GameObject _expBar;
    [SerializeField] private GameObject _affinityBar;
    [SerializeField] private Color _psnColor;
    [SerializeField] private Color _brnColor;
    [SerializeField] private Color _slpColor;
    [SerializeField] private Color _parColor;
    [SerializeField] private Color _frzColor;

    private Monster _monster;
    private Dictionary<ConditionID, Color> _statusColors;

    public void SetData(Monster monster)
    {
        if (_monster != null)
        {
            ClearData();
        }

        _monster = monster;

        _nameText.text = monster.Base.Name;
        SetLevel();
        _hpBar.SetHP((float)monster.Hp / monster.MaxHp);
        SetExp();
        SetAffinity();

        _statusColors = new Dictionary<ConditionID, Color>()
        {
            { ConditionID.Psn, _psnColor },
            { ConditionID.Brn, _brnColor },
            { ConditionID.Slp, _slpColor },
            { ConditionID.Par, _parColor },
            { ConditionID.Frz, _frzColor },
        };

        SetStatusText();
        _monster.OnStatusChanged += SetStatusText;
        _monster.OnHPChanged += UpdateHP;
    }

    private void SetStatusText()
    {
        if (_monster.Status == null)
        {
            _statusText.text = "";
        }
        else
        {
            _statusText.text = _monster.Status.ID.ToString().ToUpper();
            _statusText.color = _statusColors[_monster.Status.ID];
        }
    }

    public void SetLevel()
    {
        _levelText.text = "Lvl " + _monster.Level;
    }

    public void SetExp()
    {
        if (_expBar == null)
        {
            return;
        }

        if (_monster.Level < GlobalSettings.Instance.MaxLevel)
        {
            ToggleExpBar(true);
        }
        else
        {
            ToggleExpBar(false);
        }

        float normalizedExp = _monster.GetNormalizedExp();
        _expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
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

        float normalizedExp = _monster.GetNormalizedExp();

        yield return _expBar.transform.DOScaleX(normalizedExp, 1.15f).WaitForCompletion();
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
    }

    public IEnumerator SetAffinitySmooth()
    {
        if (_affinityBar == null)
        {
            yield break;
        }

        float normalizedAffinity = GetNormalizedAffinity();

        yield return _affinityBar.transform.DOScaleX(normalizedAffinity, 1.15f).WaitForCompletion();
    }

    private float GetNormalizedAffinity()
    {
        float normalizedAffinity = (float)_monster.AffinityLevel / 6;

        return Mathf.Clamp01(normalizedAffinity);
    }

    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync()
    {
        yield return _hpBar.SetHPSmooth((float)_monster.Hp / _monster.MaxHp);
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => !_hpBar.IsUpdating);
    }

    public void ClearData()
    {
        _monster.OnStatusChanged -= SetStatusText;
        _monster.OnHPChanged -= UpdateHP;
    }
}