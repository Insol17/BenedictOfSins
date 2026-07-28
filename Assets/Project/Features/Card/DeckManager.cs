using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    [System.Serializable]
    public class CardStack
    {
        public CardData card;
        public int quantity;

        public CardStack(CardData card, int quantity)
        {
            this.card = card;
            this.quantity = quantity;
        }
    }

    public List<CardStack> allDeck = new(); // 전체 덱
    public List<CardStack> currentDeck = new(); // 인게임에서 쓰이는 뽑을 카드 더미
    public List<CardStack> graveyard = new(); // 인게임에서 쓰이는 버린 카드 더미
    public List<CardStack> banished = new(); // 인게임에서 쓰이는 소멸된 카드 더미

    public TextMeshProUGUI deckCountText;
    public TextMeshProUGUI graveyardCountText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ? 씬 전환 시 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Update()
    {
        UpdateUI();
    }

    public void SetDeck(List<CardData> cards)
    {
        allDeck.Clear();
        currentDeck.Clear();

        if (cards == null || cards.Count == 0)
        {
            Debug.LogWarning("[DeckManager] SetDeck: 입력 카드가 비어 있음.");
            UpdateUI();
            return;
        }

        foreach (var card in cards)
        {
            if (card != null)
            {
                allDeck.Add(new CardStack(card, 1));
            }
        }

        CopyAllDeckToCurrent();
        UpdateUI();
        Debug.Log($"[DeckManager] 초기 덱 세팅 완료: {allDeck.Count}종, 총 {GetTotalDeckCount()}장");
    }

    private void CopyAllDeckToCurrent()
    {
        currentDeck.Clear();
        foreach (var stack in allDeck)
        {
            currentDeck.Add(new CardStack(stack.card, stack.quantity));
        }
    }


    public void AddCardToDeck(CardData card, int quantity = 1)
    {
        var existing = currentDeck.Find(s => s.card == card);
        if (existing != null) existing.quantity += quantity;
        else currentDeck.Add(new CardStack(card, quantity));
    }

    public void UseCard(CardData card)
    {
        if (card == null) return;

        if (card.isBanishedOnUse)
            AddToCollection(banished, card);
        else
            AddToCollection(graveyard, card);

        UpdateUI();
    }

    public void ExileCard(CardData card)
    {
        if (card == null) return;
        AddToCollection(banished, card);
        UpdateUI();
    }


    public CardData DrawCard()
    {
        EnsureDeckIsNotEmpty();

        List<CardData> flatList = new();
        foreach (var stack in currentDeck)
            for (int i = 0; i < stack.quantity; i++)
                flatList.Add(stack.card);

        if (flatList.Count == 0) return null;

        int index = Random.Range(0, flatList.Count);
        CardData drawn = flatList[index];

        RemoveCardFromDeck(drawn);
        return drawn;
    }

    private void EnsureDeckIsNotEmpty()
    {
        int total = 0;
        foreach (var s in currentDeck) total += s.quantity;

        if (total == 0 && graveyard.Count > 0)
            RecycleGraveyard();
    }

    private void RemoveCardFromDeck(CardData card)
    {
        var stack = currentDeck.Find(s => s.card == card);
        if (stack != null)
        {
            stack.quantity--;
            if (stack.quantity <= 0)
                currentDeck.Remove(stack);
        }
    }

    private void AddToCollection(List<CardStack> collection, CardData card)
    {
        var existing = collection.Find(s => s.card == card);
        if (existing != null) existing.quantity++;
        else collection.Add(new CardStack(card, 1));
    }

    public void RecycleGraveyard()
    {
        foreach (var stack in graveyard)
            AddCardToDeck(stack.card, stack.quantity);
        graveyard.Clear();
    }

    public void ReturnAllToDeck()
    {
        foreach (var stack in graveyard)
            AddCardToDeck(stack.card, stack.quantity);
        graveyard.Clear();

        foreach (var stack in banished)
            AddCardToDeck(stack.card, stack.quantity);
        banished.Clear();
    }

    private void TryFindUIText()
    {
        if (deckCountText == null)
        {
            var deckObj = GameObject.FindWithTag("DeckCountText");
            if (deckObj != null)
            {
                deckCountText = deckObj.GetComponent<TextMeshProUGUI>();
                Debug.Log("[DeckManager] 덱 카운트 텍스트 자동 연결 성공");
            }
        }

        if (graveyardCountText == null)
        {
            var graveObj = GameObject.FindWithTag("GraveyardCountText");
            if (graveObj != null)
            {
                graveyardCountText = graveObj.GetComponent<TextMeshProUGUI>();
                Debug.Log("[DeckManager] 묘지 텍스트 자동 연결 성공");
            }
        }
    }


    private void UpdateUI()
    {
        // 자동 연결 시도
        TryFindUIText();

        int deckTotal = 0, graveTotal = 0;
        foreach (var stack in currentDeck) deckTotal += stack.quantity;
        foreach (var stack in graveyard) graveTotal += stack.quantity;

        if (deckCountText != null) deckCountText.text = deckTotal.ToString();
        if (graveyardCountText != null) graveyardCountText.text = graveTotal.ToString();
    }


    public int GetTotalDeckCount()
    {
        int total = 0;
        foreach (var stack in currentDeck) total += stack.quantity;
        return total;
    }

    public void RestoreDeckFromAllDeck()
    {
        CopyAllDeckToCurrent();
        graveyard.Clear();
        banished.Clear();
        UpdateUI();
    }

    //저장 관련
    public List<SavedCardStack> GetSavedAllDeck()
    {
        List<SavedCardStack> list = new();
        foreach (var stack in allDeck)
        {
            if (!string.IsNullOrEmpty(stack.card.cardId))
            {
                list.Add(new SavedCardStack
                {
                    cardId = stack.card.cardId,
                    quantity = stack.quantity
                });
            }
        }
        return list;
    }

    public void LoadAllDeck(List<SavedCardStack> savedDeck)
    {
        allDeck.Clear();
        graveyard.Clear();
        banished.Clear();

        foreach (var saved in savedDeck)
        {
            var card = GetCardById(saved.cardId);
            if (card != null)
            {
                allDeck.Add(new CardStack(card, saved.quantity));
            }
            else
            {
                Debug.LogWarning($"[DeckManager] cardId '{saved.cardId}'에 해당하는 카드 없음.");
            }
        }

        CopyAllDeckToCurrent();
        UpdateUI();
    }

    private CardData GetCardById(string id)
    {
        var preset = SaveManager.Instance?.currentChapter;
        if (preset == null) return null;

        return preset.cardPool.Find(c => c.cardId == id); // 또는 Dictionary로 바꿔도 좋음
    }

    public void AddCardToAllDeck(CardData card, int quantity = 1)
    {
        var existing = allDeck.Find(s => s.card == card);
        if (existing != null) existing.quantity += quantity;
        else allDeck.Add(new CardStack(card, quantity));
    }
}

[System.Serializable]
public class SavedCardStack
{
    public string cardId;  // CardData의 고유 ID
    public int quantity;   // 장 수
}