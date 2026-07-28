using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandLayout : MonoBehaviour
{
    [Header("카드 리스트(자동)")]
    public List<CardHoverable> cards = new();

    [Header("기본 배치/호버 파라미터")]
    public float baseSpacing = 140f;       // 카드 간 간격(X)
    public float hoverLiftY = 200f;         // 호버된 카드 Y 상승량
    public float hoverScale = 1.3f;        // 호버된 카드 스케일
    public float tweenDuration = 0.14f;    // 트윈 시간
    public Ease tweenEase = Ease.OutQuad;

    [Header("이웃 밀어내기(Falloff)")]
    public float neighborBasePush = 60f;   // 가장 가까운 이웃 밀림 최대치
    [Range(0.5f, 3f)] public float sigma = 1.2f; // 감쇠 폭(가우시안 σ)
    public bool symmetricPush = true;      // 양옆 대칭 여부

    [Header("회전 보정")]
    public bool resetZRotationOnHover = true;

    private readonly List<Vector3> baseLocalPos = new();
    private readonly List<float> baseLocalRotZ = new();
    private readonly List<Vector3> baseLocalScale = new();

    private int hoveredIndex = -1;

    private void Start()
    {
        if (HandHoverCoordinator.Instance)
            HandHoverCoordinator.Instance.RegisterLayout(this);
    }

    /// <summary> 외부에서 카드 변경 시 호출 (추가/삭제 후) </summary>
    public void Rebind()
    {
        cards.Clear();
        GetComponentsInChildren(cards); // CardHoverable을 자식들에서 긁어온다
        cards.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        baseLocalPos.Clear();
        baseLocalRotZ.Clear();
        baseLocalScale.Clear();

        // 기본 라인 배치(수평). 이미 HandView가 배치했다면 그 값을 기준으로 스냅샷만 뜬다.
        for (int i = 0; i < cards.Count; i++)
        {
            var c = cards[i];
            c.CardIndex = i;

            // base pose 스냅샷
            baseLocalPos.Add(c.transform.localPosition);
            baseLocalRotZ.Add(c.transform.localEulerAngles.z);
            baseLocalScale.Add(c.transform.localScale);
        }

        ApplyHover(-1, immediate: true); // 초기화
    }

    public void ApplyHover(int index, bool immediate = false)
    {
        hoveredIndex = index;
        // 각 카드 목표 상태 계산
        for (int i = 0; i < cards.Count; i++)
        {
            var c = cards[i];
            c.KillTweens();

            // 기준값
            Vector3 targetPos = baseLocalPos[i];
            Vector3 targetScale = baseLocalScale[i];
            Vector3 targetEuler = c.transform.localEulerAngles;
            targetEuler.z = baseLocalRotZ[i];

            if (hoveredIndex >= 0)
            {
                // 이웃 밀어내기
                int d = i - hoveredIndex; // 0이면 중심
                if (d != 0)
                {
                    float ad = Mathf.Abs(d);
                    float push = neighborBasePush * Mathf.Exp(-(ad * ad) / (2f * sigma * sigma));
                    float sign = Mathf.Sign(d);
                    if (!symmetricPush) sign = d < 0 ? -1f : 0f; // 원한다면 한쪽만 밀 수 있음
                    targetPos.x += push * sign;
                }
            }

            // 중심 카드 처리
            if (i == hoveredIndex)
            {
                targetPos.y = baseLocalPos[i].y + hoverLiftY;
                targetScale = baseLocalScale[i] * hoverScale;

                if (resetZRotationOnHover)
                {
                    targetEuler.z = 0f; // Z 회전 보정
                }
            }

            if (immediate)
            {
                c.transform.localPosition = targetPos;
                c.transform.localScale = targetScale;
                c.transform.localEulerAngles = targetEuler;
            }
            else
            {
                c.transform.DOLocalMove(targetPos, tweenDuration).SetEase(tweenEase);
                c.transform.DOScale(targetScale, tweenDuration).SetEase(tweenEase);
                c.transform.DOLocalRotate(targetEuler, tweenDuration).SetEase(tweenEase);
            }
        }
    }

    /// <summary> 외부에서 재배치(정렬/스플라인 등) 완료 후 기본 포즈 갱신 </summary>
    public void SnapshotAsBase()
    {
        baseLocalPos.Clear();
        baseLocalRotZ.Clear();
        baseLocalScale.Clear();

        for (int i = 0; i < cards.Count; i++)
        {
            var t = cards[i].transform;
            baseLocalPos.Add(t.localPosition);
            baseLocalRotZ.Add(t.localEulerAngles.z);
            baseLocalScale.Add(t.localScale);
        }
    }

    public int GetHoveredIndex() => hoveredIndex;
}
