using System;

public class CombatService 
{
    readonly GameState _state;

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

        _state.enemyhp -= _state.clickDamage;
        if (_state.enemyhp < 0) _state.enemyhp = 0;

        _state.Notify();
        Damaged?.Invoke();

        if (_state.enemyhp <= 0)
            EnemyDied?.Invoke();
    }
    public void RespawnEnemyHp()
    {
        _state.enemyhp = _state.enemyMaxHp;
        _state.Notify();
    }
}
