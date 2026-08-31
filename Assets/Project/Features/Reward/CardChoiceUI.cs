using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 카드 보상 UI. 카드 3장을 보여주고 플레이어가 1장을 선택할 수 있음
/// </summary>
public class CardChoiceUI : MonoBehaviour
{
    [Header("UI 참조")]
    public GameObject cardPrefab;
    public Transform cardContainer;
    public Button skipButton;

    private Action<CardData> onCardChosen;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(List<CardData> cards, Action<CardData> onChosen)
    {
        gameObject.SetActive(true);
        Clear();
        onCardChosen = onChosen;

        foreach (var card in cards)
        {
            var cardObj = Instantiate(cardPrefab, cardContainer);
            var display = cardObj.GetComponent<RewardCardDisplay>();

            if (display != null)
            {
                display.Initialize(card, OnCardSelected);
            }
            else
            {
                Debug.LogError("[CardChoiceUI] RewardCardDisplay 컴포넌트가 카드 프리팹에 없습니다.");
            }
        }

        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(() =>
        {
            onCardChosen?.Invoke(null); // 스킵 시 null 전달
            Hide();
        });
    }

    private void OnCardSelected(CardData selectedCard)
    {
        onCardChosen?.Invoke(selectedCard);
        Hide();
    }

    private void Clear()
    {
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
