using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyStats : MonoBehaviour, IUnitStats
{
    [Header("Stats")]
    [SerializeField] private int maxHP = 80;
    [SerializeField] private int currentHP = 80;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public int Shield { get; set; } = 0;
    public float DebuffResistance => 0f;

    private Dictionary<string, IBuff> activeBuffs = new();
    private Dictionary<string, IDebuff> activeDebuffs = new();

    [SerializeField] private BuffIconUIController buffUI;
    [SerializeField] private MonoBehaviour aiComponent;
    private IEnemyAI ai;

    private void Awake()
    {
        ai = aiComponent as IEnemyAI;
        if (ai == null)
            Debug.LogError("[EnemyStats] 연결된 AI 컴포넌트가 IEnemyAI를 구현하지 않았습니다.");
    }

    public IEnumerator DoTurn()
    {
        Debug.Log("[EnemyStats] DoTurn 시작"); // ← 추가
        if (ai != null)
        {
            ai.PerformTurn();
            yield return new WaitForSeconds(0.5f);
        }
    }


    public void Heal(int amount) => currentHP = Mathf.Min(maxHP, currentHP + amount);
    public void SetShield(int value) => Shield = value;

    public void ReduceMaxHP(int amount)
    {
        maxHP = Mathf.Max(1, maxHP - amount);
        currentHP = Mathf.Min(currentHP, maxHP);
    }

    public void AddShield(int amount)
    {
        Shield += amount;
        Debug.Log($"[EnemyStats] 쉴드 +{amount} → 현재 쉴드: {Shield}");
    }

    public void TakeDamage(int amount, bool isDot = false)
    {
        foreach (var debuff in activeDebuffs.Values.ToList())
            debuff.OnTakeDamage(this, amount, isDot);

        int actualDamage = Mathf.Max(0, amount - Shield);
        Shield = Mathf.Max(0, Shield - amount);
        currentHP = Mathf.Max(0, currentHP - actualDamage);

        foreach (var buff in activeBuffs.Values.ToList())
            buff.OnTakeDamage(this, amount, isDot);

        // ? 적 사망 체크
        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"[EnemyStats] 적 {gameObject.name} 사망");
        gameObject.SetActive(false);

        if (CardManager.Instance.AreAllEnemiesDefeated())
        {
            CardManager.Instance.CheckAllEnemiesDefeated(); // ? 이렇게 수정
        }

    }


    public void OnTakeDamage(int amount, bool isDot = false) => TakeDamage(amount, isDot);

    public void ApplyBuff(string type, int stack)
    {
        if (!activeBuffs.ContainsKey(type))
        {
            var buff = BuffFactory.CreateBuff(type, stack);
            buff?.Apply(this);
            activeBuffs[type] = buff;
        }
        else activeBuffs[type].AddStack(stack);

        UpdateDebuffUI();
    }

    public void ApplyDebuff(string type, int stack)
    {
        int adjustedStack = Mathf.CeilToInt(stack * (1f - DebuffResistance));
        if (adjustedStack <= 0) return;

        if (!activeDebuffs.ContainsKey(type))
        {
            var debuff = BuffFactory.CreateDebuff(type, adjustedStack);
            debuff.Apply(this);
            activeDebuffs[type] = debuff;
        }
        else activeDebuffs[type].AddStack(adjustedStack);

        UpdateDebuffUI();
    }

    public void RemoveBuff(string type) => activeBuffs.Remove(type);
    public void RemoveDebuff(string type) => activeDebuffs.Remove(type);

    public void SetBuffStack(string type, int newStack)
    {
        if (activeBuffs.TryGetValue(type, out var buff))
        {
            RemoveBuff(type);
            ApplyBuff(type, newStack);
        }
    }

    public void AddStack(string type, int stack)
    {
        if (HasBuff(type)) ApplyBuff(type, stack);
        else if (HasDebuff(type)) ApplyDebuff(type, stack);
    }

    public bool HasBuff(string type) => activeBuffs.ContainsKey(type);
    public bool HasDebuff(string type) => activeDebuffs.ContainsKey(type);

    public int GetBuffStack(string type) => activeBuffs.TryGetValue(type, out var b) ? b.Stack : 0;
    public int GetDebuffStack(string type) => activeDebuffs.TryGetValue(type, out var d) ? d.Stack : 0;
    public IBuff GetBuff(string type) => activeBuffs.GetValueOrDefault(type);
    public IDebuff GetDebuff(string type) => activeDebuffs.GetValueOrDefault(type);

    public IEnumerable<IBuff> GetAllBuffs() => activeBuffs.Values;
    public IEnumerable<IDebuff> GetAllDebuffs() => activeDebuffs.Values;

    public void OnTurnStart()
    {
        Shield = 0;

        foreach (var buff in activeBuffs.Values)
        {
            buff.OnTurnStart(this); // 이 부분이 반드시 있어야 합니다!
        }

        foreach (var debuff in activeDebuffs.Values)
        {
            debuff.OnTurnStart(this);
        }
    }


    public void OnTurnEnd()
    {
        foreach (var buff in activeBuffs.Values) buff.OnTurnEnd(this);
        foreach (var debuff in activeDebuffs.Values) debuff.OnTurnEnd(this);

        foreach (var key in new List<string>(activeBuffs.Keys))
            if (activeBuffs[key].IsExpired()) activeBuffs.Remove(key);

        foreach (var key in new List<string>(activeDebuffs.Keys))
            if (activeDebuffs[key].IsExpired()) activeDebuffs.Remove(key);

        UpdateDebuffUI();
    }

    public void OnPlayerTurnStart()
    {
        Shield = 0;
        foreach (var buff in activeBuffs.Values) buff.OnPlayerTurnStart();
        foreach (var debuff in activeDebuffs.Values) debuff.OnPlayerTurnStart();
    }

    public void OnPlayerTurnEnd()
    {
        Shield = 0;
        foreach (var buff in activeBuffs.Values) buff.OnPlayerTurnEnd();
        foreach (var debuff in activeDebuffs.Values) debuff.OnPlayerTurnEnd();
    }

    public void OnEnemyTurnStart()
    {
        Shield = 0;
        foreach (var buff in activeBuffs.Values) buff.OnEnemyTurnStart();
        foreach (var debuff in activeDebuffs.Values) debuff.OnEnemyTurnStart();
    }

    public void OnEnemyTurnEnd()
    {
        Shield = 0;
        foreach (var buff in activeBuffs.Values) buff.OnEnemyTurnEnd();
        foreach (var debuff in activeDebuffs.Values) debuff.OnEnemyTurnEnd();
    }

    public void OnCardUsed(CardData card)
    {
        foreach (var buff in activeBuffs.Values) buff.OnCardUsed(this, card);
        foreach (var debuff in activeDebuffs.Values) debuff.OnCardUsed(this, card);
    }

    public void OnDealDamage(IUnitStats target, int damage)
    {
        foreach (var buff in activeBuffs.Values) buff.OnDealDamage(this, target, damage);
        foreach (var debuff in activeDebuffs.Values) debuff.OnDealDamage(this, target, damage);
    }

    public int ModifyEnergyGain(int baseGain)
    {
        int result = baseGain;
        foreach (var buff in activeBuffs.Values) result += buff.ModifyEnergyGain(baseGain);
        foreach (var debuff in activeDebuffs.Values) result += debuff.ModifyEnergyGain(baseGain);
        return result;
    }

    public void UpdateDebuffUI()
    {
        buffUI?.UpdateIcons(GetAllBuffs(), GetAllDebuffs());
    }

    public void UpdateBuffsPerTurn()
    {
        foreach (var buff in activeBuffs.Values) buff.Tick();
        foreach (var debuff in activeDebuffs.Values) debuff.Tick();

        foreach (var key in new List<string>(activeBuffs.Keys))
            if (activeBuffs[key].IsExpired()) activeBuffs.Remove(key);

        foreach (var key in new List<string>(activeDebuffs.Keys))
            if (activeDebuffs[key].IsExpired()) activeDebuffs.Remove(key);

        UpdateDebuffUI();

        if (ai != null)
            ai.ForceUpdateIntentUI();
    }

    public void PerformAction()
    {
        ai?.PerformAction();
    }

    public void PerformTurn()
    {
        OnTurnStart();
        PerformAction();
        OnTurnEnd();
    }

    public void AddOrUpdateBuff(string type, int stack)
    {
        if (activeBuffs.TryGetValue(type, out var buff))
        {
            if (buff == null)
            {
                Debug.LogError($"[EnemyStats] 기존 Buff '{type}' 가 null입니다.");
                return;
            }

            buff.AddStack(stack);
        }
        else
        {
            var newBuff = BuffFactory.CreateBuff(type, stack);
            if (newBuff == null)
            {
                Debug.LogError($"[EnemyStats] BuffFactory에서 '{type}' 생성 실패");
                return;
            }

            newBuff.Apply(this);
            activeBuffs[type] = newBuff;
        }

        buffUI?.UpdateIcons(activeBuffs.Values, activeDebuffs.Values);
    }

    public void AddOrUpdateDebuff(string type, int stack)
    {
        if (activeDebuffs.TryGetValue(type, out var debuff))
        {
            debuff.AddStack(stack);
        }
        else
        {
            var newDebuff = BuffFactory.CreateDebuff(type, stack);
            newDebuff.Apply(this);
            activeDebuffs[type] = newDebuff;
        }

        buffUI?.UpdateIcons(activeBuffs.Values, activeDebuffs.Values);
    }

    public void CalculateNextIntent()
    {
        ai?.CalculateNextIntent();
    }

    public void ForceUpdateIntentUI()
    {
        ai?.ForceUpdateIntentUI();
    }
}
