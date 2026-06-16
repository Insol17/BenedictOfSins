using UnityEngine;
using DG.Tweening;

public class SwordAnimationController : MonoBehaviour
{
    public RectTransform swordTransform;
    public Vector3 hiddenPosition;
    public Vector3 stabPosition;
    public float stabDuration = 0.7f;
    public float pullDuration = 0.5f;

    void OnEnable()
    {
        CardManager.OnEnemyTurnStarted += PlayEnemyTurnEffect;
        CardManager.OnPlayerTurnStarted += PlayPlayerTurnEffect;
    }

    void OnDisable()
    {
        CardManager.OnEnemyTurnStarted -= PlayEnemyTurnEffect;
        CardManager.OnPlayerTurnStarted -= PlayPlayerTurnEffect;
    }

    public void PlayEnemyTurnEffect()
    {
        swordTransform.gameObject.SetActive(true);
        swordTransform.DOKill();
        swordTransform.anchoredPosition = hiddenPosition;

        Sequence seq = DOTween.Sequence();
        seq.Append(swordTransform.DOAnchorPos(stabPosition, stabDuration).SetEase(Ease.OutBack));
        seq.Append(swordTransform.DOShakeAnchorPos(0.2f, strength: 10f, vibrato: 15));
    }

    public void PlayPlayerTurnEffect()
    {
        swordTransform.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.Append(swordTransform.DOAnchorPos(hiddenPosition, pullDuration).SetEase(Ease.InBack));
        seq.AppendCallback(() => swordTransform.gameObject.SetActive(false));
    }
}
