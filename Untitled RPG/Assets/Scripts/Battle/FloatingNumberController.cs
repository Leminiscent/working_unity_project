using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;

public class FloatingNumberController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _floatingText;
    [SerializeField] private float _floatDuration = 1.25f;
    [SerializeField] private float _floatDistance = 40f;
    [SerializeField] private float _hangDuration = 0.15f;
    [SerializeField] private float _fadeDuration = 0.35f;


    public void Init(int number, Color color)
    {
        if (_floatingText != null)
        {
            _floatingText.text = number.ToString();
            _floatingText.color = color;

            Material mat = new(_floatingText.fontMaterial);
            mat.SetColor("_OutlineColor", Color.black);
            mat.SetFloat("_OutlineWidth", 0.2f);
            _floatingText.fontMaterial = mat;

        }

        _ = StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        _ = sequence.Append(transform.DOLocalMoveY(transform.localPosition.y + _floatDistance, _floatDuration));
        _ = sequence.AppendInterval(_hangDuration);
        _ = sequence.Append(_floatingText.DOFade(0, _fadeDuration));
        yield return sequence.WaitForCompletion();
        Destroy(gameObject);
    }
}