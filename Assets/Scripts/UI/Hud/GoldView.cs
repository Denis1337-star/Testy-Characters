using TMPro;
using UnityEngine;
using Zenject;

public class GoldView : MonoBehaviour
{
    [SerializeField] TMP_Text _goldText;
    private GameState _gameState;
    private CombatService _combatService;
    private HeroService _heroService;

    [Inject]
    private void Construct(GameState gameState, CombatService combatService, HeroService heroService)
    {
        _gameState = gameState;
        _combatService = combatService;
        _heroService = heroService;
    }

    private void Start()
    {
        _combatService.GoldChanged+= Refresh;
        _heroService.Upgraded += Refresh;
        Refresh();
    }
    private void OnDestroy()
    {
        _combatService.GoldChanged -= Refresh;
        _heroService.Upgraded -= Refresh;
    }
    private void Refresh()
    {
        _goldText.text = NumberFormatter.Format(_gameState.gold);
    }

}
