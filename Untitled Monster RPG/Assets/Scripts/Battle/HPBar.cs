using System.Collections;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] private GameObject _health;
    
    public bool IsUpdating { get; private set; }

    public void SetHP(float hpNormalized)
    {
        _health.transform.localScale = new Vector3(hpNormalized, 1f, 1f);
    }

    public IEnumerator SetHPSmooth(float newHp)
    {
        IsUpdating = true;

        float curHp = _health.transform.localScale.x;
        bool isDamaging = curHp - newHp > 0;
        float changeAmt = curHp - newHp;

        if (changeAmt != 0)
        {
            while (isDamaging ? (curHp - newHp > Mathf.Epsilon) : (curHp - newHp < Mathf.Epsilon))
            {
                curHp -= changeAmt * Time.deltaTime;
                _health.transform.localScale = new Vector3(curHp, 1f, 1f);
                yield return null;
            }
            _health.transform.localScale = new Vector3(newHp, 1f, 1f);
        }

        IsUpdating = false;
    }
}
