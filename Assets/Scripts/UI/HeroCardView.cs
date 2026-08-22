using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroCardView : MonoBehaviour
{
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _levelText;
    [SerializeField] TMP_Text _powerText;
    [SerializeField] Button _upgradeButton;
    [SerializeField] TMP_Text _upgradeLabel;
    [SerializeField] TMP_Text _costText;

    private int _index;
    HeroService _heroService;
    GameState _state;
    private bool _isBound;

    public void Setup(int index, HeroService heroService, GameState gameState)
    {
        _index = index;
        _heroService = heroService;
        _state = gameState;

        if (!_isBound)
        {
            _upgradeButton.onClick.AddListener(OnUpgradeClicked);
            _state.Changed += Refresh;
            _isBound = true;
        }

        var def = _heroService.GetDifinition(index);
        _icon.sprite = def.Icon;
        _nameText.text = def.Name;
        Refresh();
    }
    private void OnDestroy()
    {
        _state.Changed -= Refresh;
    }
    private void OnUpgradeClicked()
    {
        _heroService.TryUpgrade(_index);
    }
    private void Refresh()
    {
        int lvl = _heroService.GetLevel(_index);
        var def = _heroService.GetDifinition(_index);
        int mult = _heroService.UpgradeMultiplier;

        if (lvl > 0)
        {
            _levelText.text = $"Уровень {lvl}";

            if (mult == 1)
                _upgradeLabel.text = $"Уровень +";
            else
                _upgradeLabel.text = $"x{mult}";
        }
        else
        {
            _levelText.text = "Не куплен";

            if (mult == 1)
                _upgradeLabel.text = $"Купить";
            else
                _upgradeLabel.text = $"x{mult}";
        }

        double cost = _heroService.GetUpgradeCost(_index);
        _costText.text = NumberFormatter.Format(cost);

        double power = _heroService.GetPower(_index);
        if (lvl <= 0)
            power = def.BasePower;

        if (def.IsClickHero)
            _powerText.text = $"{NumberFormatter.Format(power)} Урон клика";
        else
            _powerText.text = $"{NumberFormatter.Format(power)} УВС";

        _upgradeButton.interactable = _heroService.CanAffordUpgarade(_index);
    }

}
