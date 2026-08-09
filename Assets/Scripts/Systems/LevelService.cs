public class LevelService 
{
    readonly GameState _state;

    public LevelService(GameState state)
    {
        _state = state;
    }
    public void RegisterKill()
    {
        _state.killsOnLevel++;

        if (_state.killsOnLevel >= _state.killsToClear)
        {
            _state.currentLevel++;

            if (_state.currentLevel > _state.maxUnlockedLevel)
                _state.maxUnlockedLevel = _state.currentLevel;

            _state.killsOnLevel = 0;
        }
        _state.Notify();
    }
    public bool TrySelectLevel(int level)
    {
        if (level < 1 || level > _state.maxUnlockedLevel) return false;

        _state.currentLevel = level;
        _state.killsOnLevel = 0;
        _state.Notify();
        return true;
    }
}
