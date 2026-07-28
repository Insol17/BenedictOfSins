using UnityEngine;

[Effect("Atla", "장엄한 자")]
[EffectDescription("- 매 턴 방어 +20\n- 디버프 효과 -50%")]
public class AtlaBuff : IBuff
{
    public string Type => "Atla";
    public int Stack => 1;
    public Sprite Icon => IconDatabase.GetIcon("Atla");

    private IUnitStats owner;

    public AtlaBuff(int stack, IUnitStats owner) => this.owner = owner;
    public AtlaBuff() { }

    public void Apply(IUnitStats owner) => this.owner = owner;
    public void AddStack(int value) { }
    public bool IsExpired() => false;

    public void Tick() { } // 더 이상 여기서 쉴드 추가 안 함

    public float GetDamageMultiplier() => 1f;
    public float GetIncomingDamageMultiplier() => 1f;
    public float GetShieldMultiplier() => 1f;
    public float GetDebuffResistanceMultiplier() => 0.5f;
    public int GetCostModifier() => 0;

    public int ModifyDebuffStack(int baseStack) => Mathf.FloorToInt(baseStack * 0.5f);
    public int ModifyEnergyGain(int originalGain) => originalGain;

    public void OnDealDamage(IUnitStats d, IUnitStats t, int dmg) { }
    public void OnTakeDamage(IUnitStats o, int dmg, bool dot) { }
    public void OnCardUsed(IUnitStats o, CardData c) { }

    public void OnTurnStart(IUnitStats o)
    {
        o.AddShield(20); // 매 턴 시작 시 방어 +20
        Debug.Log("[AtlaBuff] 방어 +20 적용됨");
    }

    public void OnTurnEnd(IUnitStats o) { }
    public void OnPlayerTurnStart() { }
    public void OnPlayerTurnEnd() { }
    public void OnEnemyTurnStart() { }
    public void OnEnemyTurnEnd() { }
}
