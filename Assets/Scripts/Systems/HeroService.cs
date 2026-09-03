using System;

public class HeroService
{
    readonly GameState _gameState;
    readonly HeroesConfig _heroesConfig;
    public int UpgradeMultiplier { get; private set; } = 1;
    public event Action ListChanged;
    public event Action MultiplierChanged;
    public event Action Upgraded;


    public HeroService(GameState gameState, HeroesConfig heroesConfig)
    {
        _gameState = gameState;
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

        _gameState.heroLevels = new int[count];
        if (count > 0)
            _gameState.heroLevels[0] = 1;

        _gameState.heroSkillsOwned = new bool[count][];
        for (int i = 0; i < count; i++)
        {
            int skillCount = 0;
            if (_heroesConfig.Heroes[i].Skills != null)
                skillCount = _heroesConfig.Heroes[i].Skills.Length;
            else
                skillCount = 0;

            _gameState.heroSkillsOwned[i] = new bool[skillCount];
        }
    }
    public int HeroCount
    {
        get
        {
            if (_gameState.heroLevels != null)
                return _gameState.heroLevels.Length;
            else
                return 0;
        }
    }
    public bool IsVisible(int index)
    {
        if (index < 0 || index >= HeroCount) return false;
        if (_gameState.heroLevels[index] > 0) return true;

        int highestOwned = -1;
        for (int i = 0; i < HeroCount; i++)
            if (_gameState.heroLevels[i] > 0) highestOwned = i;

        return index == highestOwned + 1;
    }
    public int GetLevel(int index)
    {
        return _gameState.heroLevels[index];
    }
    public double GetPower(int index)
    {
        var definition = _heroesConfig.Heroes[index];
        int level = _gameState.heroLevels[index];

        if (level <= 0) return 0d;

        return definition.BasePower * level * GetSkillMultiplier(index);
    }
    public double GetUpgradeCost(int index, int levels)
    {
        if (levels < 1) levels = 1;

        var definition = _heroesConfig.Heroes[index];
        int current = _gameState.heroLevels[index];
        double total = 0d;

        for (int i = 0; i < levels; i++)
        {
            total += Math.Floor(definition.BaseCost * Math.Pow(1.07d, current + i));
        }
        return total;
    }
    public bool TryUpgrade(int index)
    {
        if (index < 0 || index >= HeroCount) return false;
        if (!IsVisible(index)) return false;

        int levels = UpgradeMultiplier;
        double cost = GetUpgradeCost(index, levels);
        if (_gameState.gold < cost) return false;

        bool wasLocked = _gameState.heroLevels[index] == 0;

        _gameState.gold -= cost;
        _gameState.heroLevels[index] += levels;

        RecalculatePower();
        Upgraded?.Invoke();

        if (wasLocked)
            ListChanged?.Invoke();

        return true;

    }
    public void RecalculatePower()
    {
        double click = 0d;
        double dps = 0d;

        for (int i = 0; i < HeroCount; i++)
        {
            int level = _gameState.heroLevels[i];
            if (level <= 0) continue;

            var definition = _heroesConfig.Heroes[i];
            double power = GetPower(i);

            if (definition.IsClickHero)
                click += power;
            else
                dps += power;
        }

        if (click > 0d)
            _gameState.clickDamage = click;
        else
            _gameState.clickDamage = 1d;

        _gameState.totalDPS = dps;
    }
    public HeroConfig GetDefinition(int index)
    {
        return _heroesConfig.Heroes[index];
    }
    public void SetUpgradeMultiplier(int levelsToBuy)
    {
        if (levelsToBuy != 1 && levelsToBuy != 10 && levelsToBuy != 25 && levelsToBuy != 100)
            return;

        if (UpgradeMultiplier == levelsToBuy)
            return;

        UpgradeMultiplier = levelsToBuy;
        MultiplierChanged?.Invoke();
    }
    public bool CanAffordUpgrade(int index)
    {
        if (!IsVisible(index)) return false;
        return _gameState.gold >= GetUpgradeCost(index, UpgradeMultiplier);
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
        return _gameState.heroSkillsOwned[heroIndex][skillIndex];
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

        return _gameState.gold >= GetSkill(heroIndex, skillIndex).Cost;
    }
    public bool TryBuySkill(int heroIndex, int skillIndex)
    {
        if (!CanAffordSkill(heroIndex, skillIndex)) return false;

        _gameState.gold -= GetSkill(heroIndex, skillIndex).Cost;
        _gameState.heroSkillsOwned[heroIndex][skillIndex] = true;

        RecalculatePower();
        Upgraded?.Invoke();
        return true;
    }
    public double GetSkillMultiplier(int heroIndex)
    {
        double levelsToBuy = 1d;
        int count = GetSkillCount(heroIndex);

        for (int s = 0; s < count; s++)
        {
            if (!IsSkillOwned(heroIndex, s)) continue;
            levelsToBuy *= (1d + GetSkill(heroIndex, s).DamageBonus);
        }
        return levelsToBuy;
    }
    public bool IsSkillVisibleOnCard(int heroIndex, int skillIndex)
    {
        int level = GetLevel(heroIndex);
        if (level <= 0) return false;

        if (skillIndex == 0) return true;

        return IsSkillUnlocked(heroIndex, skillIndex);
    }
    public void ResetForRebirth()
    {
        int count = HeroCount;
        for (int i = 0; i < count; i++)
            _gameState.heroLevels[i] = 0;

        if (count > 0)
            _gameState.heroLevels[0] = 1;

        for (int i = 0; i < count; i++)
        {
            var owned = _gameState.heroSkillsOwned[i];
            if (owned == null) continue;

            for (int s = 0; s < owned.Length; s++)
                owned[s] = false;
        }
        RecalculatePower();
        ListChanged?.Invoke();
        Upgraded?.Invoke();
    }

}
