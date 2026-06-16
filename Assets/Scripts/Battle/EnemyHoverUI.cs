using UnityEngine;
using System.Linq;

public class EnemyHoverUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup hoverUI;
    [SerializeField] private BoxCollider targetCollider;
    [SerializeField] private float fadeDuration = 0.1f;

    private Camera mainCamera;
    private bool isHovering = false;
    private Coroutine fadeRoutine;

    private static EnemyHoverUI currentHoveredEnemy = null;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (targetCollider == null)
            Debug.LogError($"[EnemyHoverUI] targetCollider가 지정되지 않았습니다. {gameObject.name}");

        SetAlpha(0f);
        hoverUI.interactable = false;
        hoverUI.blocksRaycasts = false;
    }

    private void Update()
    {
        CardData selectedCard = null;

        if (CardSelector.selectedCard != null)
        {
            var display = CardSelector.selectedCard.GetComponent<CardDisplay>();
            if (display != null)
                selectedCard = display.cardData;
        }

        if (selectedCard == null || !selectedCard.requiresTarget ||
            !selectedCard.effects.Any(e =>
                (e.targetType == CardTargetType.Enemy || e.targetType == CardTargetType.AllEnemy)))
        {
            if (isHovering)
            {
                Hide();
                isHovering = false;
                currentHoveredEnemy = null;
            }
            return;
        }

        bool isOver = IsMouseOver();

        if (isOver)
        {
            if (!isHovering || currentHoveredEnemy != this)
            {
                Show();
                isHovering = true;
                currentHoveredEnemy = this;
            }
        }
        else if (isHovering)
        {
            Hide();
            isHovering = false;

            if (currentHoveredEnemy == this)
                currentHoveredEnemy = null;
        }
    }

    private bool IsMouseOver()
    {
        if (targetCollider == null) return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit, 100f) && hit.collider == targetCollider;
    }

    public void SetHover(bool state)
    {
        if (state)
        {
            Show();
            isHovering = true;
            currentHoveredEnemy = this;
        }
        else
        {
            Hide();
            isHovering = false;

            if (currentHoveredEnemy == this)
                currentHoveredEnemy = null;
        }
    }

    public void Show()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[EnemyHoverUI] Show() 호출 시 비활성 상태: {gameObject.name}");
            return;
        }

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeTo(1f));
        hoverUI.interactable = true;
        hoverUI.blocksRaycasts = true;

        if (CardSelector.selectedCard != null)
        {
            var cardDisplay = CardSelector.selectedCard.GetComponent<CardDisplay>();
            var enemyStats = GetComponent<IUnitStats>();
            if (cardDisplay != null && enemyStats != null)
            {
                cardDisplay.PreviewDamageWithTarget(enemyStats);
            }
        }
    }

    public void Hide()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[EnemyHoverUI] Hide() 호출 시 비활성 상태: {gameObject.name}");
            return;
        }

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeTo(0f));
        hoverUI.interactable = false;
        hoverUI.blocksRaycasts = false;

        if (CardSelector.selectedCard != null)
        {
            var cardDisplay = CardSelector.selectedCard.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                cardDisplay.UpdateCardUI();
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        if (hoverUI != null)
            hoverUI.alpha = alpha;
    }

    private System.Collections.IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = hoverUI.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            hoverUI.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            yield return null;
        }

        hoverUI.alpha = targetAlpha;
    }
}
