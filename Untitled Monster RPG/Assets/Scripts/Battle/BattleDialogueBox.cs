using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI dialogueText;

    public void SetDialogue(string dialogue)
    {
        dialogueText.text = dialogue;
    }
}
