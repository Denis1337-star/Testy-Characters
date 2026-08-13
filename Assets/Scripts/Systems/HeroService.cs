using System;
using UnityEngine;

public class HeroService 
{
    readonly GameState _state;
    readonly HeroesConfig _heroesConfig;

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

        return def.BasePower * lvl;
    }

    public double GetUpgradeCost(int index)
    {
        var def = _heroesConfig.Heroes[index];
        int lvl = _state.heroLevels[index];
        return Math.Floor(def.BaseCost * Math.Pow(1.07d, lvl));
    }
    public bool TryUpgrade(int index)
    {
        if (index < 0 || index >= HeroCount) return false;
        if (!IsVisible(index)) return false;

        double cost = GetUpgradeCost(index);
        if (_state.gold < cost) return false;

        _state.gold -= cost;
        _state.heroLevels[index]++;
        RecalculatePower();
        _state.Notify();
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
            double power = def.BasePower * lvl;

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

}
