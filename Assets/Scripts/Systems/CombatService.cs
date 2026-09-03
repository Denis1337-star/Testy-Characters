using System;

public class CombatService 
{
    readonly GameState _gameState;
    private bool _isDeathSent;
    public event Action Damaged;
    public event Action EnemyDied;
    public event Action HpChanged;
    public event Action GoldChanged;

    public CombatService(GameState state)
    {
        _gameState = state;
        RespawnEnemyHp();
    }

    public void Click()
    {
        if (_gameState.enemyhp <= 0) return;
        ApplyDamage(_gameState.clickDamage, isFromClick: true);
    }
    public void Tick(float dt)
    {
        if (_gameState.totalDPS <= 0d || _gameState.enemyhp <= 0) return;
        ApplyDamage(_gameState.totalDPS * dt, isFromClick: false);
    }
    private void ApplyDamage(double amount, bool isFromClick)
    {
        if (amount <= 0d || _gameState.enemyhp <= 0) return;

        _gameState.enemyhp -= amount;
        if (_gameState.enemyhp < 0) _gameState.enemyhp = 0;

        HpChanged?.Invoke();

        if (isFromClick)
            Damaged?.Invoke();

        if (_gameState.enemyhp <= 0 && !_isDeathSent)
        {
            _isDeathSent = true;
            EnemyDied?.Invoke();
        }  
    }
    public void RewardForKill()
    {
        double reward = GameFormulas.GoldForKill(_gameState.enemyMaxHp);
        reward *= GameFormulas.GoldMultiplierFromCrystals(_gameState.crystals);
        reward = Math.Floor(reward);
        if (reward < 1d) reward = 1d;

        _gameState.gold += reward;
        _gameState.goldEarnedThisRun += reward;
        GoldChanged?.Invoke();
    }
    public void RespawnEnemyHp()
    {
        _isDeathSent = false;

        bool isBoss = LevelService.IsBossLevel(_gameState.currentLevel);
        _gameState.enemyMaxHp = GameFormulas.EnemyMaxHP(_gameState.currentLevel, isBoss);
        _gameState.enemyhp = _gameState.enemyMaxHp;

        HpChanged?.Invoke();
    }
    public void NotifyGoldChanged()
    {
        GoldChanged?.Invoke();
    }
}
