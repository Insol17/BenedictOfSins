using System;

[AttributeUsage(AttributeTargets.Class)]
public class EffectDescriptionAttribute : Attribute
{
    public string Description { get; }

    public EffectDescriptionAttribute(string description)
    {
        Description = description;
    }
}
