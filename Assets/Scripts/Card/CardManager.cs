using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using System.Linq;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("핸드 관련")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform drawPilePosition;
    [SerializeField] public Transform discardPilePosition;
    [SerializeField] private Transform handCenter;

    [Header("전투 관련")]
    public PlayerStats player;
    public List<EnemyStats> enemies;
    public EnergyManager energyManager;
    [SerializeField] private MapUIController mapUIController;

    [Header("핸드 및 턴")]
    public HandManager handManager;
    [SerializeField] private int cardsPerTurn = 5;
    public TurnState turnState = TurnState.PlayerTurn;

    public GameObject targetingArrow;

    public static event System.Action OnEnemyTurnStarted;
    public static event System.Action OnPlayerTurnStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(DrawCardsRoutine(cardsPerTurn));
    }

    public IEnumerator DrawCardsRoutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return new WaitForSeconds(0.05f);
            var cardData = DeckManager.Instance.DrawCard();
            if (cardData == null)
            {
                Debug.LogWarning("[CardManager] 카드 드로우 실패 (덱이 비었거나 오류)");
                continue;
            }

            handManager.DrawCard(cardData);
        }

        yield return new WaitForSeconds(0.3f);
        handManager.SaveAllCardTransforms();
    }

    public void DrawExtraCards(int count)
    {
        StartCoroutine(DrawExtraCardsRoutine(count));
    }

    private IEnumerator DrawExtraCardsRoutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CardData data = DeckManager.Instance.DrawCard();
            if (data == null) continue;

            Debug.Log($"[CardManager] 드로우 카드: {data.cardName}");
            handManager.DrawCard(data);
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.3f);
        handManager.SaveAllCardTransforms();
    }

    public void EndTurn()
    {
        if (CardSelector.selectedCard != null)
        {
            var selector = CardSelector.selectedCard.GetComponent<CardSelector>();

            // ? 파괴되었거나 컴포넌트 없는 경우 보호
            if (selector != null)
                selector.DeselectCard();

            // ? 확실하게 초기화
            CardSelector.selectedCard = null;
        }

        handManager.ClearHandWithAnimation();

        player.UpdateBuffsPerTurn();
        foreach (var e in enemies)
            e.UpdateBuffsPerTurn();

        // ?? 지연 효과 처리 (DelayedEffect)
        FindObjectOfType<CardEffectHandler>()?.OnTurnEnd();
    }

    public IEnumerator StartEnemyTurn()
    {
        turnState = TurnState.EnemyTurn;
        OnEnemyTurnStarted?.Invoke();

        foreach (EnemyStats enemy in enemies)
        {
            if (enemy != null && enemy.CurrentHP > 0)
            {
                yield return StartCoroutine(enemy.DoTurn());
            }
        }

        Debug.Log("적 턴 종료. 플레이어 턴 시작");
        yield return new WaitForSeconds(0.5f);

        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.CurrentHP > 0)
            {
                enemy.CalculateNextIntent();
                enemy.ForceUpdateIntentUI();
            }
        }

        OnPlayerTurnStarted?.Invoke();

        if (energyManager == null)
            energyManager = FindObjectOfType<EnergyManager>();

        energyManager.ResetEnergy();
        energyManager.GainEnergy(energyManager.energyPerTurn);

        StartCoroutine(DrawCardsRoutine(cardsPerTurn));
        turnState = TurnState.PlayerTurn;
    }

    public void DiscardCards(int amount)
    {
        Debug.Log($"[CardManager] 카드 {amount}장 버리기 (선택 시스템 필요)");
    }

    public void CheckAllEnemiesDefeated()
    {
        bool allDead = enemies.All(e => e == null || e.CurrentHP <= 0);

        if (allDead)
        {
            Debug.Log("[CardManager] 모든 적 처치 완료");

            // 적 전멸 시 보상 생성
            RewardManager.Instance.GenerateReward(EnemyType.Normal); // 상황에 따라 Elite, Boss

            // 다음 노드 자동 언락 처리
            var currentNode = MapManager.Instance.GetCurrentNode();
            if (currentNode != null)
            {
                MapManager.Instance.UnlockNextNodes(currentNode);
                Debug.Log("[CardManager] 다음 노드 자동 언락 완료");
            }

            // ? 자동 저장
            FindObjectOfType<InGameSaveManager>()?.SaveAfterEnemyDefeated();
        }
    }

    public bool AreAllEnemiesDefeated()
    {
        return enemies.All(e => e == null || e.CurrentHP <= 0);
    }
}
