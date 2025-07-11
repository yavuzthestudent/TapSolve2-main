using UnityEngine;
using DG.Tweening;

public class MagicTapSubtlePulse : MonoBehaviour
{
    [Range(0f, 0.2f)]
    [SerializeField] private float pulseAmount = 0.03f;

    [SerializeField] private float duration = 1f;

    private Vector3 _initialScale;

    private void Start()
    {
        _initialScale = transform.localScale;

        Vector3 targetScale = _initialScale * (1f + pulseAmount);

        // Döngüsel animasyon
        transform
            .DOScale(targetScale, duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}
