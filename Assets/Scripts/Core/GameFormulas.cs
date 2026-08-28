
using System;

public static class GameFormulas 
{
    public const double EnemyHpBase = 10d;
    public const double EnemyHpGrowth = 1.6d;
    public const double BossHpMultiplier = 10d;
    public const double GoldDivisor = 15d;
    public const double CrystalGoldDivisor = 18_000d;
    public const double CrystalExponent = 0.38685280723d;
    public const double CrystalXpScale = 3_000d;
    public const double CrystalEffectBase = 0.01d;
    public const double CrystalStandInTrees = 0.15d;

    public static double EnemyMaxHP(int level, bool isBoss)
    {
        double hp = EnemyHpBase * Math.Pow(EnemyHpGrowth, level - 1);

        if (isBoss)
            hp *= BossHpMultiplier;

        return Math.Max(1d, Math.Floor(hp));
    }
    public static double GoldForKill(double enemyMaxHp)
    {
        double gold = Math.Floor(enemyMaxHp / GoldDivisor);
        return Math.Max(1d, gold);
    }
    public static double PendingCrystals(double goldEarnedThisRun)
    {
        if (goldEarnedThisRun < 1d) return 0d;
        return Math.Floor(Math.Pow(goldEarnedThisRun / CrystalGoldDivisor, CrystalExponent));
    }
    public static double TotalXpFromCrystals(double crystals)
    {
        if (crystals < 1d) return 0d;
        double log10 = Math.Log10(crystals);
        int exponent = (int)Math.Floor(log10);
        double mantissa = crystals / Math.Pow(10d, exponent);
        return (mantissa / 10d + exponent) * CrystalXpScale;
    }
    public static int XpToNextLevel(int level)
    {
        if (level <= 6) return 1000 + (level - 1) * 25;
        if (level <= 10) return 1125 + (level - 6) * 50;
        if (level <= 21) return 1325 + (level - 10) * 15;
        if (level <= 41) return 1490 + (level - 21) * 30;
        return 2110 + (level - 41) * 20;
    }
    public static void GetCharacterProgress(double crystals, out int level,
        out double xpIntoLevel, out int xpToNextLevel)
    {
        level = 1; ;
        double xp = TotalXpFromCrystals(crystals);
        while (true)
        {
            xpToNextLevel = XpToNextLevel(level);
            if(xp < xpToNextLevel) break;
            xp -= xpToNextLevel;
            level++;
        }
        xpIntoLevel = xp;
    }
    public static double GoldMultiplierFromCrystals(double crystals)
    {
        if (crystals < 1d) return 1d;
        return 1d + CrystalEffectBase + CrystalStandInTrees * Math.Log10(1d + crystals);
    }
}
