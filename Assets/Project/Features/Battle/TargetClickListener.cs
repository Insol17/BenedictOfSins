using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetClickListener : MonoBehaviour
{
    private CardData currentCard;
    private GameObject cardObject;
    private bool targetingEnabled = false;

    public EnemyHoverUI currentHoveredEnemy;
   
    private List<EnemyHoverUI> allEnemyHovers;
    private PlayerHoverUI playerHoverUI;
    private CardEffectHandler effectHandler;

    public static bool IsTargeting = false;

    public static TargetClickListener Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public void SetTargeting(bool value)
    {
        IsTargeting = value;
    }

    void Start()
    {
        allEnemyHovers = FindObjectsOfType<EnemyHoverUI>().ToList();
        playerHoverUI = FindObjectOfType<PlayerHoverUI>();
        effectHandler = FindObjectOfType<CardEffectHandler>();
    }

    void Update()
    {
        if (!targetingEnabled || currentCard == null || cardObject == null)
            return;

        // Hover 처리: 지정형만 감지
        if (currentCard.effects.Any(e => e.requiresTarget))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var hover = hit.collider.GetComponentInParent<EnemyHoverUI>();
                if (hover != null && hover != currentHoveredEnemy)
                {
                    currentHoveredEnemy?.SetHover(false);
                    hover.SetHover(true);
                    currentHoveredEnemy = hover;
                }
            }
            else
            {
                currentHoveredEnemy?.SetHover(false);
                currentHoveredEnemy = null;
            }
        }

        // 클릭 처리
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject target = hit.collider.gameObject;

                var targetingEffect = currentCard.effects.FirstOrDefault(e => e.requiresTarget);
                if (targetingEffect == null) return;

                if (IsValidTarget(target, targetingEffect.targetType))
                {
                    effectHandler.UseCardOnTarget(currentCard, target);
                    cardObject.GetComponent<CardSelector>()?.OnCardUsed();
                    DisableTargeting();
                }
            }
        }
    }

    public void EnableTargeting(CardData card, GameObject cardObj)
    {
        currentCard = card;
        cardObject = cardObj;
        targetingEnabled = true;

        var effect = card.effects.FirstOrDefault();
        if (effect != null && !effect.requiresTarget)
        {
            if (effect.targetType == CardTargetType.Self && playerHoverUI != null)
                playerHoverUI.SetHover(true);

            if (effect.targetType == CardTargetType.AllEnemy)
                foreach (var ui in allEnemyHovers)
                    ui.SetHover(true);
        }
    }

    public void DisableTargeting()
    {
        currentCard = null;
        cardObject = null;
        targetingEnabled = false;
        DisableAllHoverUI();
    }

    private void DisableAllHoverUI()
    {
        currentHoveredEnemy?.SetHover(false);
        currentHoveredEnemy = null;

        foreach (var ui in allEnemyHovers)
            ui.SetHover(false);

        playerHoverUI?.SetHover(false);
    }

    private bool IsValidTarget(GameObject target, CardTargetType type)
    {
        if (type == CardTargetType.Enemy && target.GetComponentInParent<EnemyStats>() != null)
            return true;
        if (type == CardTargetType.Self && target.CompareTag("Player"))
            return true;
        return false;
    }

    public CardData GetCurrentCard()
    {
        return currentCard;
    }

}
