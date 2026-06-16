using UnityEngine;

public class RageController : MonoBehaviour
{
    private IUnitStats unit;
    private bool isOverheatApplied = false;

    private const string Rage = "Rage";
    private const string Overheat = "Overheat";
    private const string OverheatEcho = "OverheatEcho";

    private void Awake()
    {
        unit = GetComponent<IUnitStats>();
        if (unit == null)
            Debug.LogError("[RageController] IUnitStats 컴포넌트가 없습니다.");
    }

    private void Update()
    {
        if (unit == null) return;

        int rageStack = TryGetRageStack();

        if (rageStack >= 11 && !isOverheatApplied)
        {
            Debug.Log("[RageController] Rage >= 11 → Overheat 부여");
            unit.ApplyBuff(Overheat, 1);
            isOverheatApplied = true;
        }
        else if (rageStack <= 10 && isOverheatApplied)
        {
            Debug.Log("[RageController] Rage <= 10 → Overheat 제거 + Echo 디버프");
            unit.RemoveBuff(Overheat);
            unit.ApplyDebuff(OverheatEcho, 1);
            isOverheatApplied = false;
        }
    }

    private int TryGetRageStack()
    {
        return unit.GetBuffStack(Rage);
    }
}
