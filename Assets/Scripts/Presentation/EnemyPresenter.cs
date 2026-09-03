using UnityEngine;
using Zenject;

public class EnemyPresenter : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] float _deathDelay = 0.3f;

    CombatService _combatService;
    LevelService _levelService;
    LocationService _locationService;
    GameState _gameState;

    [Inject]
    private void Contstruct(CombatService combatService, LevelService levelService,
        LocationService locationService, GameState gameState)
    {
        _combatService = combatService;
        _levelService = levelService;
        _locationService = locationService;
        _gameState = gameState;
    }

    private void Start()
    {
        _combatService.Damaged += PlayHurt;
        _combatService.EnemyDied += OnEnemyDied;
        _levelService.ZoneChanged += OnZoneChanged;

        ApplyFromState();
        _combatService.RespawnEnemyHp();
    }
    private void OnDestroy()
    {
        _combatService.EnemyDied -= OnEnemyDied;
        _combatService.Damaged -= PlayHurt;
        _levelService.ZoneChanged -= OnZoneChanged;
    }
    public void PlayHurt()
    {
        _animator.SetTrigger("Hurt");
    }
    private void OnEnemyDied()
    {
        _animator.SetTrigger("Death");
        CancelInvoke(nameof(AfterDeath));
        Invoke(nameof(AfterDeath), _deathDelay);

    }
    private void AfterDeath()
    {
        int levelBefore = _gameState.currentLevel;

        _combatService.RewardForKill();
        _levelService.RegisterKill();

        if (_gameState.currentLevel == levelBefore)
            OnZoneChanged();
    }
    private int PickNextIndex(int current, int length)
    {
        if (length <= 1) return 0;

        int next;
        do
        {
            next = Random.Range(0, length);
        }
        while (next == current);

        return next;
    }
    private void ApplyController(RuntimeAnimatorController controller)
    {
        if ( controller== null) return;

        _animator.runtimeAnimatorController = controller;
        _animator.Rebind();
        _animator.Update(0f);
    }
    private void ApplyFromState()
    {
        var pool = _locationService.GetEnemyPool();
        if (pool.Length == 0) return;

        if (_gameState.currentEnemyIndex < 0 || _gameState.currentEnemyIndex >= pool.Length)
            _gameState.currentEnemyIndex = 0;

        ApplyController(pool[_gameState.currentEnemyIndex]);
    }
    private void OnZoneChanged()
    {
        var pool = _locationService.GetEnemyPool();
        if (pool.Length == 0) return;

        _gameState.currentEnemyIndex = PickNextIndex(_gameState.currentEnemyIndex, pool.Length);
        ApplyController(pool[_gameState.currentEnemyIndex]);
        _combatService.RespawnEnemyHp();
    }
}
