// MapNodeView.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections;

public class MapNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public Button button;
    public MapNode node;
    private RectTransform rectTransform;
    private MapManager manager;

    [Header("UI 요소")]
    [SerializeField] private GameObject visitedMark;
    [SerializeField] private Image outlineImage; // 아웃라인 이미지
    [SerializeField] private GameObject clickEffectPrefab; // 선택 애니메이션 이펙트
    [SerializeField] private CanvasGroup raycastBlocker; // 마우스 반응 차단용

    [Header("색상 설정")]
    [SerializeField] private Color defaultOutlineColor = Color.black;
    [SerializeField] private Color highlightOutlineColor = Color.white;
    [SerializeField] private Color normalIconColor = Color.white;
    [SerializeField] private Color visitedIconColor = new Color(0.5f, 0.5f, 0.5f); // 회색톤
    [SerializeField] private Color passedIconColor = new Color(0.3f, 0.3f, 0.3f); // 지나온 노드 (더 진함)

    [Header("스케일 설정")]
    [SerializeField] private float bossScale = 1.5f;
    [SerializeField] private float bossHoverScale = 1.65f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float normalHoverScale = 1.1f;

    private Vector3 originalScale;
    private bool isHovered = false;
    private bool isBoss = false;

    public void Setup(MapNode node, Sprite icon)
    {
        this.node = node;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        iconImage.sprite = icon;
        if (outlineImage != null)
            outlineImage.sprite = icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        // ? 고정 기준은 항상 1로 시작
        originalScale = Vector3.one;

        if (node.type == NodeType.Boss)
        {
            isBoss = true;
            transform.localScale = originalScale * bossScale;
        }
        else
        {
            isBoss = false;
            transform.localScale = originalScale * normalScale;
        }
    }

    public void SetupPosition(int x, int y)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        float xOffset = Random.Range(-30f, 30f);
        float yOffset = Random.Range(-20f, 20f);

        float baseX = x * 180f + xOffset;
        float baseY = y * 200f + yOffset;

        if (node.type == NodeType.Boss)
            baseY += 300f;

        rectTransform.anchoredPosition = new Vector2(baseX, baseY);
    }

    public void SetManager(MapManager mgr)
    {
        manager = mgr;
        UpdateState();
    }

    public void UpdateState()
    {
        bool isAvailable = node.isUnlocked && !node.isCleared;
        button.interactable = isAvailable;

        if (visitedMark != null)
            visitedMark.SetActive(node.visited);

        if (raycastBlocker != null)
            raycastBlocker.blocksRaycasts = !node.isUnlocked && node.visited;

        if (outlineImage != null)
            outlineImage.color = defaultOutlineColor;

        if (iconImage != null)
        {
            if (node.visited)
                iconImage.color = passedIconColor;
            else
                iconImage.color = node.isUnlocked ? normalIconColor : visitedIconColor;
        }


        // ? 크기 유지
        if (isBoss)
            transform.localScale = originalScale * bossScale;
        else
            transform.localScale = originalScale * normalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!node.isUnlocked && node.visited)
            return;

        isHovered = true;

        float targetScale = isBoss ? bossHoverScale : normalHoverScale;
        transform.DOScale(originalScale * targetScale, 0.15f).SetEase(Ease.OutBack);

        // ? 오직 상호작용 가능한 경우에만 아웃라인 강조
        if (button.interactable && outlineImage != null)
            outlineImage.color = highlightOutlineColor;
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovered) return;

        isHovered = false;

        float baseScale = isBoss ? bossScale : normalScale;
        transform.DOScale(originalScale * baseScale, 0.15f).SetEase(Ease.OutCubic);

        if (outlineImage != null)
            outlineImage.color = defaultOutlineColor;
    }

    private void OnClick()
    {
        if (!manager.CanEnter(node)) return;

        if (clickEffectPrefab != null)
        {
            var effect = Instantiate(clickEffectPrefab, transform.position, Quaternion.identity, transform);
            Destroy(effect, 1f);
        }

        button.interactable = false;
        StartCoroutine(DelayedEnter());
    }

    private IEnumerator DelayedEnter()
    {
        yield return new WaitForSeconds(0.4f);
        manager.EnterNode(node);
    }
}