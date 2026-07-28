// ================================
// HandManager.cs (전체 통합 원본)
// ================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using System.Linq;

public class HandManager : MonoBehaviour
{
    [SerializeField] private int maxHandSize;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] public SplineContainer splineContainer;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform discardPilePosition;
    [SerializeField] private Transform drawPilePosition;
    [SerializeField] private Transform handCenter;

    [Header("Hover Layout")]
    [Tooltip("호버 카드가 올라가는 높이(월드)")]
    [SerializeField] private float hoverLiftY = 60f;
    [Tooltip("호버 카드 스케일 배수")]
    [SerializeField] private float hoverScale = 1.3f;
    [Tooltip("가장 가까운 이웃이 밀리는 최대 거리(월드)")]
    [SerializeField] private float neighborBasePush = 60f;
    [Tooltip("가우시안 감쇠 폭(클수록 멀리까지 영향)")]
    [SerializeField, Range(0.5f, 3f)] private float neighborSigma = 1.2f;
    [SerializeField] private float hoverTween = 0.14f;

    private List<GameObject> handCards = new();
    private int prevCardCount = -1;

    void Update()
    {
        int currentCount = handCards.Count;
        if (currentCount != prevCardCount)
        {
            UpdateCardPositions();
            prevCardCount = currentCount;
        }
    }

    public void DrawCard(CardData cardData)
    {
        Debug.Log($"[DrawCard] cardPrefab is {(cardPrefab == null ? "NULL" : "OK")}");
        Debug.Log($"[DrawCard] drawPilePosition is {(drawPilePosition == null ? "NULL" : drawPilePosition.position.ToString())}");

        if (handCards.Count >= maxHandSize) return;

        GameObject go = Instantiate(cardPrefab, drawPilePosition.position + Vector3.down * 200f, Quaternion.identity, handCenter);
        if (go == null)
        {
            Debug.LogError("[DrawCard] 카드 생성 실패! go == null");
            return;
        }
        else
        {
            Debug.Log("[DrawCard] 카드 생성 성공!");
        }

        go.transform.localScale = Vector3.zero;

        go.GetComponent<CardDisplay>()?.SetCard(cardData);
        handCards.Add(go);

        var selector = go.GetComponent<CardSelector>();
        selector?.StartDrawAnimation();

        Sequence drawSeq = DOTween.Sequence();
        drawSeq.Append(go.transform.DOJump(handCenter.position, 100f, 1, 0.2f).SetEase(Ease.OutQuad));
        drawSeq.Join(go.transform.DOScale(Vector3.one, 0.2f));
        drawSeq.OnComplete(() =>
        {
            selector?.EndDrawAnimation();
            UpdateCardPositions();
            StartCoroutine(SaveTransformAfterPositioning(go));
        });
    }

    private IEnumerator SaveTransformAfterPositioning(GameObject card)
    {
        yield return new WaitForSeconds(0.2f);

        if (card == null) yield break;

        CardSelector selector = card.GetComponent<CardSelector>();
        if (selector != null)
        {
            selector.ForceSaveOriginalTransform();
        }
    }


    public void AddCardToHand(GameObject card)
    {
        handCards.Add(card);
        UpdateCardPositions();
        StartCoroutine(SaveTransformAfterPositioning(card));
    }

    public void SaveAllCardTransforms()
    {
        foreach (var card in handCards)
        {
            var selector = card.GetComponent<CardSelector>();
            selector?.ForceSaveOriginalTransform();
        }
    }

    public void ClearHand()
    {
        foreach (var card in handCards)
        {
            if (card != null)
                Destroy(card);
        }
        handCards.Clear();
    }

    public void ClearHandWithAnimation()
    {
        StartCoroutine(ClearHandRoutine());
    }

    private IEnumerator ClearHandRoutine()
    {
        foreach (var card in handCards)
        {
            if (card != null)
            {
                // ? 카드 데이터를 미리 등록
                var cardData = card.GetComponent<CardDisplay>()?.cardData;
                if (cardData != null)
                    DeckManager.Instance.UseCard(cardData); // ? 이 라인 추가

                // 애니메이션
                card.transform.DOMove(discardPilePosition.position, 0.3f).SetEase(Ease.InBack);
                card.transform.DOScale(Vector3.zero, 0.3f);
                Destroy(card, 0.35f);
            }
        }

        handCards.Clear();
        yield return new WaitForSeconds(0.35f);
        UpdateCardPositions();
    }



    public void RemoveCardFromHand(GameObject card)
    {
        if (card != null && handCards.Contains(card))
            handCards.Remove(card);

    }

    public void UpdateCardPositions()
    {
        handCards.RemoveAll(card => card == null);
        if (handCards.Count == 0) return;

        int count = handCards.Count;
        float spacing = 150f;
        float rotationRange = 10f;
        float centerIndex = (count - 1) / 2f;

        Spline spline = splineContainer.Spline;
        Vector3 centerPos = splineContainer.transform.TransformPoint(spline.Knots.ElementAt(2).Position);
        Vector3 rightRef = splineContainer.transform.TransformPoint(spline.Knots.ElementAt(3).Position);
        Vector3 leftRef = splineContainer.transform.TransformPoint(spline.Knots.ElementAt(1).Position);
        Vector3 rightDir = (rightRef - leftRef).normalized;

        float yCurveHeight = 20f;
        float maxDist = Mathf.Max(1f, spacing * centerIndex * 1.1f);

        // === 호버된 카드 인덱스 계산 ===
        int hoveredIndex = -1;
        var coord = HandHoverCoordinator.Instance;
        if (coord != null)
        {
            int idx = coord.CurrentHoveredIndex;   // 코디네이터가 계산한 현재 호버 인덱스
            if (idx >= 0 && idx < handCards.Count)
            {
                var sel = handCards[idx].GetComponent<CardSelector>();
                if (!(sel != null && (sel.IsSelected || sel.IsMovingToCenter || sel.IsFollowingMouse || sel.IsEnteringFromDraw)))
                    hoveredIndex = idx;
            }
        }

        for (int i = 0; i < count; i++)
        {
            GameObject card = handCards[i];
            var selector = card.GetComponent<CardSelector>();
            var tCard = card.transform;

            // 선택/중앙이동/드래그/드로우 진입중인 카드는 레이아웃에서 제외(현재 동작 유지)
            if (selector != null && (selector.IsMovingToCenter || selector.IsSelected || selector.IsFollowingMouse || selector.IsEnteringFromDraw))
                continue;

            bool isHoveredCard = (i == hoveredIndex);

            // 기본 위치 계산(곡선 살짝)
            float offset = i - centerIndex;
            Vector3 finalPos = centerPos + rightDir * offset * spacing;

            float distFromCenter = Vector3.Distance(finalPos, centerPos);
            float lerpT = Mathf.Clamp01(distFromCenter / maxDist);
            float height = Mathf.Lerp(yCurveHeight, 0f, lerpT);
            finalPos.y = centerPos.y + height;

            // === 이웃 밀어내기 (가우시안 감쇠) ===
            if (hoveredIndex >= 0 && i != hoveredIndex)
            {
                int d = i - hoveredIndex;           // 좌(-), 우(+)
                float ad = Mathf.Abs(d);
                float push = neighborBasePush * Mathf.Exp(-(ad * ad) / (2f * neighborSigma * neighborSigma));
                finalPos += rightDir * push * Mathf.Sign(d);
            }

            // 회전
            float angleZ = (centerIndex == 0) ? 0f : -offset / centerIndex * rotationRange;
            if (isHoveredCard) angleZ = 0f; // 호버 카드 Z 회전 보정
            Quaternion rot = Quaternion.Euler(0, 0, angleZ);

            // 스케일
            Vector3 baseScale = new(0.7f, 1f, 1f);
            Vector3 scale = isHoveredCard ? baseScale * hoverScale : baseScale;

            // Y 리프트
            if (isHoveredCard) finalPos.y += hoverLiftY;

            // 트윈
            tCard.DOKill();
            tCard.DOMove(finalPos, hoverTween).SetEase(DG.Tweening.Ease.OutQuad);
            tCard.DORotateQuaternion(rot, hoverTween).SetEase(DG.Tweening.Ease.OutQuad);
            tCard.DOScale(scale, hoverTween).SetEase(DG.Tweening.Ease.OutQuad)
                .OnComplete(() =>
                {
                    // 베이스 포즈 스냅샷: 호버 상태는 스냅샷하지 않음
                    if (selector != null
                        && !selector.IsMovingToCenter
                        && !selector.IsSelected
                        && !selector.IsFollowingMouse
                        && !selector.IsEnteringFromDraw)
                    {
                        selector.ForceSaveOriginalTransform();
                    }
                });

            // Z-Order는 기존 순서 유지
            tCard.SetSiblingIndex(i);
        }
    }


    public List<GameObject> GetHandCards() => handCards;

    public Vector3 GetCenterPosition()
    {
        if (splineContainer != null)
        {
            var knots = splineContainer.Spline.Knots.ToList();
            if (knots.Count > 2)
            {
                Vector3 knot2Local = knots[2].Position;
                return splineContainer.transform.TransformPoint(knot2Local);
            }
        }
        return transform.position;
    }

    public void DiscardRandomCard()
    {
        if (handCards.Count == 0) return;

        int index = Random.Range(0, handCards.Count);
        GameObject card = handCards[index];

        handCards.RemoveAt(index);

        var cardData = card.GetComponent<CardDisplay>()?.cardData;
        DeckManager.Instance.UseCard(cardData); // ? 이 줄 추가

        card.transform.DOMove(discardPilePosition.position, 0.3f).SetEase(Ease.InCubic);
        card.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InCubic);
        Destroy(card, 0.3f);

        UpdateCardPositions();
    }

}
