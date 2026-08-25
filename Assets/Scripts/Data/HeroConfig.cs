using UnityEngine;

[CreateAssetMenu(menuName ="Game/Hero Config")]
public class HeroConfig : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public bool IsClickHero;
    public double BaseCost;
    public double BasePower;
    public HeroSkillDefinition[] Skills;
}
