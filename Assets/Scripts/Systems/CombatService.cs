using System;

public class CombatService 
{
    readonly GameState _state;
    private bool _isDeathSent;

    public event Action Damaged;
    public event Action EnemyDied;

    public CombatService(GameState state)
    {
        _state = state;
        _state.enemyhp = _state.enemyMaxHp;
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

        _state.Notify();

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
        _state.gold += Math.Floor(10d  *Math.Pow(1.15d, _state.currentLevel - 1));
        _state.Notify();

    }
    public void RespawnEnemyHp()
    {
        _isDeathSent = false;
        _state.enemyhp = _state.enemyMaxHp;
        _state.Notify();
    }
}
