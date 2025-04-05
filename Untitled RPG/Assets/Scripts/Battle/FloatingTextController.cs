using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;

public class FloatingTextController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _floatingText;

    private float _floatDuration = 0.35f;
    private float _floatDistance = 15f;
    private float _hangDuration = 0.4f;
    private float _fadeDuration = 0.25f;


    public void Init(string text, Color color)
    {
        if (_floatingText != null)
        {
            _floatingText.text = text;
            _floatingText.color = color;

            Material mat = new(_floatingText.fontMaterial);
            mat.SetColor("_OutlineColor", Color.black);
            mat.SetFloat("_OutlineWidth", 0.25f);
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