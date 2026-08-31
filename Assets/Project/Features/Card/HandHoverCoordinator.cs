using System.Collections;
using UnityEngine;

public class HandHoverCoordinator : MonoBehaviour
{
    public static HandHoverCoordinator Instance { get; private set; }

    [Header("전환/차단 옵션")]
    [Range(0f, 0.2f)] public float hoverSwitchDebounce = 0.05f;
    public bool hoverBlocked = false;

    private float lastSwitchTime = -999f;

    // 현재 호버 대상 (신/구 공통)
    private GameObject currentGO;
    private CardHoverable currentHoverable;   // 구형(CardHoverable) 호환
    private HandLayout handLayout;            // 구형(HandLayout) 호환
    private int currentIndex = -1;

    public int CurrentHoveredIndex
    {
        get
        {
            if (currentGO == null) return -1;

            // 1) HandManager 기준(우선)
            var hm = FindObjectOfType<HandManager>();
            if (hm != null)
            {
                var list = hm.GetHandCards();
                return list != null ? list.IndexOf(currentGO) : -1;
            }

            // 2) HandLayout 기준(레거시)
            if (handLayout != null && currentHoverable != null)
                return handLayout.cards.IndexOf(currentHoverable);

            return -1;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // =========================
    // 레거시 API (구버전 코드 호환)
    // =========================
    public void RegisterLayout(HandLayout layout) => handLayout = layout;

    public void SetHover(CardHoverable target)
    {
        if (hoverBlocked || target == null) return;
        if (Time.unscaledTime - lastSwitchTime < hoverSwitchDebounce) return;

        currentHoverable = target;
        currentGO = target.gameObject;
        lastSwitchTime = Time.unscaledTime;

        Apply();
    }

    public void ClearHover(CardHoverable target)
    {
        if (target != null && currentHoverable != target) return;

        currentHoverable = null;
        currentGO = null;
        currentIndex = -1;

        Apply();
    }

    // =========================
    // 신규 API (경량 핸들러용)
    // =========================
    public void SetHoveredObject(GameObject go)
    {
        if (hoverBlocked || go == null) return;
        if (Time.unscaledTime - lastSwitchTime < hoverSwitchDebounce) return;

        currentGO = go;
        currentHoverable = go.GetComponent<CardHoverable>();
        lastSwitchTime = Time.unscaledTime;

        Apply();
    }

    public void ClearHover(GameObject go, bool force = false)
    {
        if (!force && currentGO != go) return;

        currentGO = null;
        currentHoverable = null;
        currentIndex = -1;

        Apply();
    }

    public void BlockHover(float seconds)
    {
        if (gameObject.activeInHierarchy) StartCoroutine(CoBlock(seconds));
        else { hoverBlocked = true; Invoke(nameof(UnblockHover), seconds); }
    }
    public void UnblockHover() => hoverBlocked = false;

    private IEnumerator CoBlock(float sec)
    {
        hoverBlocked = true;
        yield return new WaitForSecondsRealtime(sec);
        UnblockHover();
    }

    // 공통 적용 경로: HandManager/HandLayout 모두 지원
    private void Apply()
    {
        // HandManager 레이아웃 갱신
        var hm = FindObjectOfType<HandManager>();
        if (hm != null)
        {
            currentIndex = currentGO ? hm.GetHandCards().IndexOf(currentGO) : -1;
            hm.UpdateCardPositions();
        }

        // HandLayout 레거시 갱신
        if (handLayout != null)
        {
            int idx = (currentHoverable != null) ? handLayout.cards.IndexOf(currentHoverable) : -1;
            handLayout.ApplyHover(idx);
        }
    }

    private void OnDisable()
    {
        currentGO = null;
        currentHoverable = null;
        currentIndex = -1;
    }
}
