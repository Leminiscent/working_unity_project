using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Image _image;

    [Header("Bars")]
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private HPBar _hpBar;
    [SerializeField] private TextMeshProUGUI _expText;
    [SerializeField] private GameObject _expBar;

    [Header("Status Conditions")]
    [SerializeField] private GameObject _brnText;
    [SerializeField] private GameObject _psnText;
    [SerializeField] private GameObject _frzText;
    [SerializeField] private GameObject _slpText;
    [SerializeField] private GameObject _parText;

    [Header("Message")]
    [SerializeField] private TextMeshProUGUI _messageText;

    private Battler _battler;

    private void OnDestroy()
    {
        ClearData();
    }

    public void Init(Battler battler)
    {
        _battler = battler;
        UpdateData();
        SetMessage("");
        if (_battler != null)
        {
            _battler.OnHPChanged += SetHP;
            _battler.OnStatusChanged += SetStatusText;
        }
    }

    private void UpdateData()
    {
        if (_battler == null)
        {
            Debug.LogWarning("Battler data is missing in PartyMemberUI.UpdateData.");
            return;
        }

        SetBasicInfo();
        SetHP();
        SetExp();
        SetStatusText();
    }

    public void ClearData()
    {
        if (_battler != null)
        {
            _battler.OnHPChanged -= SetHP;
            _battler.OnStatusChanged -= SetStatusText;
        }
    }

    private void SetBasicInfo()
    {
        if (_nameText != null)
        {
            _nameText.text = _battler.Base.Name;
        }

        if (_levelText != null)
        {
            _levelText.text = $"Lvl {_battler.Level}";
        }

        if (_image != null)
        {
            _image.sprite = _battler.Base.Sprite;
        }
    }

    private void SetHP()
    {
        if (_hpBar != null)
        {
            _hpBar.SetHP((float)_battler.Hp / _battler.MaxHp);
        }

        if (_hpText != null)
        {
            _hpText.text = $"{_battler.Hp} / {_battler.MaxHp}";
        }
    }

    private void SetExp()
    {
        if (_expBar != null)
        {
            _expBar.transform.localScale = new Vector3(_battler.GetNormalizedExp(), 1f, 1f);
        }

        if (_expText != null)
        {
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
    }

    private void SetStatusText()
    {
        if (_battler == null)
        {
            return;
        }

        (GameObject uiElement, ConditionID condition)[] statusMappings = new (GameObject uiElement, ConditionID condition)[]
        {
            (_brnText, ConditionID.Brn),
            (_psnText, ConditionID.Psn),
            (_frzText, ConditionID.Frz),
            (_slpText, ConditionID.Slp),
            (_parText, ConditionID.Par),
        };

        foreach ((GameObject uiElement, ConditionID condition) in statusMappings)
        {
            if (uiElement == null)
            {
                continue;
            }

            bool hasStatus = _battler.Statuses.ContainsKey(condition);

            if (gameObject.activeInHierarchy)
            {
                _ = StartCoroutine(ObjectUtil.ScaleInOut(uiElement, hasStatus));
            }
            else
            {
                uiElement.SetActive(hasStatus);
            }
        }
    }

    public void SetMessage(string message)
    {
        if (_messageText != null)
        {
            _messageText.text = message;
        }
    }

    public void SetSelected(bool selected)
    {
        if (_nameText != null && GlobalSettings.Instance != null)
        {
            _nameText.color = selected ? GlobalSettings.Instance.ActiveColor : GlobalSettings.Instance.InactiveColor;
        }
    }
}
