
using System;

public static class GameFormulas 
{
    public const double EnemyHpBase = 10d;
    public const double EnemyHpGrowth = 1.6d;
    public const double BossHpMultiplier = 10d;
    public const double GoldDivisor = 15d;

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
}
