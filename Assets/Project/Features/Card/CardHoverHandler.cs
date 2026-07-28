using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class CardHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Refs")]
    [SerializeField] private CardSelector selector; // 인스펙터로 연결 가능

    [Header("Timing")]
    [Range(0f, 0.2f)] public float localDebounce = 0.02f;
    [Range(0f, 0.08f)] public float exitGrace = 0.03f;

    private float lastEventTime = -999f;
    private Coroutine exitRoutine;

    private void Reset()
    {
        // 에디터에서 컴포넌트 추가 시 자동 바인딩
        selector = GetComponent<CardSelector>() ?? GetComponentInParent<CardSelector>() ?? GetComponentInChildren<CardSelector>();
    }

    private void Awake()
    {
        if (!selector)
            selector = GetComponent<CardSelector>() ?? GetComponentInParent<CardSelector>() ?? GetComponentInChildren<CardSelector>();
    }

    private void TryResolveSelector()
    {
        if (!selector)
            selector = GetComponent<CardSelector>() ?? GetComponentInParent<CardSelector>() ?? GetComponentInChildren<CardSelector>();
    }

    public void OnPointerEnter(PointerEventData _)
    {
        if (Time.unscaledTime - lastEventTime < localDebounce) return;
        lastEventTime = Time.unscaledTime;

        TryResolveSelector();
        // CardSelector가 없으면 이 카드에서는 호버 로직을 건너뜀
        if (!selector) return;

        if (selector.IsSelected || selector.IsMovingToCenter || selector.IsFollowingMouse || selector.IsEnteringFromDraw)
            return;

        var hm = FindObjectOfType<HandManager>();
        if (hm == null || !hm.GetHandCards().Contains(gameObject)) return;

        selector.ForceSaveOriginalTransform();
        if (exitRoutine != null) { StopCoroutine(exitRoutine); exitRoutine = null; }

        selector.IsHovered = true;
        HandHoverCoordinator.Instance?.SetHoveredObject(gameObject);
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (Time.unscaledTime - lastEventTime < localDebounce) return;
        lastEventTime = Time.unscaledTime;

        // null 가드
        TryResolveSelector();
        if (selector) selector.IsHovered = false;

        if (exitRoutine != null) StopCoroutine(exitRoutine);
        exitRoutine = StartCoroutine(CoExitGrace());
    }

    private IEnumerator CoExitGrace()
    {
        yield return new WaitForSecondsRealtime(exitGrace);

        var coord = HandHoverCoordinator.Instance;
        var hm = FindObjectOfType<HandManager>();
        if (coord == null || hm == null) yield break;

        int idx = coord.CurrentHoveredIndex;
        var list = hm.GetHandCards();
        if (idx >= 0 && idx < list.Count && list[idx] == gameObject)
            coord.ClearHover(gameObject);

        exitRoutine = null;
    }

    private void OnDisable()
    {
        if (exitRoutine != null) StopCoroutine(exitRoutine);
        HandHoverCoordinator.Instance?.ClearHover(gameObject, force: true);
    }
}
