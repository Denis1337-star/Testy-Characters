using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UpgradeMultiplierView : MonoBehaviour
{
    [SerializeField] Button _x1;
    [SerializeField] Button _x10;
    [SerializeField] Button _x25;
    [SerializeField] Button _x100;

    [SerializeField] Color _activeColor;
    [SerializeField] Color _idleColor;

    HeroService _heroService;

    [Inject]
    private void Construct(HeroService heroService) 
    {
        _heroService = heroService;
    }
    private void Start()
    {
        _x1.onClick.AddListener (() => _heroService.SetUpgradeMultiplier(1));
        _x10.onClick.AddListener (()=> _heroService.SetUpgradeMultiplier(10));
        _x25.onClick.AddListener(() => _heroService.SetUpgradeMultiplier(25));
        _x100.onClick.AddListener(() => _heroService.SetUpgradeMultiplier(100));

        _heroService.MultiplierChanged += RefreshVisual;
        RefreshVisual();
    }
    private void OnDestroy()
    {
        _heroService.MultiplierChanged -= RefreshVisual;
    }
    private void RefreshVisual()
    {
        int mult = _heroService.UpgradeMultiplier;

        if (mult == 1) 
            _x1.image.color = _activeColor;
        else 
            _x1.image.color = _idleColor;

        if (mult == 10)
            _x10.image.color = _activeColor;
        else
            _x10.image.color = _idleColor;

        if (mult == 25)
            _x25.image.color = _activeColor;
        else
            _x25.image.color = _idleColor;

        if (mult == 100)
            _x100.image.color = _activeColor;
        else
            _x100.image.color = _idleColor;
    }
}
