using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RewardItemUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image frameImage;
    [SerializeField] public Button claimButton;
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite cardIcon;

    public void SetupGold(int amount, Action onClaimed)
    {
        nameText.text = $"골드 ({amount})";
        iconImage.sprite = goldIcon;
        iconImage.gameObject.SetActive(true);
        frameImage.color = Color.yellow;

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() =>
        {
            GoldManager.Instance.AddGold(amount);
            claimButton.interactable = false;
            onClaimed?.Invoke();
        });
    }

    public void SetupCardButton(Action onClick)
    {
        nameText.text = "카드 선택";
        iconImage.sprite = cardIcon;
        frameImage.color = Color.cyan;

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() =>
        {
            onClick?.Invoke();
            // Destroy(gameObject); ? 제거 → UI 유지
        });
    }




    // 유물 보상 설정
    public void SetupRelic(RelicData relic, Action onClaimed)
    {
        nameText.text = relic.relicName;
        iconImage.sprite = relic.icon;
        frameImage.color = relic.isBossRelic ? Color.red : Color.white;

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() =>
        {
            RelicManager.Instance.AddRelic(relic);
            claimButton.interactable = false; // ? 파괴 대신 비활성화
            onClaimed?.Invoke();
        });
    }
}
