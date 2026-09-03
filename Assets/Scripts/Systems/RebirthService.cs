using System;

public class RebirthService 
{
    readonly GameState _gameState;
    readonly HeroService _heroService;
    readonly LevelService _levelService;
    readonly CombatService _combatService;
    public event Action Rebirthed;

    public RebirthService(GameState gameState, HeroService heroService,
        LevelService levelService, CombatService combatService)
    {
        _gameState = gameState;
        _heroService = heroService;
        _levelService = levelService;
        _combatService = combatService;
    }
    public double GetPendingCrystals()
    {
        return GameFormulas.PendingCrystals(_gameState.goldEarnedThisRun);
    }
    public bool CanRebirth()
    {
        return GetPendingCrystals() >= 1d;
    }
    public void DoRebirth()
    {
        if (!CanRebirth()) return;

        double pending = GetPendingCrystals();
        _gameState.crystals += pending;

        _gameState.gold = 0d;
        _gameState.goldEarnedThisRun = 0d;

        _heroService.ResetForRebirth();
        _levelService.ResetToFirstZone();
        _combatService.RespawnEnemyHp();
        _combatService.NotifyGoldChanged();

        Rebirthed?.Invoke();
    }
    public RebirthPreview GetPreview()
    {
        double pendingWhole = GetPendingCrystals();
        double pendingExact = GameFormulas.PendingCrystalsRaw(_gameState.goldEarnedThisRun);
        double crystalsOwned = _gameState.crystals;
        double crystalsAfterPrestige = crystalsOwned + pendingWhole;
        double crystalsForExperience = GameFormulas.CrystalsForXp(crystalsOwned, pendingExact);

        GameFormulas.GetCharacterProgress(
            crystalsForExperience,
            out int profileLevel,
            out double experienceIntoLevel,
            out int experienceToNextLevel);

        return new RebirthPreview
        {
            Pending = pendingWhole,
            CrystalsAfter = crystalsAfterPrestige,
            CrystalsForExperience = crystalsForExperience,
            GoldBonusDeltaPercent = GameFormulas.GoldBonusDeltaPercent(crystalsOwned, crystalsAfterPrestige),
            ProfileLevel = profileLevel,
            XpIntoLevel = experienceIntoLevel,
            XpToNextLevel = experienceToNextLevel,
            CanRebirth = CanRebirth()
        };
    }
}
