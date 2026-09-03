using System;
using UnityEngine;

public class LevelService
{
    readonly GameState _gameState;
    public event Action ZoneChanged;
    public event Action ProgressChanged;

    public LevelService(GameState state)
    {
        _gameState = state;
    }
    public void RegisterKill()
    {
        if (_gameState.killsOnLevel < _gameState.killsToClear)
            _gameState.killsOnLevel++;

        bool isOnFrontier = _gameState.currentLevel >= _gameState.maxUnlockedLevel;
        bool isCleared = _gameState.killsOnLevel >= _gameState.killsToClear;

        if (isCleared && isOnFrontier)
        {
            _gameState.isBossActive = false;
            _gameState.bossTimerLeft = 0f;

            _gameState.currentLevel++;
            if (_gameState.currentLevel > _gameState.maxUnlockedLevel)
                _gameState.maxUnlockedLevel = _gameState.currentLevel;

            _gameState.killsOnLevel = 0;
            ApplyLevelRules();
            RaiseZoneChanged();
            RaiseProgressChanged();
        }
        else
        {
            if (_gameState.killsOnLevel > _gameState.killsToClear)
                _gameState.killsOnLevel = _gameState.killsToClear;

            RaiseProgressChanged();
        }
    }
    public bool TrySelectLevel(int level)
    {
        if (level < 1 || level > _gameState.maxUnlockedLevel) return false;

        _gameState.currentLevel = level;
        ApplyLevelRules();

        if (level < _gameState.maxUnlockedLevel)
            _gameState.killsOnLevel = _gameState.killsToClear;
        else
            _gameState.killsOnLevel = 0;

        RaiseZoneChanged();
        RaiseProgressChanged();
        return true;
    }
    private void ApplyLevelRules()
    {
        _gameState.killsToClear = KillsRequiredFor(_gameState.currentLevel);

        bool isBoss = IsBossLevel(_gameState.currentLevel);
        bool isFrontier = _gameState.currentLevel >= _gameState.maxUnlockedLevel;

        if (isBoss && isFrontier)
        {
            _gameState.isBossActive = true;
            _gameState.bossTimerLeft = GameState.BossTimeLimit;
        }
        else
        {
            _gameState.isBossActive = false;
            _gameState.bossTimerLeft = 0f;
        }
    }
    public void FailBoss()
    {
        if (!IsBossLevel(_gameState.currentLevel)) return;

        int back = Mathf.Max(1, _gameState.currentLevel - 1);

        _gameState.currentLevel = back;
        ApplyLevelRules();
        _gameState.killsOnLevel = _gameState.killsToClear;
        _gameState.isBossActive = false;
        _gameState.bossTimerLeft = 0f;

        RaiseZoneChanged();
        RaiseProgressChanged();
    }
    public void TickBoss(float time)
    {
        if (!_gameState.isBossActive) return;

        _gameState.bossTimerLeft -= time;

        if (_gameState.bossTimerLeft <= 0f)
        {
            _gameState.bossTimerLeft = 0f;
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
        _gameState.currentLevel = 1;
        _gameState.maxUnlockedLevel = 1;
        _gameState.killsOnLevel = 0;

        ApplyLevelRules();
        RaiseZoneChanged();
        RaiseProgressChanged();
    }
    private void RaiseZoneChanged()
    {
        ZoneChanged?.Invoke();
    }
    private void RaiseProgressChanged()
    {
        ProgressChanged?.Invoke();
    }
    
}
