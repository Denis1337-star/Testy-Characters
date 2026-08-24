using System;
using UnityEngine;

[Serializable]
public class HeroDefinition
{
    public string Name;
    public Sprite Icon;
    public bool IsClickHero;
    public double BaseCost;
    public double BasePower;
    public HeroSkillDefinition[] Skills;
}
