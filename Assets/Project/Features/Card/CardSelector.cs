using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class CardSelector : MonoBehaviour, IPointerClickHandler
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private int originalSiblingIndex;

    public static GameObject selectedCard = null;
    private bool isSelected = false;
    private bool isFollowingMouse = false;
    private bool isInHand = true;
    private bool isEnteringFromDraw = false;

    private Coroutine followMouseCoroutine = null;
    private CanvasGroup canvasGroup;
    private CardDisplay cardDisplay;
    private CardEffectHandler cardEffectHandler;

    public bool IsSelected => isSelected;
    public bool IsOriginalTransformSet => isOriginalTransformSet;
    public Vector3 OriginalLocalPosition => originalPosition;
    public bool IsMovingToCenter { get; private set; } = false;
    public bool IsEnteringFromDraw => isEnteringFromDraw;
    private bool isOriginalTransformSet = false;
    public bool IsHovered { get; set; } = false;
    public bool IsFollowingMouse => isFollowingMouse;

    private bool isBeingUsed = false;
    private bool hasBeenUsed = false;

    public CardData cardData;

    void Start()
    {
        cardDisplay = GetComponent<CardDisplay>();
        cardEffectHandler = FindObjectOfType<CardEffectHandler>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (selectedCard != null && selectedCard == gameObject && this == null)
        {
            selectedCard = null;
            return;
        }

        if (selectedCard != null && !IsMouseInScreen())
        {
            selectedCard.GetComponent<CardSelector>()?.DeselectCard();
        }

        if (selectedCard != null && Input.GetMouseButtonDown(1))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                selectedCard.GetComponent<CardSelector>()?.DeselectCard();
            }
        }
    }


    public void StartDrawAnimation() => isEnteringFromDraw = true;

    public void EndDrawAnimation()
    {
        isEnteringFromDraw = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (hasBeenUsed) return; //사용 완료된 카드 클릭 방지

        Vector2 mousePos = Input.mousePosition;
        bool isInUpper = mousePos.y > Screen.height * 0.2f;

        if (selectedCard != null && selectedCard != gameObject)
        {
            selectedCard.GetComponent<CardSelector>()?.DeselectCard();
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!isSelected) SelectCard();
            else if (isInUpper) UseCard();
            else DeselectCard();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isSelected || selectedCard == gameObject) DeselectCard();
        }
    }

    private void SelectCard()
    {
        if (!isInHand) return; //이미 사용된 카드 클릭 방지

        var energy = FindObjectOfType<EnergyManager>();
        var data = cardDisplay?.cardData;

        if (energy != null && data != null && energy.currentEnergy < data.energyCost)
        {
            FindObjectOfType<CostWarningUI>()?.ShowWarning("코스트 부족!");
            return;
        }

        isSelected = true;
        selectedCard = gameObject;
        transform.SetAsLastSibling();

        StopAllCoroutines();

        if (data != null && data.requiresTarget)
            followMouseCoroutine = StartCoroutine(FollowMouseUntilUpper());
        else
        {
            Vector3 targetPos = transform.localPosition + Vector3.up * 10f;
            transform.DOLocalMove(targetPos, 0.25f).SetEase(Ease.OutQuad);
            followMouseCoroutine = StartCoroutine(FollowMouse());
            ShowHoverUIBasedOnEffects();
        }
    }


    private bool IsMouseInScreen()
    {
        Vector2 mousePos = Input.mousePosition;
        return mousePos.x >= 0 && mousePos.y >= 0 &&
               mousePos.x <= Screen.width && mousePos.y <= Screen.height;
    }

    IEnumerator FollowMouseUntilUpper()
    {
        RectTransform rt = GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();
        if (rt == null || canvas == null) yield break;

        isFollowingMouse = true;

        while (isSelected && isFollowingMouse && isInHand)
        {
            Vector2 mousePos = Input.mousePosition;

            if (IsInUpperRegion(mousePos))
            {
                isFollowingMouse = false;
                MoveToCenterPosition();
                TargetClickListener.Instance.EnableTargeting(cardDisplay.cardData, gameObject);
                yield break;
            }

            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rt, mousePos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector3 worldPos
            );
            transform.position = worldPos;
            yield return null;
        }
    }

    IEnumerator FollowMouse()
    {
        RectTransform rt = GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();
        if (rt == null || canvas == null) yield break;

        isFollowingMouse = true;

        while (isSelected && isFollowingMouse && isInHand)
        {
            Vector2 mousePos = Input.mousePosition;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rt, mousePos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector3 worldPos
            );
            transform.position = worldPos;
            yield return null;
        }
    }

    private void ShowHoverUIBasedOnEffects()
    {
        if (cardDisplay?.cardData == null) return;

        foreach (var effect in cardDisplay.cardData.effects)
        {
            switch (effect.targetType)
            {
                case CardTargetType.Self:
                    var playerHover = FindObjectOfType<PlayerHoverUI>();
                    playerHover?.Show();
                    break;

                case CardTargetType.AllEnemy:
                    var allEnemies = FindObjectsOfType<EnemyHoverUI>();
                    foreach (var enemyUI in allEnemies)
                        enemyUI.Show();
                    break;
            }
        }
    }

    private void HideHoverUI()
    {
        FindObjectOfType<PlayerHoverUI>()?.Hide();
        var allEnemies = FindObjectsOfType<EnemyHoverUI>();
        foreach (var enemyUI in allEnemies)
            enemyUI.Hide();
    }

    private bool IsInUpperRegion(Vector2 mousePos) => mousePos.y > Screen.height * 0.2f;

    public void DeselectCard()
    {
        if (!isInHand) return; // 파괴 직전 카드 클릭 방지

        isSelected = false;
        selectedCard = null;
        isFollowingMouse = false;
        StopAllCoroutines();

        IsMovingToCenter = false;

        TargetClickListener.Instance?.DisableTargeting();
        HideHoverUI();

        FindObjectOfType<HandManager>()?.UpdateCardPositions();

        var arrowGO = CardManager.Instance?.targetingArrow;
        arrowGO?.GetComponent<BezierArrows>()?.ResetArrow();
        arrowGO?.SetActive(false);
        HideAllHoverUI();
    }

    public void UseCard()
    {
        if (!isInHand || !isSelected || isBeingUsed || hasBeenUsed) return;

        var effectHandler = FindObjectOfType<CardEffectHandler>();
        if (effectHandler != null && effectHandler.IsCardBeingPlayed)
        {
            Debug.Log("[CardSelector] 이전 카드가 아직 처리 중입니다.");
            return;
        }

        StopAllCoroutines();
        isFollowingMouse = false;
        isInHand = false;
        isBeingUsed = true;
        hasBeenUsed = true;

        DOTween.Kill(transform);
        isSelected = false;
        selectedCard = null;

        cardEffectHandler?.UseCardOnTarget(cardDisplay.cardData, null);
        OnCardUsed();
    }


    public void OnCardUsed()
    {
        // 묘지 처리 제거됨. 애니메이션만 남김
        var hand = FindObjectOfType<HandManager>();
        hand?.RemoveCardFromHand(gameObject);
        hand?.UpdateCardPositions();
        MoveToDiscardAndDestroy();
        TargetClickListener.Instance?.DisableTargeting();

        var arrowGO = CardManager.Instance?.targetingArrow;
        arrowGO?.GetComponent<BezierArrows>()?.ResetArrow();
        arrowGO?.SetActive(false);

        HideAllHoverUI();
    }


    private void MoveToDiscardAndDestroy()
    {
        if (this == null || transform == null) return;

        float duration = 0.4f;
        var discardPos = CardManager.Instance?.discardPilePosition;
        if (discardPos == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOJump(discardPos.position, 1.5f, 1, duration).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(Vector3.zero, duration).SetEase(Ease.InCubic));
        seq.Join(transform.DORotate(new Vector3(0, 0, -90f), duration).SetEase(Ease.InSine));
        seq.OnComplete(() => Destroy(gameObject));
    }


    public void MoveToCenterPosition()
    {
        var hand = FindObjectOfType<HandManager>();
        RectTransform rt = GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();
        if (hand == null || rt == null || canvas == null) return;

        var knots = hand.splineContainer.Spline.Knots.ToList();
        if (knots.Count <= 2) return;

        Vector3 uiLocalTarget = (Vector3)knots[2].Position + new Vector3(-498f, -100f, 0f);

        ForceSaveOriginalTransform();

        IsMovingToCenter = true;
        rt.DOKill();
        rt.localRotation = Quaternion.identity;
        rt.localScale = originalScale;
        CardManager.Instance?.targetingArrow?.SetActive(true);

        rt.DOLocalMove(uiLocalTarget, 0.3f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => IsMovingToCenter = false);
    }

    public void ForceSaveOriginalTransform()
    {
        if (!isInHand) return;
        if (IsHovered || IsMovingToCenter || IsSelected || isFollowingMouse || isEnteringFromDraw) return;

        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalScale = new Vector3(0.7f, 1f, 1f);
        originalSiblingIndex = transform.GetSiblingIndex();
        isOriginalTransformSet = true;
    }

    //HandManager 호환을 위해 추가
    public void SaveOriginalTransform()
    {
        ForceSaveOriginalTransform();
    }

    private void HideAllHoverUI()
    {
        var playerHover = FindObjectOfType<PlayerHoverUI>();
        playerHover?.Hide();

        var enemyHovers = FindObjectsOfType<EnemyHoverUI>();
        foreach (var enemy in enemyHovers)
            enemy.Hide();
    }
}