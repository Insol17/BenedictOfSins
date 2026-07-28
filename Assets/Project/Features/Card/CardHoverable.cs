using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class CardHoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("가드 플래그")]
    public bool isInHand = true;          // 핸드에 있을 때만 호버 허용
    public bool isEnteringFromDraw = false; // 드로우 직후 잠깐 호버 금지
    [Range(0f, 0.3f)] public float drawHoverBlockTime = 0.08f;

    [Header("디바운스")]
    [Range(0f, 0.2f)] public float localDebounce = 0.02f; // 동일 카드 재입력 최소 간격

    // 내부 상태
    private float lastLocalEventTime = -999f;
    private bool isHovered = false;
    private bool tweenLocked = false;

    // 캐시
    private Tween moveTw, rotTw, scaleTw;
    public int CardIndex { get; set; } = -1;

    private void OnEnable()
    {
        // 드로우 직후 보호
        if (isEnteringFromDraw)
        {
            if (HandHoverCoordinator.Instance)
                HandHoverCoordinator.Instance.BlockHover(drawHoverBlockTime);
            Invoke(nameof(ClearEnteringFromDraw), drawHoverBlockTime);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInHand) return;
        if (isEnteringFromDraw) return;
        if (Time.unscaledTime - lastLocalEventTime < localDebounce) return;

        lastLocalEventTime = Time.unscaledTime;
        if (HandHoverCoordinator.Instance)
            HandHoverCoordinator.Instance.SetHover(this);

        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInHand) return;
        if (isEnteringFromDraw) return;
        if (Time.unscaledTime - lastLocalEventTime < localDebounce) return;

        lastLocalEventTime = Time.unscaledTime;

        if (HandHoverCoordinator.Instance)
            HandHoverCoordinator.Instance.ClearHover(this);

        isHovered = false;
    }

    public void InternalUnhover(bool immediate)
    {
        if (tweenLocked) return;

        // 이 카드는 레이아웃이 원상복귀를 맡으므로 여기선 플래그만
        isHovered = false;
    }

    public void KillTweens()
    {
        moveTw?.Kill(); moveTw = null;
        rotTw?.Kill(); rotTw = null;
        scaleTw?.Kill(); scaleTw = null;
    }

    private void ClearEnteringFromDraw() => isEnteringFromDraw = false;
}
