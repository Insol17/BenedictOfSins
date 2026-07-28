using UnityEngine;

public interface IBuff
{
    string Type { get; }
    int Stack { get; }
    Sprite Icon { get; }

    void Apply(IUnitStats owner);
    void AddStack(int value);
    void Tick();
    bool IsExpired();

    // 전투 영향
    float GetDamageMultiplier();           // 주는 데미지
    float GetIncomingDamageMultiplier();   // 받는 데미지
    float GetShieldMultiplier();           // 얻는 쉴드 배율
    int GetCostModifier();                 // 코스트 증감

    // 트리거 훅
    void OnTurnStart(IUnitStats owner);
    void OnTurnEnd(IUnitStats owner);
    void OnPlayerTurnStart();
    void OnPlayerTurnEnd();
    void OnEnemyTurnStart();
    void OnEnemyTurnEnd();
    void OnCardUsed(IUnitStats owner, CardData card);
    void OnDealDamage(IUnitStats owner, IUnitStats target, int damage);
    void OnTakeDamage(IUnitStats owner, int damage, bool isDot);

    // 기타
    int ModifyEnergyGain(int baseGain); // 에너지 조정 (기본값 0 반환)
}
