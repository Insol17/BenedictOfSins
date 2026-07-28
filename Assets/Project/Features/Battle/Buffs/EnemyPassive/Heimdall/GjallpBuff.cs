using UnityEngine;

[Effect("Gjallp", "울부짖는 자")]
[EffectDescription("피해를 받을 때마다 종말의 파동 스택 +1\n스택당 공격력 +3% 증가")]
public class GjallpBuff : IBuff
{
    public string Type => "Gjallp";
    public int Stack => 1;
    public Sprite Icon => IconDatabase.GetIcon("Gjallp");
    private IUnitStats owner;
    public GjallpBuff(int stack, IUnitStats owner) => this.owner = owner;
    public GjallpBuff() { }
    public void Apply(IUnitStats owner) => this.owner = owner;
    public void AddStack(int value) { }
    public void Tick() { }
    public bool IsExpired() => false;
    public float GetDamageMultiplier() => 1f;
    public float GetIncomingDamageMultiplier() => 1f;
    public float GetShieldMultiplier() => 1f;
    public int GetCostModifier() => 0;
    public void OnTakeDamage(IUnitStats owner, int dmg, bool dot)
    {
        if (!dot && dmg > 0) owner.AddOrUpdateBuff("DoomWave", 1);
    }
    public void OnTurnStart(IUnitStats o) { }
    public void OnTurnEnd(IUnitStats o) { }
    public void OnPlayerTurnStart() { }
    public void OnPlayerTurnEnd() { }
    public void OnEnemyTurnStart() { }
    public void OnEnemyTurnEnd() { }
    public void OnDealDamage(IUnitStats d, IUnitStats t, int dmg) { }
    public void OnCardUsed(IUnitStats o, CardData c) { }
    public float GetDebuffResistanceMultiplier() => 1f;
    public int ModifyDebuffStack(int baseStack) => baseStack;
    public int ModifyEnergyGain(int originalGain) => originalGain;
}