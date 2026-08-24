using System;

public class HeroService
{
    readonly GameState _state;
    readonly HeroesConfig _heroesConfig;
    public int UpgradeMultiplier { get; private set; } = 1;
    public event Action ListChanged;
    public event Action MultiplierChanged;
    public event Action Upgraded;


    public HeroService(GameState gameState, HeroesConfig heroesConfig)
    {
        _state = gameState;
        _heroesConfig = heroesConfig;

        InitLevels();
        RecalculatePower();
    }
    private void InitLevels()
    {
        int count;
        if (_heroesConfig.Heroes != null)
            count = _heroesConfig.Heroes.Length;
        else
            count = 0;

        _state.heroLevels = new int[count];
        if (count > 0)
            _state.heroLevels[0] = 1;

        _state.heroSkillsOwned = new bool[count][];
        for (int i = 0; i < count; i++)
        {
            int skillCount = 0;
            if (_heroesConfig.Heroes[i].Skills != null)
                skillCount = _heroesConfig.Heroes[i].Skills.Length;
            else
                skillCount = 0;

            _state.heroSkillsOwned[i] = new bool[skillCount];
        }
    }
    public int HeroCount
    {
        get
        {
            if (_state.heroLevels != null)
                return _state.heroLevels.Length;
            else
                return 0;
        }
    }
    public bool IsVisible(int index)
    {
        if (index < 0 || index >= HeroCount) return false;
        if (_state.heroLevels[index] > 0) return true;

        int highestOwned = -1;
        for (int i = 0; i < HeroCount; i++)
            if (_state.heroLevels[i] > 0) highestOwned = i;

        return index == highestOwned + 1;
    }
    public int GetLevel(int index)
    {
        return _state.heroLevels[index];
    }
    public double GetPower(int index)
    {
        var def = _heroesConfig.Heroes[index];
        int lvl = _state.heroLevels[index];

        if (lvl <= 0) return 0d;

        return def.BasePower * lvl * GetSkillMultiplier(index);
    }
    public double GetUpgradeCost(int index, int levels)
    {
        if (levels < 1) levels = 1;

        var def = _heroesConfig.Heroes[index];
        int current = _state.heroLevels[index];
        double total = 0d;

        for (int i = 0; i < levels; i++)
        {
            total += Math.Floor(def.BaseCost * Math.Pow(1.07d, current + i));
        }
        return total;
    }
    public bool TryUpgrade(int index)
    {
        if (index < 0 || index >= HeroCount) return false;
        if (!IsVisible(index)) return false;

        int levels = UpgradeMultiplier;
        double cost = GetUpgradeCost(index, levels);
        if (_state.gold < cost) return false;

        bool isWasLocked = _state.heroLevels[index] == 0;

        _state.gold -= cost;
        _state.heroLevels[index] += levels;

        RecalculatePower();
        Upgraded?.Invoke();

        if (isWasLocked)
            ListChanged?.Invoke();

        return true;

    }
    public void RecalculatePower()
    {
        double click = 0d;
        double dps = 0d;

        for (int i = 0; i < HeroCount; i++)
        {
            int lvl = _state.heroLevels[i];
            if (lvl <= 0) continue;

            var def = _heroesConfig.Heroes[i];
            double power = GetPower(i);

            if (def.IsClickHero)
                click += power;
            else
                dps += power;
        }

        if (click > 0d)
            _state.clickDamage = click;
        else
            _state.clickDamage = 1d;

        _state.totalDPS = dps;
    }
    public HeroDefinition GetDifinition(int index)
    {
        return _heroesConfig.Heroes[index];
    }
    public void SetUpgradeMultiplier(int mult)
    {
        if (mult != 1 && mult != 10 & mult != 25 && mult != 100)
            return;

        if (UpgradeMultiplier == mult)
            return;

        UpgradeMultiplier = mult;
        MultiplierChanged?.Invoke();
    }
    public bool CanAffordUpgarade(int index)
    {
        if (!IsVisible(index)) return false;
        return _state.gold >= GetUpgradeCost(index, UpgradeMultiplier);
    }
    public int GetSkillCount(int heroIndex)
    {
        var skills = _heroesConfig.Heroes[heroIndex].Skills;
        if (skills != null)
            return skills.Length;
        else
            return 0;
    }
    public HeroSkillDefinition GetSkill(int heroIndex, int skillIndex)
    {
        return _heroesConfig.Heroes[heroIndex].Skills[skillIndex];
    }
    public bool IsSkillOwned(int heroIndex, int skillIndex)
    {
        return _state.heroSkillsOwned[heroIndex][skillIndex];
    }
    public bool IsSkillUnlocked(int heroIndex, int skillIndex)
    {
        int level = GetLevel(heroIndex);
        if (level <= 0) return false;

        var skill = GetSkill(heroIndex, skillIndex);
        if (level >= skill.UnlockLevel)
            return true;
        else
            return false;
    }
    public bool CanAffordSkill(int heroIndex, int skillIndex)
    {
        if (!IsSkillUnlocked(heroIndex, skillIndex)) return false;
        if (IsSkillOwned(heroIndex, skillIndex)) return false;

        return _state.gold >= GetSkill(heroIndex, skillIndex).Cost;
    }
    public bool TryBuySkill(int heroIndex, int skillIndex)
    {
        if (!CanAffordSkill(heroIndex, skillIndex)) return false;

        _state.gold -= GetSkill(heroIndex, skillIndex).Cost;
        _state.heroSkillsOwned[heroIndex][skillIndex] = true;

        RecalculatePower();
        Upgraded?.Invoke();
        return true;
    }
    public double GetSkillMultiplier(int heroIndex)
    {
        double mult = 1d;
        int count = GetSkillCount(heroIndex);

        for (int s = 0; s < count; s++)
        {
            if (!IsSkillOwned(heroIndex, s)) continue;
            mult *= (1d + GetSkill(heroIndex, s).DamageBonus);
        }
        return mult;
    }

}
