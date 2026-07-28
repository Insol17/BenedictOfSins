using UnityEngine;

[Effect("Overheat", "오버히트")]
[EffectDescription("- 턴 당 분노 4 소모\n- 분노 10스택 이하 도달 시 해제 및 오버히트 잔흔 부여\n- 주는 데미지 +50%\n- 얻는 방어력 -50%")]
public class OverheatBuff : IBuff
{
    public string Type => "Overheat";
    public int Stack => 1;
    public Sprite Icon => IconDatabase.GetIcon("Overheat");

    private IUnitStats owner;

    // 기본 생성자 (아이콘 로딩용)
    public OverheatBuff() { }

    public OverheatBuff(int stack, IUnitStats owner)
    {
        this.owner = owner;
    }

    public void Apply(IUnitStats owner)
    {
        this.owner = owner;
    }

    public void AddStack(int value)
    {
        // 오버히트는 최대 1스택 유지
    }

    public void Tick()
    {
        if (owner == null) return;

        // 매 턴 분노 4 소모
        if (owner.HasBuff("Rage"))
        {
            int currentRage = owner.GetBuffStack("Rage");
            int newRage = Mathf.Max(0, currentRage - 4);
            owner.SetBuffStack("Rage", newRage);

            if (newRage <= 10)
            {
                owner.RemoveBuff("Overheat");
                owner.AddOrUpdateDebuff("OverheatEcho", 1);
            }
        }
    }

    public bool IsExpired() => false;

    public float GetDamageMultiplier() => 1.5f;
    public float GetIncomingDamageMultiplier() => 1f;
    public float GetShieldMultiplier() => 0.5f;
    public int GetCostModifier() => 0;
    public int ModifyEnergyGain(int baseGain) => baseGain;

    public void OnTurnStart(IUnitStats owner) { }
    public void OnTurnEnd(IUnitStats owner) { }

    public void OnPlayerTurnStart() { }
    public void OnPlayerTurnEnd() { }
    public void OnEnemyTurnStart() { }
    public void OnEnemyTurnEnd() { }

    public void OnCardUsed(IUnitStats owner, CardData card) { }
    public void OnDealDamage(IUnitStats source, IUnitStats target, int amount) { }
    public void OnTakeDamage(IUnitStats owner, int amount, bool isFromPlayer) { }
}
