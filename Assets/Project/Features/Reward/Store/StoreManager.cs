using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public StoreType storeType;

    public List<CardData> availableCards;
    public List<RelicData> availableRelics;

    public GameObject storeUI;
    public StoreDialogueUI dialogueUI;

    [SerializeField] private StoreUIController storeUIController; // ? 추가

    void Start()
    {
        SetupStore();
    }

    public void SetupStore()
    {
        if (SaveManager.Instance.currentChapter == null)
        {
            Debug.LogError("[StoreManager] currentChapter가 null입니다! 기본 프리셋을 수동 설정해야 합니다.");
            return;
        }

        GenerateBasicStoreInventory();

        if (availableCards == null || availableCards.Count == 0)
        {
            Debug.LogError("[StoreManager] availableCards가 비어 있습니다. ShowCardItems 호출 생략.");
            return;
        }

        storeUIController.ShowCardItems(availableCards); // ? 문제 해결됨
        storeUI.SetActive(true);
    }

    // ? 누락된 메서드 정의
    private void GenerateBasicStoreInventory()
    {
        availableCards = CardDatabase.GetRandomCards(5);
        availableRelics = new List<RelicData>();
    }
}
