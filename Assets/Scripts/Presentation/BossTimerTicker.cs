using UnityEngine;
using Zenject;

public class BossTimerTicker : MonoBehaviour
{
    LevelService  _levelService;

    [Inject]
    private void Construct(LevelService levelService)
        {
        _levelService = levelService;
    }
   private void Update()
    {
        _levelService.TickBoss(Time.deltaTime);
    }
}
