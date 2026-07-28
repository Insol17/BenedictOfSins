using System.Collections.Generic;
using UnityEngine;

public interface IUnitStats
{
    int MaxHP { get; }
    int CurrentHP { get; }
    int Shield { get; }

    float DebuffResistance { get; } // 예: 0.5f → 50% 저항

    void TakeDamage(int amount, bool isDot = false);
    void Heal(int amount);
    void AddShield(int amount);
    void ReduceMaxHP(int amount);
    void SetShield(int value);

    void ApplyBuff(string effectName, int stack);
    void ApplyDebuff(string effectName, int stack);
    void RemoveBuff(string effectName);
    void RemoveDebuff(string effectName);
    void SetBuffStack(string effectName, int newStack);

    void AddStack(string effectName, int stack);

    void AddOrUpdateBuff(string type, int stack);
    void AddOrUpdateDebuff(string type, int stack);

    bool HasBuff(string effectName);
    bool HasDebuff(string effectName);

    int GetBuffStack(string effectName);
    int GetDebuffStack(string effectName);

    IBuff GetBuff(string effectName);
    IDebuff GetDebuff(string effectName);

    IEnumerable<IBuff> GetAllBuffs();
    IEnumerable<IDebuff> GetAllDebuffs();

    void OnTurnStart();   // 턴 시작 훅
    void OnTurnEnd();     // 턴 종료 훅
    void OnPlayerTurnStart();
    void OnPlayerTurnEnd();
    void OnEnemyTurnStart();
    void OnEnemyTurnEnd();
    void OnCardUsed(CardData card); // 카드 사용 훅
    void OnDealDamage(IUnitStats target, int damage); // 공격 시 훅
    void OnTakeDamage(int amount, bool isDot); // 피해 시 훅

    int ModifyEnergyGain(int baseGain); // 에너지 변화 조정 (예: OverheatEcho)
}
