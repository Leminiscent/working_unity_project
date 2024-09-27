using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class DialogueState : State<GameController>
{
    public static DialogueState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
