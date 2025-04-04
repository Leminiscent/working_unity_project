using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;

public class FloatingNumberController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _floatingText;
    [SerializeField] private float _floatDuration = 1.75f;
    [SerializeField] private float _floatDistance = 50f;

    public void Init(int number, Color color)
    {
        if (_floatingText != null)
        {
            _floatingText.text = number.ToString();
            _floatingText.color = color;
        }

        _ = StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        Sequence sequence = DOTween.Sequence();

        _ = sequence.Append(transform.DOLocalMoveY(_floatingText.transform.localPosition.y + _floatDistance, _floatDuration));
        _ = sequence.Join(_floatingText.DOFade(0, _floatDuration));
        yield return sequence.WaitForCompletion();
        Destroy(gameObject);
    }
}