using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;
    [SerializeField] GameObject affinityBar;
    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Monster _monster;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Monster monster)
    {
        if (_monster != null)
        {
            ClearData();
        }

        _monster = monster;

        nameText.text = monster.Base.Name;
        SetLevel();
        hpBar.SetHP((float)monster.HP / monster.MaxHp);
        SetExp();
        SetAffinity();

        statusColors = new Dictionary<ConditionID, Color>()
        {
            { ConditionID.psn, psnColor },
            { ConditionID.brn, brnColor },
            { ConditionID.slp, slpColor },
            { ConditionID.par, parColor },
            { ConditionID.frz, frzColor },
        };

        SetStatusText();
        _monster.OnStatusChanged += SetStatusText;
        _monster.OnHPChanged += UpdateHP;
    }

    void SetStatusText()
    {
        if (_monster.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _monster.Status.ID.ToString().ToUpper();
            statusText.color = statusColors[_monster.Status.ID];
        }
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _monster.Level;
    }

    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp = _monster.GetNormalizedExp();

        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (expBar == null) yield break;

        if (reset)
        {
            expBar.transform.localScale = new Vector3(0, 1, 1);
        }

        float normalizedExp = _monster.GetNormalizedExp();

        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    public void SetAffinity()
    {
        if (affinityBar == null) return;

        float normalizedAffinity = GetNormalizedAffinity();

        affinityBar.transform.localScale = new Vector3(normalizedAffinity, 1, 1);
    }

    public IEnumerator SetAffinitySmooth()
    {
        if (affinityBar == null) yield break;

        float normalizedAffinity = GetNormalizedAffinity();

        yield return affinityBar.transform.DOScaleX(normalizedAffinity, 1.5f).WaitForCompletion();
    }

    float GetNormalizedAffinity()
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
        yield return hpBar.SetHPSmooth((float)_monster.HP / _monster.MaxHp);
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => !hpBar.IsUpdating);
    }

    public void ClearData()
    {
        _monster.OnStatusChanged -= SetStatusText;
        _monster.OnHPChanged -= UpdateHP;
    }
}