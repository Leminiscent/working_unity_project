using DG.Tweening;
using System.Collections;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] private GameObject _health;

    private const float TWEEN_DURATION = 1.15f;

    public bool IsUpdating { get; private set; }

    public void SetHP(float hpNormalized)
    {
        if (_health == null)
        {
            Debug.LogError("Health GameObject is not assigned.");
            return;
        }
        _health.transform.localScale = new Vector3(hpNormalized, 1f, 1f);
    }

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
