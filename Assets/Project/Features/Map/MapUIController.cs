using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class MapUIController : MonoBehaviour
{
    [Header("맵 드래그 및 위치")]
    public RectTransform mapRoot;
    public MapDragController dragController;

    [Header("맵 전체 UI 오브젝트")]
    public GameObject mapUI;

    [Header("맵 버튼들")]
    public Button hideMapButton;
    public Button showMapButton;

    [Header("애니메이션 위치 설정")]
    public Vector2 hiddenPosition = new Vector2(0f, -1080f); // 아래 숨김
    public float mapTweenDuration = 0.5f;

    [Header("Close 버튼 위치 설정")]
    public RectTransform hideMapButtonRect;
    public Vector2 hideButtonVisiblePosition = new Vector2(-400f, 0f);
    public Vector2 hideButtonHiddenPosition = new Vector2(-1000f, 0f);
    public float buttonTweenDuration = 0.3f;

    private void Awake()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.RegisterUIController(this);
        }

        if (hideMapButton != null)
            hideMapButton.onClick.AddListener(HideMap);

        if (showMapButton != null)
            showMapButton.onClick.AddListener(ShowMap);

        if (hideMapButtonRect != null)
            hideMapButtonRect.anchoredPosition = hideButtonHiddenPosition;

        HideMap(); // 시작 시 숨기기
    }


    public void ShowMapInstant()
    {
        if (mapUI != null)
            mapUI.SetActive(true);

        if (mapRoot != null)
            mapRoot.anchoredPosition = Vector2.zero;

        if (dragController != null)
        {
            dragController.enabled = true;
            dragController.ResetDragState(); // ✅ 여기
        }


        if (hideMapButtonRect != null)
            hideMapButtonRect.anchoredPosition = hideButtonVisiblePosition;
    }

    public void ShowMap()
    {
        if (mapRoot == null || mapUI == null)
        {
            Debug.LogWarning("[MapUIController] 맵 Root 또는 UI 누락");
            return;
        }

        mapUI.SetActive(true);
        dragController.enabled = false;
        dragController.ResetDragState();

        // === 1. 초기 위치: 맵 Root를 아래에 고정 (숨겨진 상태)
        mapRoot.anchoredPosition = hiddenPosition;

        // === 2. 보스 라인 기준 위치 ===
        float bossY = Mathf.Clamp(lineCenterY[14], dragController.minY, dragController.maxY);

        // === 3. 현재 노드 위치 ===
        var currentNode = MapManager.Instance?.GetCurrentNode();
        float targetY = 0f;

        if (currentNode != null)
        {
            int lineIndex = Mathf.Clamp(currentNode.y, 0, lineCenterY.Length - 1);
            float desiredCenterY = lineCenterY[lineIndex];
            targetY = Mathf.Clamp(desiredCenterY, dragController.minY, dragController.maxY);
        }
        else
        {
            Debug.LogWarning("[MapUIController] currentNode가 null입니다. 기본 위치로 이동합니다.");
        }

        // === 4. 트윈 시퀀스 (보스 → 현재 노드로 이동) ===
        mapRoot.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.Append(mapRoot.DOAnchorPos(new Vector2(0f, bossY), 0.5f) // 처음 보스로 올라오기
                          .SetEase(Ease.OutCubic))
           .AppendInterval(0.5f) // 0.5초 정지
           .Append(mapRoot.DOAnchorPos(new Vector2(0f, targetY), 1f) // 현재 노드로 천천히 내려감
                          .SetEase(Ease.InOutCubic))
           .OnComplete(() =>
           {
               dragController.enabled = true;

               if (hideMapButtonRect != null)
               {
                   hideMapButtonRect.DOAnchorPos(hideButtonVisiblePosition, buttonTweenDuration)
                                    .SetEase(Ease.OutBack);
               }
           });
    }


    public void HideMap()
    {
        if (dragController != null)
            dragController.enabled = false;

        mapRoot.DOKill();
        mapRoot.DOAnchorPos(hiddenPosition, 0.4f)
               .SetEase(Ease.InCubic)
               .OnComplete(() =>
               {
                   if (mapUI != null)
                       mapUI.SetActive(false);
               });

        if (hideMapButtonRect != null)
        {
            hideMapButtonRect.DOAnchorPos(hideButtonHiddenPosition, buttonTweenDuration)
                             .SetEase(Ease.InBack);
        }
    }

    private float[] lineCenterY = new float[15]
    {
    1000f,  // 0번 라인 (시작)
    860f,
    720f,
    580f,
    440f,
    300f,
    160f,
    20f,
    -120f,
    -260f,
    -400f,
    -540f,
    -680f,
    -820f,
    -960f  // 14번 라인 (보스)
    };

    /// <summary>
    /// 전투나 휴식, 상점 등 특정 행동 이후 호출. 현재 노드의 다음 노드를 해금하고 맵을 표시.
    /// </summary>
    public void ShowMapWithUnlock()
    {
        var mapManager = MapManager.Instance;
        if (mapManager == null)
        {
            Debug.LogError("[MapUIController] MapManager 인스턴스 없음");
            return;
        }

        // ✅ 현재 노드의 다음 노드 해금
        var currentNode = mapManager.GetCurrentNode();
        if (currentNode != null)
        {
            mapManager.UnlockNextNodes(currentNode);
        }
        else
        {
            Debug.LogWarning("[MapUIController] 현재 노드가 null이라 노드 해금을 생략합니다.");
        }

        // ✅ 맵 띄우기
        ShowMap();
    }
}
