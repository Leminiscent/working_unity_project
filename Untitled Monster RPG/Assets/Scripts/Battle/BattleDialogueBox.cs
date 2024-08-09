using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] Text dialogueText;

    public void SetDialogue(string dialogue)
    {
        dialogueText.text = dialogue;
    }
}
