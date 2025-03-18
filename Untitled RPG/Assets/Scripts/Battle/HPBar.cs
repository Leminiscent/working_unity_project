using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the visual representation of an HP bar by updating its scale.
/// </summary>
public class HPBar : MonoBehaviour
{
    [SerializeField] private GameObject _health;

    private const float TWEEN_DURATION = 1.15f;

    public bool IsUpdating { get; private set; }

    /// <summary>
    /// Immediately sets the HP bar's fill level.
    /// </summary>
    /// <param name="hpNormalized">The normalized HP value (between 0 and 1).</param>
    public void SetHP(float hpNormalized)
    {
        if (_health == null)
        {
            Debug.LogError("Health GameObject is not assigned.");
            return;
        }
        _health.transform.localScale = new Vector3(hpNormalized, 1f, 1f);
    }

    /// <summary>
    /// Smoothly animates the HP bar update from its current value to the new normalized HP value.
    /// </summary>
    /// <param name="newHp">The target normalized HP value (between 0 and 1).</param>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator SetHPSmooth(float newHp)
    {
        if (_health == null)
        {
            Debug.LogError("Health GameObject is not assigned.");
            yield break;
        }

        IsUpdating = true;
        yield return _health.transform.DOScaleX(newHp, TWEEN_DURATION).WaitForCompletion();
        IsUpdating = false;
    }
}
