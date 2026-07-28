using System.Collections.Generic;
using UnityEngine;

public static class DamageCalculator
{
    public static int CalculateOutgoingDamage(IUnitStats owner, int baseDamage)
    {
        float multiplier = 1f;

        foreach (var buff in owner.GetAllBuffs())
            multiplier *= buff.GetDamageMultiplier();

        foreach (var debuff in owner.GetAllDebuffs())
            multiplier *= debuff.GetDamageMultiplier();

        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);
        Debug.Log($"[DamageCalc] Outgoing: {baseDamage} ¡æ {finalDamage} (x{multiplier:F2})");
        return finalDamage;
    }

    public static int CalculateIncomingDamage(IUnitStats owner, int baseDamage, bool isDot = false)
    {
        float additiveBonus = 0f;

        foreach (var debuff in owner.GetAllDebuffs())
            additiveBonus += debuff.GetIncomingDamageMultiplier(isDot) - 1f;

        foreach (var buff in owner.GetAllBuffs())
            additiveBonus += buff.GetIncomingDamageMultiplier() - 1f;

        float totalMultiplier = 1f + additiveBonus;

        int final = Mathf.RoundToInt(baseDamage * totalMultiplier);
        Debug.Log($"[DamageCalc] Incoming: {baseDamage} ¡æ {final} (x{totalMultiplier:F2})");
        return final;
    }

    public static int GetModifiedShield(IUnitStats owner, int baseShield)
    {
        float modifier = 1f;

        foreach (var buff in owner.GetAllBuffs())
            modifier *= buff.GetShieldMultiplier();

        foreach (var debuff in owner.GetAllDebuffs())
            modifier *= debuff.GetShieldMultiplier();

        int final = Mathf.RoundToInt(baseShield * modifier);
        Debug.Log($"[DamageCalc] Shield: {baseShield} ¡æ {final} (x{modifier:F2})");
        return final;
    }

    public static int CalculateFinalDamageWithTarget(IUnitStats attacker, IUnitStats target, int baseDamage)
    {
        float outgoingMult = 1f;

        foreach (var buff in attacker.GetAllBuffs())
            outgoingMult += (buff.GetDamageMultiplier() - 1f);

        foreach (var debuff in attacker.GetAllDebuffs())
            outgoingMult += (debuff.GetDamageMultiplier() - 1f);

        int damageAfterOutgoing = Mathf.RoundToInt(baseDamage * outgoingMult);

        float additiveIncomingBonus = 0f;

        foreach (var debuff in target.GetAllDebuffs())
            additiveIncomingBonus += debuff.GetIncomingDamageMultiplier(false) - 1f;

        foreach (var buff in target.GetAllBuffs())
            additiveIncomingBonus += buff.GetIncomingDamageMultiplier() - 1f;

        int final = Mathf.RoundToInt(damageAfterOutgoing * (1f + additiveIncomingBonus));
        Debug.Log($"[DamageCalc] Final: {baseDamage} ¡æ {final} (Outgoing x{outgoingMult:F2}, Incoming +{additiveIncomingBonus * 100f:F1}%)");
        return final;
    }
}
