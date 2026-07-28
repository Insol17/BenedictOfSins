using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUIManager : MonoBehaviour
{
    [Header("UI 참조")]
    public GameObject rewardPanel;
    public Button skipButton;

    public Transform rewardListContainer;
    public GameObject rewardItemPrefab; // 골드/유물/카드 공통 프리팹

    public CardChoiceUI cardChoiceUI; // 카드 선택 UI

    private Reward currentReward;
    private bool cardChosen = false;
    private bool relicClaimed = false;
    private bool goldClaimed = false;

    private void Awake()
    {
        rewardPanel.SetActive(false); // 게임 시작 시 숨기기
    }


    public void Show(Reward reward)
    {
        currentReward = reward;
        rewardPanel.SetActive(true);

        // ?? 상태 초기화
        cardChosen = false;
        relicClaimed = !reward.isRelicGiven;
        goldClaimed = reward.gold <= 0;

        skipButton.interactable = true;
        skipButton.GetComponentInChildren<TMP_Text>().text = "보상 스킵";
        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(OnSkipClicked);

        // ?? 기존 리스트 정리 필요
        foreach (Transform child in rewardListContainer)
        {
            Destroy(child.gameObject);
        }
        cardRewardUI = null;

        // 보상 생성
        if (reward.gold > 0)
            CreateGoldReward(reward.gold);

        CreateCardReward();

        if (reward.isRelicGiven)
            CreateRelicReward(reward.relic);
    }



    private void CreateGoldReward(int amount)
    {
        var goldItem = Instantiate(rewardItemPrefab, rewardListContainer);
        var rewardUI = goldItem.GetComponent<RewardItemUI>();
        rewardUI.SetupGold(amount, () =>
        {
            goldClaimed = true;
            CheckAllRewardsClaimed();
        });
    }

    private void CreateRelicReward(RelicData relic)
    {
        var relicItem = Instantiate(rewardItemPrefab, rewardListContainer);
        var rewardUI = relicItem.GetComponent<RewardItemUI>();
        rewardUI.SetupRelic(relic, () =>
        {
            relicClaimed = true;
            CheckAllRewardsClaimed();
        });
    }

    private RewardItemUI cardRewardUI;

    private void CreateCardReward()
    {
        if (cardRewardUI != null)
            Destroy(cardRewardUI.gameObject); // ?? 이전 버튼 제거 (중복 방지)

        var cardItem = Instantiate(rewardItemPrefab, rewardListContainer);
        cardRewardUI = cardItem.GetComponent<RewardItemUI>();

        cardRewardUI.SetupCardButton(() =>
        {
            // ?? 보상 UI 끄고 카드 보상 UI 켜기
            rewardPanel.SetActive(false);

            cardChoiceUI.Show(currentReward.cardOptions, (selectedCard) =>
            {
                if (selectedCard != null)
                {
                    OnCardChosen(selectedCard); // ? 카드 추가 & 버튼 비활성화
                    cardRewardUI.claimButton.interactable = false;
                }
                else
                {
                    // ?스킵한 경우: 다시 카드 선택 가능해야 하므로 버튼 활성화
                    cardRewardUI.claimButton.interactable = true;
                }

                rewardPanel.SetActive(true); // 다시 보상 UI 보여줌
            });
        });
    }

    private void OnCardChosen(CardData selected)
    {
        cardChosen = true;

        // ? allDeck에 직접 추가
        DeckManager.Instance.AddCardToAllDeck(selected);

        // ? currentDeck도 정리
        DeckManager.Instance.RestoreDeckFromAllDeck();

        if (cardRewardUI != null)
            cardRewardUI.claimButton.interactable = false;

        SaveManager.Instance.SaveGame(SaveManager.Instance.CurrentSlotIndex);
        CheckAllRewardsClaimed();
    }


    private void OnSkipClicked()
    {
        Debug.Log("[RewardUIManager] 보상 스킵 시도");

        // 스킵 시 모든 보상을 '받은 것으로 간주' 처리
        goldClaimed = true;
        relicClaimed = true;
        cardChosen = true;

        // 보상 창 닫기
        rewardPanel.SetActive(false);

        // 보상 상태 저장
        RewardManager.Instance.MarkRewardClaimed();
        SaveManager.Instance.ClearPendingReward(); // 보상 대기 상태 초기화
        SaveManager.Instance.SaveGame(SaveManager.Instance.CurrentSlotIndex);

        // ? 맵 UI만 보여줌 (씬 전환 없이)
        MapManager.Instance?.ShowMap();
    }


    private void CheckAllRewardsClaimed()
    {
        if (AllRewardsClaimed())
        {
            skipButton.GetComponentInChildren<TMP_Text>().text = "진행하기";
        }
    }

    private bool AllRewardsClaimed()
    {
        return cardChosen && goldClaimed && relicClaimed;
    }
}
