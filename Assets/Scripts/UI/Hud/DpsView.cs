using TMPro;
using UnityEngine;
using Zenject;

public class DpsView : MonoBehaviour
{
    [SerializeField] private TMP_Text _dpsText;
    private GameState _gameState;
    private HeroService _heroService;

    [Inject]
    private void Construct(GameState gameState, HeroService heroService)
    {
        _gameState = gameState;
        _heroService = heroService;
    }
    private void Start()
    {
        _heroService.Upgraded += Refresh;
        _heroService.ListChanged += Refresh;
        Refresh();
    }
    private void OnDestroy()
    {
        _heroService.Upgraded -= Refresh;
        _heroService.ListChanged -= Refresh;
    }
    private void Refresh()
    {
        _dpsText.text = $"{NumberFormatter.Format(_gameState.totalDPS)} УВС";
    }
}
