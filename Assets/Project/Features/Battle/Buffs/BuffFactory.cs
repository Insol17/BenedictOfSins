using UnityEngine;

public static class BuffFactory
{
    public static IBuff CreateBuff(string effectName, int stack, IUnitStats owner = null) =>
        EffectRegistry.CreateBuff(effectName, stack, owner);

    public static IDebuff CreateDebuff(string effectName, int stack, IUnitStats owner = null) =>
        EffectRegistry.CreateDebuff(effectName, stack, owner);
}
