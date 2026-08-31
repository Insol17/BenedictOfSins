using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 보상 카드 선택 UI에서 사용하는 카드 표시 및 클릭 처리 컴포넌트
/// </summary>
public class RewardCardDisplay : CardDisplay
{
    [Header("버튼")]
    public Button button;

    private Action<CardData> onSelected;

    public void Initialize(CardData data, Action<CardData> onClick)
    {
        // 부모 클래스(CardDisplay)의 메서드 호출로 카드 렌더링 처리
        SetCard(data); // 또는 DisplayCard(data) ? CardDisplay에 어떤 게 정의되어 있는지에 따라

        onSelected = onClick;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelected?.Invoke(data));
        }
        else
        {
            Debug.LogWarning("[RewardCardDisplay] Button이 연결되지 않았습니다.");
        }
    }

}
