using System.Collections.Generic;
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
    [SerializeField] Button _skillsRowButton;
    [SerializeField] Transform _skillsContainer;
    [SerializeField] HeroSkillIconView _skillIconPrefab;

    private HeroSkillsPanelView _skillPanel;
    readonly List<HeroSkillIconView> _skillIconList = new();
    bool _skillsBuilt;

    private int _index;
    private HeroService _heroService;
    private CombatService _combatService;
    private GameState _gameState;
    private bool _isBound;

    public void Setup(int index, HeroService heroService,CombatService combatService,
        HeroSkillsPanelView heroSkillsPanelView, GameState gameState)
    {
        _index = index;
        _heroService = heroService;
        _combatService = combatService;
        _skillPanel = heroSkillsPanelView;
        _gameState = gameState;

        if (!_isBound)
        {
            _upgradeButton.onClick.AddListener(OnUpgradeClicked);
            _skillsRowButton.onClick.AddListener(OnSkillsRowClicked);
            _heroService.Upgraded+= Refresh;
            _heroService.MultiplierChanged += Refresh;
            _combatService.GoldChanged += Refresh;
            _isBound = true;
        }

        var definition = _heroService.GetDefinition(index);
        _icon.sprite = definition.Icon;
        _nameText.text = definition.Name;
        BuildSkillIconOnce();
        Refresh();
    }
    private void OnDestroy()
    {
        _heroService.Upgraded -= Refresh;
        _heroService.MultiplierChanged -= Refresh;
        _combatService.GoldChanged -= Refresh;
        _skillsRowButton.onClick.RemoveListener(OnSkillsRowClicked);
    }
    private void OnUpgradeClicked()
    {
        _heroService.TryUpgrade(_index);
    }
    private void Refresh()
    {
        int level = _heroService.GetLevel(_index);
        var definition = _heroService.GetDefinition(_index);
        int levelsToBuy = _heroService.UpgradeMultiplier;

        if (level > 0)
        {
            _levelText.text = $"Уровень {level}";

            if (levelsToBuy == 1)
                _upgradeLabel.text = $"Уровень +";
            else
                _upgradeLabel.text = $"x{levelsToBuy}";
        }
        else
        {
            _levelText.text = "Не куплен";

            if (levelsToBuy == 1)
                _upgradeLabel.text = $"Купить";
            else
                _upgradeLabel.text = $"x{levelsToBuy}";
        }

        double cost = _heroService.GetUpgradeCost(_index, levelsToBuy);
        _costText.text = NumberFormatter.Format(cost);
         
        double power = _heroService.GetPower(_index);
        if (level <= 0)
            power = definition.BasePower;

        if (definition.IsClickHero)
            _powerText.text = $"{NumberFormatter.Format(power)} Урон клика";
        else
            _powerText.text = $"{NumberFormatter.Format(power)} УВС";

        _upgradeButton.interactable = _heroService.CanAffordUpgrade(_index);

        RefreshSkills();
    }
    private void BuildSkillIconOnce()
    {
        if (_skillsBuilt) return;
        _skillsBuilt = true;

        int count = _heroService.GetSkillCount(_index);
        for (int s = 0; s < count; s++)
        {
            var view = Instantiate(_skillIconPrefab, _skillsContainer);
            var skill = _heroService.GetSkill(_index, s);
            view.Bind(skill.Icon);
            _skillIconList.Add(view);
        }
    }
    private void RefreshSkills()
    {
        int level = _heroService.GetLevel(_index);

        if (level <= 0)
        {
            _skillsRowButton.gameObject.SetActive(false);
            return;
        }

        _skillsRowButton.gameObject.SetActive(true);

        for (int i = 0; i < _skillIconList.Count; i++)
        {
            bool isUnlocked  = _heroService.IsSkillVisibleOnCard(_index, i);
            bool isOwned = _heroService.IsSkillOwned(_index, i);
            bool isCanAfford = _heroService.CanAffordSkill(_index, i);
            _skillIconList[i].RefreshState(isUnlocked, isOwned, isCanAfford);
        }
    }
    private void OnSkillsRowClicked()
    {
        if (_heroService.GetLevel(_index) <= 0) return;
        _skillPanel.Open(_index);
    }
}
