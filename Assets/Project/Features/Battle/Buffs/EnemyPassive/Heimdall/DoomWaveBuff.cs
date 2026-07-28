using UnityEngine;

[Effect("DoomWave", "종말의 파동")]
[EffectDescription("- 피해를 받을 때마다 1스택 획득\n- 스택 당 공격력 +3% 증가")]
public class DoomWaveBuff : IBuff
{
    public string Type => "DoomWave";
    public int Stack => stack;
    public Sprite Icon => IconDatabase.GetIcon("DoomWave");

    private const int MaxStack = 999;
    private int stack;
    private IUnitStats owner;

    public DoomWaveBuff(int stack, IUnitStats owner)
    {
        this.stack = Mathf.Min(stack, MaxStack);
        this.owner = owner;
    }

    public DoomWaveBuff() { }

    public void Apply(IUnitStats owner) => this.owner = owner;

    public void AddStack(int value) => stack = Mathf.Min(stack + value, MaxStack);

    public void Tick() { }

    public bool IsExpired() => false; // 이 버프는 영구 지속

    public float GetDamageMultiplier() => 1f + 0.03f * stack;

    public float GetIncomingDamageMultiplier() => 1f;

    public float GetShieldMultiplier() => 1f;

    public int GetCostModifier() => 0;

    public void OnTurnStart(IUnitStats owner) { }

    public void OnTurnEnd(IUnitStats owner) { }

    public void OnPlayerTurnStart() { }
    public void OnPlayerTurnEnd() { }
    public void OnEnemyTurnStart() { }
    public void OnEnemyTurnEnd() { }

    public void OnCardUsed(IUnitStats owner, CardData card) { }

    public void OnTakeDamage(IUnitStats owner, int damage, bool isDot)
    {
        if (!isDot) AddStack(1);
    }

    public void OnDealDamage(IUnitStats dealer, IUnitStats target, int damage) { }

    public int ModifyDebuffStack(int baseStack) => baseStack;

    public int ModifyEnergyGain(int originalGain) => originalGain;
}
