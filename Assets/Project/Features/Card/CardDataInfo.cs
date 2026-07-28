using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 카드 정보를 단순하게 저장하는 직렬화 가능한 클래스.
/// - 카드 ID, 이름, 에너지 코스트를 포함.
/// - SaveData 등에 카드 정보를 간단히 저장할 때 사용.
/// </summary>
[System.Serializable]
public class CardDataInfo
{
    /// <summary> 카드 고유 ID (예: "fireball_001") </summary>
    public string cardId;

    /// <summary> 카드 이름 (예: "화염구") </summary>
    public string cardName;

    /// <summary> 카드 에너지 코스트 </summary>
    public int energyCost;
}
