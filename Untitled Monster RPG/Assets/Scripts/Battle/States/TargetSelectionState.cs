using UnityEngine;
using Utils.StateMachine;

public class TargetSelectionState : State<BattleSystem>
{
    private BattleSystem _battleSystem;
    private int _selectedTarget = 0;
    private float _selectionTimer = 0;
    private const float SELECTION_SPEED = 5f;

    public static TargetSelectionState Instance { get; private set; }
    public bool SelectionMade { get; private set; }
    public int SelectedTarget => _selectedTarget;
    public bool IsTargetingAllies { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void UpdateSelectionInUI()
    {
        if (IsTargetingAllies)
        {
            for (int i = 0; i < _battleSystem.PlayerUnits.Count; i++)
            {
                _battleSystem.PlayerUnits[i].SetSelected(i == _selectedTarget);
            }
        }
        else
        {
            for (int i = 0; i < _battleSystem.EnemyUnits.Count; i++)
            {
                _battleSystem.EnemyUnits[i].SetSelected(i == _selectedTarget);
            }
        }
    }

    private void UpdateSelectionTimer()
    {
        if (_selectionTimer > 0)
        {
            _selectionTimer = Mathf.Clamp(_selectionTimer - Time.deltaTime, 0, _selectionTimer);
        }
    }

    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;
        SelectionMade = false;
        _selectedTarget = 0;
        UpdateSelectionInUI();
    }

    public override void Execute()
    {
        UpdateSelectionTimer();

        int prevSelection = _selectedTarget;

        float v = Input.GetAxisRaw("Vertical");
        
        if (_selectionTimer == 0 && Mathf.Abs(v) > 0.2f)
        {
            _selectedTarget += -(int)Mathf.Sign(v);

            if (_selectedTarget < 0)
            {
                _selectedTarget = IsTargetingAllies ? _battleSystem.PlayerUnits.Count - 1 : _battleSystem.EnemyUnits.Count - 1;
            }
            else if (_selectedTarget >= (IsTargetingAllies ? _battleSystem.PlayerUnits.Count : _battleSystem.EnemyUnits.Count))
            {
                _selectedTarget = 0;
            }

            _selectionTimer = 1 / SELECTION_SPEED;
        }

        if (_selectedTarget != prevSelection)
        {
            UpdateSelectionInUI();
            AudioManager.Instance.PlaySFX(AudioID.UIShift);
        }

        if (Input.GetButtonDown("Action"))
        {
            SelectionMade = true;
            AudioManager.Instance.PlaySFX(AudioID.UISelect);
            _battleSystem.StateMachine.Pop();
        }
        else if (Input.GetButtonDown("Back"))
        {
            SelectionMade = false;
            AudioManager.Instance.PlaySFX(AudioID.UIReturn);
            _battleSystem.StateMachine.Pop();
        }
    }

    public override void Exit()
    {
        if (IsTargetingAllies)
        {
            _battleSystem.PlayerUnits[_selectedTarget].SetSelected(false);
        }
        else
        {
            _battleSystem.EnemyUnits[_selectedTarget].SetSelected(false);
        }
    }
}