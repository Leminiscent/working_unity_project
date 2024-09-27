using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class RecruitmentState : State<BattleSystem>
{
    [SerializeField] RecruitmentSelectionUI selectionUI;
    BattleSystem battleSystem;

    public List<RecruitmentAnswer> Answers { get; set; }
    public static RecruitmentState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
