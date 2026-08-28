using System;
using UnityEngine;

public class LevelService
{
    readonly GameState _state;
    public event Action ZoneChanged;
    public event Action ProgressChanged;

    public LevelService(GameState state)
    {
        _state = state;
    }
    public void RegisterKill()
    {
        if (_state.killsOnLevel < _state.killsToClear)
            _state.killsOnLevel++;

        bool isOnFrontier = _state.currentLevel >= _state.maxUnlockedLevel;
        bool isCleared = _state.killsOnLevel >= _state.killsToClear;

        if (isCleared && isOnFrontier)
        {
            _state.isBossActive = false;
            _state.bossTimerLeft = 0f;

            _state.currentLevel++;
            if (_state.currentLevel > _state.maxUnlockedLevel)
                _state.maxUnlockedLevel = _state.currentLevel;

            _state.killsOnLevel = 0;
            ApplayLevelRules();
            RaizeZoneChanged();
            RaiseProgressChanged();
        }
        else
        {
            if (_state.killsOnLevel > _state.killsToClear)
                _state.killsOnLevel = _state.killsToClear;

            RaiseProgressChanged();
        }
    }
    public bool TrySelectLevel(int level)
    {
        if (level < 1 || level > _state.maxUnlockedLevel) return false;

        _state.currentLevel = level;
        ApplayLevelRules();

        if (level < _state.maxUnlockedLevel)
            _state.killsOnLevel = _state.killsToClear;
        else
            _state.killsOnLevel = 0;

        RaizeZoneChanged();
        RaiseProgressChanged();
        return true;
    }
    private void ApplayLevelRules()
    {
        _state.killsToClear = KillsRequiredFor(_state.currentLevel);

        bool isBoss = IsBossLevel(_state.currentLevel);
        bool isFrontier = _state.currentLevel >= _state.maxUnlockedLevel;

        if (isBoss && isFrontier)
        {
            _state.isBossActive = true;
            _state.bossTimerLeft = GameState.BossTimeLimit;
        }
        else
        {
            _state.isBossActive = false;
            _state.bossTimerLeft = 0f;
        }
    }
    public void FailBoss()
    {
        if (!IsBossLevel(_state.currentLevel)) return;

        int back = Mathf.Max(1, _state.currentLevel - 1);

        _state.currentLevel = back;
        ApplayLevelRules();
        _state.killsOnLevel = _state.killsToClear;
        _state.isBossActive = false;
        _state.bossTimerLeft = 0f;

        RaizeZoneChanged();
        RaiseProgressChanged();
    }
    public void TickBoss(float time)
    {
        if (!_state.isBossActive) return;

        _state.bossTimerLeft -= time;

        if (_state.bossTimerLeft <= 0f)
        {
            _state.bossTimerLeft = 0f;
            FailBoss();
        }
    }
    public static bool IsBossLevel(int level)
    {
        return level > 0 && level % 5 == 0;
    }
    public static int KillsRequiredFor(int level)
    {
        if (IsBossLevel(level))
            return 1;
        else
            return 10;
    }
    public void ResetToFirstZone()
    {
        _state.currentLevel = 1;
        _state.maxUnlockedLevel = 1;
        _state.killsOnLevel = 0;

        ApplayLevelRules();
        RaizeZoneChanged();
        RaiseProgressChanged();
    }
    private void RaizeZoneChanged()
    {
        ZoneChanged?.Invoke();
    }
    private void RaiseProgressChanged()
    {
        ProgressChanged?.Invoke();
    }
    
}
