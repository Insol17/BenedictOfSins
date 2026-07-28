using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private int originalSiblingIndex;

    public float hoverScale = 1.2f;
    public float hoverHeight = 20f;

    private RectTransform rectTransform;
    private HandManager handManager;
    private bool isHovering = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        handManager = FindObjectOfType<HandManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CardSelector.selectedCard == gameObject) return;
        if (!gameObject.activeInHierarchy) return;
        if (isHovering) return;

        var selector = GetComponent<CardSelector>();
        if (selector != null)
        {
            selector.IsHovered = true;
            Debug.Log($"[Hover] {gameObject.name} → IsHovered = true");
        }

        StartCoroutine(DelayedHover());
    }


    IEnumerator DelayedHover()
    {
        isHovering = true;
        yield return null; // 딜레이로 Enter/Exit 꼬임 방지

        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.localPosition;
        originalSiblingIndex = transform.GetSiblingIndex();

        transform.SetAsLastSibling();
        rectTransform.localScale = originalScale * hoverScale;
        rectTransform.localPosition = originalPosition + new Vector3(0f, hoverHeight, 0f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CardSelector.selectedCard == gameObject) return;
        if (!isHovering) return;

        var selector = GetComponent<CardSelector>();
        if (selector != null) selector.IsHovered = false;

        isHovering = false;

        rectTransform.localScale = originalScale;
        rectTransform.localPosition = originalPosition;
        transform.SetSiblingIndex(originalSiblingIndex);

        selector?.ForceSaveOriginalTransform(); // 💡 EXIT 시 정상 위치 저장

        handManager?.UpdateCardPositions();
    }
}
