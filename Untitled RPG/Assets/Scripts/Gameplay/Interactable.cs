using System.Collections;
using UnityEngine;

public interface IInteractable
{
    IEnumerator Interact(Transform initiator);
}