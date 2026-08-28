using System;

public class RebirthService 
{
    readonly GameState _state;
    readonly HeroService _heroService;
    readonly LevelService _levelService;
    readonly CombatService _combatService;
    public event Action Rebirthed;

    public RebirthService(GameState gameState, HeroService heroService,
        LevelService levelService, CombatService combatService)
    {
        _state = gameState;
        _heroService = heroService;
        _levelService = levelService;
        _combatService = combatService;
    }
    public double GetPending()
    {
        return GameFormulas.PendingCrystals(_state.goldEarnedThisRun);
    }
    public bool CanRebirth()
    {
        return GetPending() >= 1d;
    }
    public void DoRebirth()
    {
        if (!CanRebirth()) return;

        double pending = GetPending();
        _state.crystals += pending;

        _state.gold = 0d;
        _state.goldEarnedThisRun = 0d;

        _heroService.ResetForRebirth();
        _levelService.ResetToFirstZone();
        _combatService.RespawnEnemyHp();
        _combatService.NotifyGoldChanged();

        Rebirthed?.Invoke();
    }
}
