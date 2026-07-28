using System.Collections.Generic;
using UnityEngine;

public class StoreUIController : MonoBehaviour
{
    [Header("카드 슬롯 설정")]
    [SerializeField] private Transform cardSlotContainer;       // 카드 슬롯을 배치할 부모 오브젝트
    [SerializeField] private GameObject cardSlotPrefab;         // StoreCardDisplay가 붙은 프리팹

    [Header("가격 범위")]
    [SerializeField] private int minPrice = 50;
    [SerializeField] private int maxPrice = 100;

    /// <summary>
    /// 상점에 카드 아이템을 표시하는 메서드
    /// </summary>
    public void ShowCardItems(List<CardData> cards)
    {
        // 기존 슬롯 정리
        foreach (Transform child in cardSlotContainer)
        {
            Destroy(child.gameObject);
        }

        // 새로운 슬롯 생성 및 초기화
        foreach (var card in cards)
        {
            GameObject slotObj = Instantiate(cardSlotPrefab, cardSlotContainer);
            StoreCardDisplay slot = slotObj.GetComponent<StoreCardDisplay>();

            int price = Random.Range(minPrice, maxPrice + 1);
            slot.Initialize(card, price);
        }

        Debug.Log($"[StoreUIController] 카드 {cards.Count}개 표시됨.");
    }
}
