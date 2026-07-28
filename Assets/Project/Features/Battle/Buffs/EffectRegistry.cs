using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class EffectRegistry
{
    private static Dictionary<string, Func<int, IUnitStats, IBuff>> buffFactories = new();
    private static Dictionary<string, Func<int, IUnitStats, IDebuff>> debuffFactories = new();
    private static Dictionary<string, EffectInfo> effectInfos = new();

    static EffectRegistry()
    {
        RegisterAllEffects();
    }

    private static void RegisterAllEffects()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();

        foreach (var type in types)
        {
            var effectAttr = type.GetCustomAttribute<EffectAttribute>();
            if (effectAttr == null) continue;

            string id = effectAttr.Id;
            string name = effectAttr.Name;

            string description = type.GetCustomAttribute<EffectDescriptionAttribute>()?.Description ?? "(설명 없음)";

            // ? 아이콘: 정적 필드 또는 인스턴스 프로퍼티에서 추출
            Sprite icon = null;

            // 1) 정적 필드 시도
            FieldInfo iconField = type.GetField("Icon", BindingFlags.Static | BindingFlags.Public);
            if (iconField != null && iconField.GetValue(null) is Sprite staticIcon)
            {
                icon = staticIcon;
            }
            else
            {
                // 2) 인스턴스 프로퍼티 시도 (Icon 프로퍼티 존재할 경우)
                try
                {
                    var dummy = Activator.CreateInstance(type);
                    var iconProp = type.GetProperty("Icon", BindingFlags.Instance | BindingFlags.Public);
                    if (iconProp != null && iconProp.GetValue(dummy) is Sprite dynamicIcon)
                        icon = dynamicIcon;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[EffectRegistry] '{type.Name}' 아이콘 로드 실패: {e.Message}");
                }
            }

            effectInfos[id] = new EffectInfo
            {
                Id = id,
                Name = name,
                Description = description.Replace("/n", "\n"),
                Icon = icon
            };

            if (typeof(IBuff).IsAssignableFrom(type))
            {
                buffFactories[id] = (stack, owner) =>
                {
                    var instance = Activator.CreateInstance(type, new object[] { stack, owner }) as IBuff;
                    instance?.Apply(owner);
                    return instance;
                };
            }

            if (typeof(IDebuff).IsAssignableFrom(type))
            {
                debuffFactories[id] = (stack, owner) =>
                {
                    var instance = Activator.CreateInstance(type, new object[] { stack, owner }) as IDebuff;
                    instance?.Apply(owner);
                    return instance;
                };
            }

        }

        Debug.Log($"[EffectRegistry] 등록 완료: {effectInfos.Count}개 효과");
    }


    public static EffectInfo Get(string id)
    {
        return effectInfos.TryGetValue(id, out var info) ? info : null;
    }

    public static IBuff CreateBuff(string id, int stack, IUnitStats owner = null)
    {
        if (buffFactories.TryGetValue(id, out var factory))
            return factory(stack, owner);

        Debug.LogWarning($"[EffectRegistry] Buff ID '{id}' 등록되지 않음");
        return null;
    }

    public static IDebuff CreateDebuff(string id, int stack, IUnitStats owner = null)
    {
        if (debuffFactories.TryGetValue(id, out var factory))
            return factory(stack, owner);

        Debug.LogWarning($"[EffectRegistry] Debuff ID '{id}' 등록되지 않음");
        return null;
    }
}
