using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BackgroundView : MonoBehaviour
{
    [SerializeField] private Image _background;

    GameState _state;
    LocationService _locationService;
    private int _lastLocationIndex = 1;

    [Inject]
    private void Construct(GameState gameState, LocationService locationService)
    {
        _state = gameState;
        _locationService = locationService;
    }
    private void Start()
    {
        _state.Changed += Refresh;
        Refresh();
    }
    private void OnDestroy()
    {
        _state.Changed -= Refresh;
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
