using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BackgroundView : MonoBehaviour
{
    [SerializeField] private Image _background;

    private GameState _state;
    private LocationService _locationService;
    private LevelService _levelService;
    private int _lastLocationIndex = -1;

    [Inject]
    private void Construct(GameState gameState, LocationService locationService, LevelService levelService)
    {
        _state = gameState;
        _locationService = locationService;
        _levelService = levelService;
    }
    private void Start()
    {
        _levelService.ZoneChanged += Refresh;
        Refresh();
    }
    private void OnDestroy()
    {
       _levelService.ZoneChanged -= Refresh;
    }
    private void Refresh()
    {
        int index = _locationService.GetLocationIndex();
        if (index == _lastLocationIndex) return;

        _lastLocationIndex = index;
        var sprite = _locationService.GetBackground();
        if (sprite != null)
            _background.sprite = sprite;
    }
}
