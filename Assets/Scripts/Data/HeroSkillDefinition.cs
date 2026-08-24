using System;
using UnityEngine;

[Serializable]
public class HeroSkillDefinition 
{
    public string Name;
    public string Description;
    public Sprite Icon;
    public int UnlockLevel;
    public double Cost;
    public float DamageBonus;
}
