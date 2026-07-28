using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class SlideMenuController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] RectTransform panel;     // 왼쪽 패널
    [SerializeField] Button openButton;       // 메뉴(햄버거) 버튼
    [SerializeField] Button closeButton;      // X 버튼(있으면)
    [SerializeField] Image backdrop;          // 어두운 배경(선택). 클릭 시 닫힘

    [Header("Motion")]
    [SerializeField] float slideDistance = 240f; // 오른쪽으로 이동할 거리
    [SerializeField] float duration = 0.28f;
    [SerializeField] Ease ease = Ease.OutCubic;
    [SerializeField] bool fadeBackdrop = true;

    [Header("Events")]
    public UnityEvent onOpened;
    public UnityEvent onClosed;

    Vector2 closedPos;     // 기본(닫힘) 위치
    Vector2 openedPos;     // 열림 위치
    bool isOpen = false;
    bool isAnimating = false;
    CanvasGroup backdropCg;

    void Awake()
    {
        closedPos = panel.anchoredPosition;
        openedPos = closedPos + new Vector2(slideDistance, 0f);

        if (backdrop != null)
        {
            if (!backdrop.TryGetComponent(out backdropCg))
                backdropCg = backdrop.gameObject.AddComponent<CanvasGroup>();

            // 시작은 숨김
            backdropCg.alpha = 0f;
            backdrop.raycastTarget = false;
            backdrop.gameObject.SetActive(false);

            // 바깥 클릭 닫기
            var btn = backdrop.GetComponent<Button>();
            if (btn == null) btn = backdrop.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(Close);
        }

        if (openButton) openButton.onClick.AddListener(Open);
        if (closeButton) closeButton.onClick.AddListener(Close);
    }

    void Update()
    {
        // ESC로 닫기
        if (isOpen && !isAnimating && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Open()
    {
        if (isOpen || isAnimating) return;
        isAnimating = true;

        if (backdrop != null)
        {
            backdrop.gameObject.SetActive(true);
            backdrop.raycastTarget = true;
            if (fadeBackdrop) backdropCg.DOFade(0.5f, duration * 0.9f);
            else backdropCg.alpha = 0.5f;
        }

        panel.DOAnchorPos(openedPos, duration)
             .SetEase(ease)
             .OnComplete(() =>
             {
                 isOpen = true;
                 isAnimating = false;
                 onOpened?.Invoke();
             });
    }

    public void Close()
    {
        if (!isOpen || isAnimating) return;
        isAnimating = true;

        if (backdrop != null)
        {
            backdrop.raycastTarget = false;
            if (fadeBackdrop) backdropCg.DOFade(0f, duration * 0.8f)
                                        .OnComplete(() => backdrop.gameObject.SetActive(false));
            else
            {
                backdropCg.alpha = 0f;
                backdrop.gameObject.SetActive(false);
            }
        }

        panel.DOAnchorPos(closedPos, duration)
             .SetEase(ease)
             .OnComplete(() =>
             {
                 isOpen = false;
                 isAnimating = false;
                 onClosed?.Invoke();
             });
    }

    public void Toggle() { if (isOpen) Close(); else Open(); }
}
