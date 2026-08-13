using UnityEngine;
using Zenject;

public class DpsTicker : MonoBehaviour
{
    CombatService _combatService;

    [Inject]
    private void Construct(CombatService combatService)
    {
        _combatService = combatService;
    }
    private void Update()
    {
        _combatService.Tick(Time.deltaTime);
    }
}
