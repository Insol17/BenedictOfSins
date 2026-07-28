using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

/// <summary>
/// 카드 데이터 ScriptableObject.
/// - 카드 이름, 설명, 코스트(에너지/HP/분노), 기본 데미지, 타겟팅 정보, 효과 리스트 등 포함.
/// - 타임라인 연출, 이동/줌 애니메이션, 사용 후 소멸 여부도 지정 가능.
/// - CardEffectData 리스트로 복수 효과를 구성할 수 있음.
/// </summary>
[CreateAssetMenu(fileName = "NewCard", menuName = "Card/Create New Card")]
public class CardData : ScriptableObject
{
    [Header("기본 정보")]
    public string cardId;
    public string cardName;                     // 카드 이름
    [TextArea]
    public string description;                  // 카드 설명 텍스트
    public Sprite artwork;                      // 카드 일러스트 이미지

    [Header("코스트")]
    public int energyCost;                      // 에너지 소모량
    public int hpCost;                          // HP 소모량
    public int rageCost;                        // 분노 소모량

    [Header("전투 수치")]
    public int baseDamage;                      // 기본 데미지 수치

    [Header("타겟팅 설정")]
    public bool requiresTarget;                 // 대상 선택이 필요한 카드 여부

    [Header("카드 효과")]
    public List<CardEffectData> effects;        // 이 카드에 적용되는 효과 리스트

    [Header("연출 옵션")]
    public PlayableAsset timelineAsset;         // 카드 사용 시 재생할 타임라인 자산
    public bool requiresMovement;               // 카드 사용 시 적에게 이동하는 연출 포함 여부
    public bool requiresCameraZoom;             // 카메라 줌 연출 포함 여부

    [Header("기타 설정")]
    public bool isBanishedOnUse;                // 사용 후 카드가 소멸되는지 여부
    public CardRarity rarity;                   // 카드 희귀도

    /// <summary>
    /// 카드의 에너지/HP/분노 코스트를 문자열로 요약해서 반환.
    /// 예: "E:1 HP:5 R:2"
    /// </summary>
    public string GetCostSummary()
    {
        List<string> costParts = new();

        if (energyCost > 0)
            costParts.Add($"E:{energyCost}");
        if (hpCost > 0)
            costParts.Add($"HP:{hpCost}");
        if (rageCost > 0)
            costParts.Add($"R:{rageCost}");

        return string.Join(" ", costParts);
    }

}
