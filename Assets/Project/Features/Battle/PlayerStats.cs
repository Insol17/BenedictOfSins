using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 플레이어의 스탯 및 버프/디버프 상태를 관리하는 핵심 클래스.
/// - IUnitStats 인터페이스 구현: 유닛 공통 스탯 및 이벤트 인터페이스.
/// - GameObject에 부착되며, 외부에서 직접 호출하거나 이벤트 기반으로 메서드 호출됨.
/// - BuffFactory를 통해 생성된 IBuff / IDebuff 인스턴스를 적용하고 관리.
/// - 턴 이벤트, 피해 처리, 카드 사용 등 주요 게임 흐름과 직접 연결됨.
/// </summary>
public class PlayerStats : MonoBehaviour, IUnitStats
{
    // ========== 기본 스탯 ==========

    /// <summary> 최대 체력. 세이브 시 저장 대상. </summary>
    public int MaxHP { get; private set; } = 100;

    /// <summary> 현재 체력. 세이브 시 저장 대상. </summary>
    public int CurrentHP { get; private set; } = 100;

    /// <summary> 현재 실드 수치. 턴 시작 시 초기화됨. </summary>
    public int Shield { get; set; } = 0;

    /// <summary> 디버프 저항 수치. 현재는 0이며, 향후 시스템에 따라 증가 가능. </summary>
    public float DebuffResistance => 0f;

    // ========== 상태 효과 ==========

    /// <summary> 현재 적용 중인 버프 목록. key: string (이름), value: IBuff </summary>
    private Dictionary<string, IBuff> activeBuffs = new();

    /// <summary> 현재 적용 중인 디버프 목록. key: string (이름), value: IDebuff </summary>
    private Dictionary<string, IDebuff> activeDebuffs = new();

    // ========== 외부 참조 ==========

    /// <summary> UI 상에 버프/디버프 아이콘을 갱신해주는 컨트롤러 (월드 캔버스) </summary>
    [SerializeField] private BuffIconUIController buffUI;

    /// <summary> 핸드 카드 시스템을 제어하는 핸드 매니저 (카드 버리기용) </summary>
    [SerializeField] private HandManager handManager;

    // ========== 시작시 초기값 반영 ==========

    private void Start()
    {
        Debug.Log($"[PlayerStats:Start] 저장된 HP 적용 전 SaveManager: {SaveManager.Instance.CurrentPlayerHP}");

        // 자신이 준비됐음을 알림
        SaveManager.Instance?.OnPlayerStatsReady(this);

        if (SaveManager.Instance != null)
        {
            SetMaxHP(SaveManager.Instance.CurrentPlayerMaxHP);
            SetCurrentHP(SaveManager.Instance.CurrentPlayerHP);
        }

        Debug.Log($"[PlayerStats:Start] HP 최종 적용 결과: {CurrentHP}");
    }

    // ========== 체력 관련 ==========

    // 시작시 디버그
    void Awake()
    {
        Debug.Log("PlayerStats Awake");
    }

    /// <summary> 최대 체력 설정 </summary>
    public void SetMaxHP(int value)
    {
        MaxHP = Mathf.Max(1, value);

        // ? 저장
        if (SaveManager.Instance != null)
            SaveManager.Instance.CurrentPlayerMaxHP = MaxHP;
    }

    /// <summary> 현재 체력 직접 설정 </summary>
    public void SetCurrentHP(int value)
    {
        CurrentHP = Mathf.Clamp(value, 0, MaxHP);

        // ? 저장
        if (SaveManager.Instance != null)
            SaveManager.Instance.CurrentPlayerHP = CurrentHP;
    }

    /// <summary> 체력 회복 (최대 HP 초과 없음) </summary>
    public void Heal(int amount)
    {
        SetCurrentHP(CurrentHP + amount); // ? 저장도 반영됨

        if (SaveManager.Instance != null)
            SaveManager.Instance.CurrentPlayerHP = CurrentHP;
    }

    /// <summary> 실드 추가 </summary>
    public void AddShield(int amount) => Shield += amount;

    /// <summary> 실드 수치 설정 </summary>
    public void SetShield(int value) => Shield = value;

    /// <summary> 최대 체력 감소 (최소 1 보장), 현재 체력도 최대 체력을 넘지 않게 조정 </summary>
    public void ReduceMaxHP(int amount)
    {
        MaxHP = Mathf.Max(1, MaxHP - amount);
        CurrentHP = Mathf.Min(CurrentHP, MaxHP);
    }

    /// <summary>
    /// 데미지 처리 (실드 우선 차감 → 체력 차감)  
    /// - 모든 디버프/버프에게 OnTakeDamage 이벤트 전파  
    /// - DOT 여부로 디버프/버프의 반응이 달라질 수 있음
    /// </summary>
    public void TakeDamage(int amount, bool isDot = false)
    {
        foreach (var debuff in activeDebuffs.Values.ToList())
            debuff.OnTakeDamage(this, amount, isDot);

        int actualDamage = Mathf.Max(0, amount - Shield);
        Shield = Mathf.Max(0, Shield - amount);

        SetCurrentHP(CurrentHP - actualDamage);

        foreach (var buff in activeBuffs.Values.ToList())
            buff.OnTakeDamage(this, amount, isDot);

        // ? 체력 0 이하 시 사망 처리
        if (CurrentHP <= 0)
        {
            OnPlayerDeath();
        }
    }



    /// <summary> 인터페이스용 위임 메서드 </summary>
    public void OnTakeDamage(int amount, bool isDot = false) => TakeDamage(amount, isDot);

    // ========== 상태 효과 적용 ==========

    /// <summary> 버프 적용 또는 스택 추가. BuffFactory로 생성 </summary>
    public void ApplyBuff(string type, int stack)
    {
        if (!activeBuffs.ContainsKey(type))
        {
            var buff = BuffFactory.CreateBuff(type, stack);
            buff.Apply(this);
            activeBuffs[type] = buff;
        }
        else activeBuffs[type].AddStack(stack);

        UpdateDebuffUI();
    }

    /// <summary> 디버프 적용 또는 스택 추가. DebuffResistance 반영 후 적용 </summary>
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

    // ========== 상태 확인 및 조회 ==========

    public bool HasBuff(string type) => activeBuffs.ContainsKey(type);
    public bool HasDebuff(string type) => activeDebuffs.ContainsKey(type);

    public int GetBuffStack(string type) => activeBuffs.TryGetValue(type, out var b) ? b.Stack : 0;
    public int GetDebuffStack(string type) => activeDebuffs.TryGetValue(type, out var d) ? d.Stack : 0;

    public IBuff GetBuff(string type) => activeBuffs.GetValueOrDefault(type);
    public IDebuff GetDebuff(string type) => activeDebuffs.GetValueOrDefault(type);

    public IEnumerable<IBuff> GetAllBuffs() => activeBuffs.Values;
    public IEnumerable<IDebuff> GetAllDebuffs() => activeDebuffs.Values;

    // ========== 턴 처리 ==========

    private void ResetDefense()
    {
        // 1) 쉴드 0 (세터 경유로)
        SetShield(0);
       
        UpdateDebuffUI();
    }

    /// <summary> 턴 시작 시 실드 초기화 및 버프/디버프 OnTurnStart 호출 </summary>
    public void OnTurnStart()
    {
        // 먼저 방어를 싹 지움
        ResetDefense();

        if (HasBuff("Guard")) RemoveBuff("Guard");
        if (HasBuff("Shield")) RemoveBuff("ShieldUp");

        foreach (var buff in activeBuffs.Values.ToList()) buff.OnTurnStart(this);
        foreach (var debuff in activeDebuffs.Values.ToList()) debuff.OnTurnStart(this);

        UpdateDebuffUI();
    }

    /// <summary> 턴 종료 처리 및 만료된 버프/디버프 제거 </summary>
    public void OnTurnEnd()
    {
        foreach (var buff in activeBuffs.Values.ToList()) buff.OnTurnEnd(this);
        foreach (var debuff in activeDebuffs.Values.ToList()) debuff.OnTurnEnd(this);

        foreach (var key in new List<string>(activeBuffs.Keys))
            if (activeBuffs[key].IsExpired()) activeBuffs.Remove(key);

        foreach (var key in new List<string>(activeDebuffs.Keys))
            if (activeDebuffs[key].IsExpired()) activeDebuffs.Remove(key);

        UpdateDebuffUI();
    }

    public void OnPlayerTurnStart()
    {
        ResetDefense();

        foreach (var buff in activeBuffs.Values.ToList()) buff.OnPlayerTurnStart();
        foreach (var debuff in activeDebuffs.Values.ToList()) debuff.OnPlayerTurnStart();
    }

    public void OnPlayerTurnEnd()
    {
        foreach (var buff in activeBuffs.Values.ToList()) buff.OnPlayerTurnEnd();
        foreach (var debuff in activeDebuffs.Values.ToList()) debuff.OnPlayerTurnEnd();
    }

    public void OnEnemyTurnStart()
    {
        foreach (var buff in activeBuffs.Values.ToList()) buff.OnEnemyTurnStart();
        foreach (var debuff in activeDebuffs.Values.ToList()) debuff.OnEnemyTurnStart();
    }

    public void OnEnemyTurnEnd()
    {
        ResetDefense();

        foreach (var buff in activeBuffs.Values.ToList()) buff.OnEnemyTurnEnd();
        foreach (var debuff in activeDebuffs.Values.ToList()) debuff.OnEnemyTurnEnd();
    }

    // ========== 기타 트리거 ==========

    public void OnCardUsed(CardData card)
    {
        foreach (var buff in activeBuffs.Values.ToList()) buff.OnCardUsed(this, card);
        foreach (var debuff in activeDebuffs.Values.ToList()) debuff.OnCardUsed(this, card);
    }

    public void OnDealDamage(IUnitStats target, int damage)
    {
        foreach (var buff in activeBuffs.Values.ToList()) buff.OnDealDamage(this, target, damage);
        foreach (var debuff in activeDebuffs.Values.ToList()) debuff.OnDealDamage(this, target, damage);
    }

    public int ModifyEnergyGain(int baseGain)
    {
        int result = baseGain;
        foreach (var buff in activeBuffs.Values.ToList()) result += buff.ModifyEnergyGain(baseGain);
        foreach (var debuff in activeDebuffs.Values.ToList()) result += debuff.ModifyEnergyGain(baseGain);
        return result;
    }

    // ========== UI & 갱신 ==========

    /// <summary> UI 갱신 (버프/디버프 아이콘) </summary>
    public void UpdateDebuffUI() => buffUI?.UpdateIcons(GetAllBuffs(), GetAllDebuffs());

    /// <summary> 모든 버프/디버프 Tick 호출 후 만료 제거 및 UI 갱신 </summary>
    public void UpdateBuffsPerTurn()
    {
        foreach (var buff in activeBuffs.Values.ToList()) buff.Tick();
        foreach (var debuff in activeDebuffs.Values.ToList()) debuff.Tick();

        foreach (var key in new List<string>(activeBuffs.Keys))
            if (activeBuffs[key].IsExpired()) activeBuffs.Remove(key);

        foreach (var key in new List<string>(activeDebuffs.Keys))
            if (activeDebuffs[key].IsExpired()) activeDebuffs.Remove(key);

        UpdateDebuffUI();
    }

    // ========== 상태 직접 설정 (주로 외부 디버그 또는 툴에서 사용) ==========

    public void AddOrUpdateBuff(string type, int stack)
    {
        if (activeBuffs.TryGetValue(type, out var buff))
        {
            buff.AddStack(stack);
        }
        else
        {
            var newBuff = BuffFactory.CreateBuff(type, stack);
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

    /// <summary> 무작위 카드 버리기. 핸드에 카드 없거나 handManager 없으면 무시됨 </summary>
    public void DiscardRandomCard()
    {
        if (handManager != null)
            handManager.DiscardRandomCard();
        else
            Debug.LogWarning("[PlayerStats] HandManager가 연결되지 않았습니다.");
    }

    private void OnPlayerDeath()
    {
        Debug.Log("[PlayerStats] 플레이어 사망 - 씬 전환 시작");

        SaveManager.Instance?.SaveGame(SaveManager.Instance.CurrentSlotIndex);

        // 직접 static 방식으로 실행
        PlayerDeathSceneTransition.RunSceneTransition(
            nextSceneName: "GameOverScene",
            delayBefore: 0.5f,
            fadeOutDuration: 1.0f,
            fadeInDuration: 1.0f,
            delayAfter: 0.3f,
            fadeColor: Color.black
        );

        // 또는 PlayerDeathSceneTransition 컴포넌트를 찾아서 실행
        // GetComponent<PlayerDeathSceneTransition>()?.TriggerTransition();
    }

}
