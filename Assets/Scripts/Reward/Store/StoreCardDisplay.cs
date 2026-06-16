using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 상점에서 판매하는 카드 UI를 표시하고, 구매 처리까지 담당하는 컴포넌트
/// </summary>
public class StoreCardDisplay : CardDisplay
{
    [Header("상점 UI 요소")]
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    private new CardData cardData; // CardDisplay의 cardData를 숨기기 (CS0108 경고 방지)
    private int price;

    /// <summary>
    /// 카드와 가격을 UI에 설정하고 구매 처리 바인딩
    /// </summary>
    public void Initialize(CardData data, int cost)
    {
        cardData = data;
        price = cost;

        // 카드 UI 표시 (CardDisplay에서 상속됨)
        SetCard(data);

        // 가격 표시
        if (priceText != null)
            priceText.text = $"{price}G";

        // 버튼 설정
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
        else
        {
            Debug.LogWarning("[StoreCardDisplay] BuyButton이 연결되지 않았습니다.");
        }
    }

    /// <summary>
    /// 구매 버튼 클릭 시 처리
    /// </summary>
    private void OnBuyButtonClicked()
    {
        if (!GoldManager.Instance.SpendGold(price))
        {
            Debug.Log("[Store] 골드가 부족합니다.");
            return;
        }

        DeckManager.Instance.AddCardToAllDeck(cardData, 1);
        DeckManager.Instance.AddCardToDeck(cardData, 1);

        buyButton.interactable = false; // ? 여기 잘 들어오는지 로그 찍어보기

        Debug.Log($"[Store] 카드 구매 완료: {cardData.cardName} ({price}G)");
    }
}
