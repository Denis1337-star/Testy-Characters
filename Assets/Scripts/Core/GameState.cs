using System;
public class GameState 
{
    public double gold;

    public double enemyMaxHp = 10;
    public double enemyhp;
    public int currentEnemyIndex;

    public int killsOnLevel;
    public int killsToClear = 10;
    public int currentLevel = 1;
    public int maxUnlockedLevel = 1;
    public float bossTimerLeft;
    public bool isBossActive;
    public const float BossTimeLimit = 30f;

    public double clickDamage = 1;
    public double totalDPS;
    public int[] heroLevels;

    public event Action Changed;

    public void Notify() 
    {
        Changed?.Invoke();
    } 
}
