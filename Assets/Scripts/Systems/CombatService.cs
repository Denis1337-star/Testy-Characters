using System;

public class CombatService 
{
    readonly GameState _state;
    private bool _isDeathSent;

    public event Action Damaged;
    public event Action EnemyDied;
    public event Action HpChanged;
    public event Action GoldChanged;

    public CombatService(GameState state)
    {
        _state = state;
        RespawnEnemyHp();
    }

    public void Click()
    {
        if (_state.enemyhp <= 0) return;
        ApplyDamage(_state.clickDamage, isFromClick: true);
    }
    public void Tick(float dt)
    {
        if (_state.totalDPS <= 0d || _state.enemyhp <= 0) return;
        ApplyDamage(_state.totalDPS * dt, isFromClick: false);
    }
    private void ApplyDamage(double amount, bool isFromClick)
    {
        if (amount <= 0d || _state.enemyhp <= 0) return;

        _state.enemyhp -= amount;
        if (_state.enemyhp < 0) _state.enemyhp = 0;

        HpChanged?.Invoke();

        if (isFromClick)
            Damaged?.Invoke();

        if (_state.enemyhp <= 0 && !_isDeathSent)
        {
            _isDeathSent = true;
            EnemyDied?.Invoke();
        }  
    }
    public void RewardForKill()
    {
        double reward = GameFormulas.GoldForKill(_state.enemyMaxHp);
        reward *= GameFormulas.GoldMultiplierFromCrystals(_state.crystals);
        reward = Math.Floor(reward);
        if (reward < 1d) reward = 1d;

        _state.gold += reward;
        _state.goldEarnedThisRun += reward;
        GoldChanged?.Invoke();

    }
    public void RespawnEnemyHp()
    {
        _isDeathSent = false;

        bool isBoss = LevelService.IsBossLevel(_state.currentLevel);
        _state.enemyMaxHp = GameFormulas.EnemyMaxHP(_state.currentLevel, isBoss);
        _state.enemyhp = _state.enemyMaxHp;

        HpChanged?.Invoke();
    }
    public void NotifyGoldChanged()
    {
        GoldChanged?.Invoke();
    }
}
