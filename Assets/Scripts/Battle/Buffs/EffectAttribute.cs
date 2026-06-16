using System;

[AttributeUsage(AttributeTargets.Class)]
public class EffectAttribute : Attribute
{
    public string Id { get; }
    public string Name { get; }

    public EffectAttribute(string id, string name)
    {
        Id = id;
        Name = name;
    }
}
