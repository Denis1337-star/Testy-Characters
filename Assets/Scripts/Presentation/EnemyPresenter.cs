using UnityEngine;
using Zenject;

public class EnemyPresenter : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] float _deathDelay = 0.3f;

    CombatService _combatService;
    LevelService _levelService;
    LocationService _locationService;
    GameState _state;

    [Inject]
    private void Contstruct(CombatService combatService, LevelService levelService,
        LocationService locationService, GameState gameState)
    {
        _combatService = combatService;
        _levelService = levelService;
        _locationService = locationService;
        _state = gameState;
    }

    private void Start()
    {
        _combatService.Damaged += PlayHurt;
        _combatService.EnemyDied += OnEnemyDied;

        ApplyFromState();
    }
    private void OnDestroy()
    {
        if (_combatService == null) return;
        _combatService.EnemyDied -= OnEnemyDied;
        _combatService.Damaged -= PlayHurt;
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
        _levelService.RegisterKill();

        var pool = _locationService.GetEnemyPool();
        if (pool.Length == 0) return;

        _state.currentEnemyIndex = PickNextIndex(_state.currentEnemyIndex, pool.Length);

        ApplyController(pool[_state.currentEnemyIndex]);
        _combatService.RespawnEnemyHp();
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

        if (_state.currentEnemyIndex < 0 || _state.currentEnemyIndex >= pool.Length)
            _state.currentEnemyIndex = 0;

        ApplyController(pool[_state.currentEnemyIndex]);
    }
}
