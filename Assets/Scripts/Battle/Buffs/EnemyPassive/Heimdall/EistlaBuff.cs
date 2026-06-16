using UnityEngine;

[Effect("Eistla", "빛나는 자")]
[EffectDescription("매 턴 무작위 카드 1장 버림\n공격 시마다 '광휘' 1 스택 부여")]
public class EistlaBuff : IBuff
{
    public string Type => "Eistla";
    public int Stack => 1;
    public Sprite Icon => IconDatabase.GetIcon("Eistla");

    private IUnitStats owner;

    // ? EffectRegistry에서 반드시 요구하는 생성자
    public EistlaBuff(int stack, IUnitStats owner)
    {
        this.owner = owner;
    }

    // 필요 시 디폴트 생성자도 유지 (컴파일러/툴 요구 대응)
    public EistlaBuff() { }

    public void Apply(IUnitStats owner)
    {
        this.owner = owner;
    }

    public void AddStack(int value)
    {
        // 스택 없음
    }

    public void Tick()
    {
        // 매 턴 핸드에서 랜덤 카드 1장 제거
        if (owner is PlayerStats player)
        {
            player.DiscardRandomCard(); // DiscardRandomCard는 HandManager 등에서 위임받아야 함
        }
    }

    public bool IsExpired() => false;

    public float GetDamageMultiplier() => 1f;
    public float GetIncomingDamageMultiplier() => 1f;
    public float GetShieldMultiplier() => 1f;
    public int GetCostModifier() => 0;

    public void OnDealDamage(IUnitStats dealer, IUnitStats target, int damage)
    {
        if (dealer == owner && target != null)
        {
            target.AddOrUpdateDebuff("Glow", 1);
        }
    }

    // 생략 가능한 기타 메서드들
    public void OnTurnStart(IUnitStats owner) { }
    public void OnTurnEnd(IUnitStats owner) { }
    public void OnPlayerTurnStart() { }
    public void OnPlayerTurnEnd() { }
    public void OnEnemyTurnStart() { }
    public void OnEnemyTurnEnd() { }
    public void OnCardUsed(IUnitStats owner, CardData card) { }
    public void OnTakeDamage(IUnitStats owner, int damage, bool isDot) { }

    public float GetDebuffResistanceMultiplier() => 1f;
    public int ModifyDebuffStack(int baseStack) => baseStack;
    public int ModifyEnergyGain(int originalGain) => originalGain;
}
