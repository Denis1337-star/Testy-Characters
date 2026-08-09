using UnityEngine;
using Zenject;

public class EnemyPresenter : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private RuntimeAnimatorController[] _enemyControllers;
    [SerializeField] float _deathDelay = 0.3f;

    CombatService _combatService;
    LevelService _levelService;
    GameState _state;

    [Inject]
    private void Contstruct(CombatService combatService, LevelService levelService, GameState gameState)
    {
        _combatService = combatService;
        _levelService = levelService;
        _state = gameState;
    }

    private void Start()
    {
        _combatService.Damaged += PlayHurt;
        _combatService.EnemyDied += OnEnemyDied;
        ApplyController(_state.currentEnemyIndex);
    }
    private void OnEnemyDied()
    {
        _animator.SetTrigger("Death");
        CancelInvoke(nameof(AfterDeath));
        Invoke(nameof(AfterDeath), _deathDelay);

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
    private void AfterDeath()
    {
        _levelService.RegisterKill();

        int next = _state.currentEnemyIndex + 1;
        if (next >= _enemyControllers.Length) next = 0;
        _state.currentEnemyIndex = next;

        ApplyController(next);
        _combatService.RespawnEnemyHp();


    }
    private void ApplyController(int index)
    {
        if (_enemyControllers == null || index < 0 || index >= _enemyControllers.Length) return;
        _animator.runtimeAnimatorController = _enemyControllers[index];
        _animator.Rebind();
        _animator.Update(0f);
    }

}
