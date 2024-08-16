using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterController : MonoBehaviour
{
    [SerializeField] Dialogue dialogue;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject los;
    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetLosRotation(character.Animator.DefaultDirection);
    }

    public IEnumerator TriggerBattle(PlayerController player)
    {
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        var diff = player.transform.position - transform.position;
        var moveVector = diff - diff.normalized;
        moveVector = new Vector2(Mathf.Round(moveVector.x), Mathf.Round(moveVector.y));

        yield return character.Move(moveVector);

        StartCoroutine(DialogueManager.Instance.ShowDialogue(dialogue, () =>
        {

        }));
    }

    public void SetLosRotation(FacingDirections dir)
    {
        float angle = 0f;
        
        if (dir == FacingDirections.Right)
        {
            angle = 90f;
        }
        else if (dir == FacingDirections.Up)
        {
            angle = 180f;
        }
        else if (dir == FacingDirections.Left)
        {
            angle = 270f;
        }
        los.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }
}
