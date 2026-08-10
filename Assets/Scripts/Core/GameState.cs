using System;
public class GameState 
{
    public int enemyMaxHp = 10;
    public int enemyhp;
    public int clickDamage = 1;

    public int currentLevel = 1;
    public int maxUnlockedLevel = 1;
    public int killsOnLevel;
    public int killsToClear = 10;

    public int currentEnemyIndex;

    public event Action Changed;

    public void Notify() => Changed?.Invoke();
}
